// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using EternalWar.Engine.Godot.Run.Combat.Resources;
using Godot;
using GodotExtensions;
using GodotExtensions.Coroutine;
using GodotExtensions.GameDataShared;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NumericsVector3 = System.Numerics.Vector3;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatView_World_Units(GodotGameVisualDatabase visualDatabase, GodotCombatNodeCollection combatNodeCollectionInstance, GodotEngineRandomResolver engineRandomResolver)
    {
        private const float SLIDE_SPEED = 3.0f; // Godot Units per second

        private readonly Dictionary<CombatUnitUniqueIdentifier, GodotCombatUnitView> _unitsMap = new();
        private readonly GodotGameVisualDatabase _visualDatabase = visualDatabase;
        private readonly GodotCombatNodeCollection _combatNodeCollectionInstance = combatNodeCollectionInstance;
        private readonly GodotEngineRandomResolver _engineRandomResolver = engineRandomResolver;

        public void CreateUnit(CombatUnitUniqueIdentifier unitIdentifier, NumericsVector3 position, EightDirection facingDirection)
        {
            string unitDefinitionId = unitIdentifier.DefinitionId;

            GodotUnitVisualResource unitVisualResource = _visualDatabase.GetUnit(unitDefinitionId);

            PackedScene animationPrefab = unitVisualResource.AnimationPrefab;
            Node3D unitVisualInstance = animationPrefab.Instantiate<Node3D>();

            Node unitsParentNode = _combatNodeCollectionInstance.UnitsParentNode;
            unitsParentNode.AddChild(unitVisualInstance);

            // Position
            unitVisualInstance.GlobalPosition = position.ToGodot();

            // Rotation
            unitVisualInstance.SetFacingDirection(facingDirection);

            GodotCombatUnitView unitView = new(unitVisualInstance);
            _unitsMap.Add(unitIdentifier, unitView);
        }

        public void SetUnitPosition(CombatUnitUniqueIdentifier unitIdentifier, NumericsVector3 position)
        {
            if (!TryGetUnitView_WithErrorHandling(unitIdentifier, nameof(SetUnitPosition), out GodotCombatUnitView? unitView))
                return;
            Node3D visualInstance = unitView.VisualInstance;
            visualInstance.GlobalPosition = position.ToGodot();
        }

        public void SetUnitFacingDirection(CombatUnitUniqueIdentifier unitIdentifier, EightDirection facingDirection)
        {
            if (!TryGetUnitView_WithErrorHandling(unitIdentifier, nameof(SetUnitFacingDirection), out GodotCombatUnitView? unitView))
                return;
            Node3D visualInstance = unitView.VisualInstance;
            visualInstance.SetFacingDirection(facingDirection);
        }

        public IReadOnlyPresentationTask PlayUnitOneShotAnimation(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationRequest activeLoopRequest, UnitOneShotAnimationRequest animationRequest)
        {
            if (!TryGetUnitView_WithErrorHandling(unitIdentifier, nameof(PlayUnitOneShotAnimation), out GodotCombatUnitView? unitView))
                return PresentationTask.Instant;
            AnimationPlayer? unitAnimator = unitView.AnimationPlayer;
            if (unitAnimator == null)
                return PresentationTask.Instant; // This unit does not animate

            GodotUnitVisualResource unitVisualResource = _visualDatabase.GetUnit(unitIdentifier.DefinitionId);
            GodotUnitAnimationSetResource? animationSet = unitVisualResource.AnimationSet;
            if (animationSet == null)
                return PresentationTask.Instant; // This unit does not animate
            if (!animationSet.TryFindBestFit(activeLoopRequest, animationRequest, out GodotUnitOneShotAnimationResource? bestFit))
            {
                Debug.PrintError($"Failed to find best fit for one-shot animation request ({animationRequest}) for unit visual resource ({unitIdentifier.DefinitionId})");
                return PresentationTask.Instant;
            }
            string animationName = bestFit.RollAnimation(_engineRandomResolver);
            if (string.IsNullOrEmpty(animationName))
            {
                Debug.PrintError($"Failed to get animation name from best fit resource ({bestFit.ResourcePath})");
                return PresentationTask.Instant;
            }

            Animation animation = unitAnimator.GetAnimation(animationName);
            if (animation == null)
                return PresentationTask.Instant;

            unitAnimator.Stop();
            unitAnimator.Play(animationName);

            PresentationTask waitUntilFinishedTask = new();
            float animationTime = bestFit.AdvanceTimeOverride ?? animation.Length;
            GodotCoroutineManager.Instance.InvokeAfterSeconds(() =>
            {
                waitUntilFinishedTask.IsFinished = true;
            }, animationTime);
            return waitUntilFinishedTask;
        }

        public void SetUnitAnimationLoop(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationRequest animationRequest)
        {
            if (!TryGetUnitView_WithErrorHandling(unitIdentifier, nameof(PlayUnitOneShotAnimation), out GodotCombatUnitView? unitView))
                return;
            AnimationPlayer? unitAnimator = unitView.AnimationPlayer;
            if (unitAnimator == null)
                return; // This unit does not animate

            GodotUnitVisualResource unitVisualResource = _visualDatabase.GetUnit(unitIdentifier.DefinitionId);
            GodotUnitAnimationSetResource? animationSet = unitVisualResource.AnimationSet;
            if (animationSet == null)
                return; // This unit does not animate
            if (!animationSet.TryFindBestFit(animationRequest, out GodotUnitLoopAnimationResource? bestFit))
            {
                Debug.PrintError($"Failed to find best fit for loop animation request ({animationRequest}) for unit visual resource ({unitIdentifier.DefinitionId})");
                return;
            }
            string animationName = bestFit.AnimationName;
            if (string.IsNullOrEmpty(animationName))
            {
                Debug.PrintError($"Failed to get animation name from best fit resource ({bestFit.ResourcePath})");
                return;
            }

            Animation animation = unitAnimator.GetAnimation(animationName);
            if (animation == null)
                return;

            string currentAnimationName = unitAnimator.CurrentAnimation;
            if (string.IsNullOrEmpty(currentAnimationName))
            {
                unitAnimator.Play(animationName);
                return;
            }
            // If the animation is another loop, interupt it, if it's a one-shot, let it finish the one shot first.
            Animation currentAnimation = unitAnimator.GetAnimation(currentAnimationName);
            bool currentAnimationIsOneShot = currentAnimation.LoopMode == Animation.LoopModeEnum.None;
            if (currentAnimationIsOneShot)
            {
                if (animationRequest.InterruptsCurrentOneShot)
                {
                    // Replace the active one-shot and any stale queued loop (usually idle) with the movement loop. When movement ends, the normal loop request restores idle.
                    unitAnimator.ClearQueue();
                    unitAnimator.Play(animationName);
                }
                else
                {
                    // We only use the queue for loop animations and if we queue multiple loop animations, we'd get stuck in the first one.
                    unitAnimator.ClearQueue();
                    unitAnimator.Queue(animationName);
                }
            }
            else // Current animation is a different loop
            {
                unitAnimator.Play(animationName);
            }
        }

        public IReadOnlyPresentationTask SlideUnit(CombatUnitUniqueIdentifier unitIdentifier, NumericsVector3 toPosition)
        {
            if (!TryGetUnitView_WithErrorHandling(unitIdentifier, nameof(SlideUnit), out GodotCombatUnitView? unitView))
                return PresentationTask.Instant;

            GodotCoroutineManager coroutineManager = GodotCoroutineManager.Instance;
            PresentationTask presentationTask = new();
            coroutineManager.StartCoroutine(_SlideCoroutine());

            return presentationTask;

            IEnumerator _SlideCoroutine()
            {
                Node3D unitVisualInstance = unitView.VisualInstance;
                Vector3 startingPosition = unitVisualInstance.GlobalPosition;
                Vector3 toPositionGodot = toPosition.ToGodot();

                float distance = startingPosition.DistanceTo(toPositionGodot);

                float elapsedTime = 0.0f;
                float timeToTake = distance / SLIDE_SPEED;
                while (elapsedTime < timeToTake)
                {
                    float t = elapsedTime / timeToTake;
                    Vector3 newPosition = startingPosition.Lerp(toPositionGodot, t);
                    unitVisualInstance.GlobalPosition = newPosition;

                    yield return null;

                    elapsedTime += coroutineManager.DeltaF;
                }
                unitVisualInstance.GlobalPosition = toPositionGodot;

                presentationTask.IsFinished = true;
            }
        }

        public void DeleteUnit(CombatUnitUniqueIdentifier unitIdentifier)
        {
            if (!_unitsMap.TryGetValue(unitIdentifier, out GodotCombatUnitView? unitView))
                return;
            _unitsMap.Remove(unitIdentifier);

            Node3D unitVisualInstance = unitView.VisualInstance;
            unitVisualInstance.QueueFree();
        }

        public void DeleteAllUnits()
        {
            foreach (GodotCombatUnitView unitView in _unitsMap.Values)
            {
                Node3D unitVisualInstance = unitView.VisualInstance;
                unitVisualInstance.QueueFree();
            }
            _unitsMap.Clear();
        }

        private bool TryGetUnitView_WithErrorHandling(CombatUnitUniqueIdentifier unitIdentifier, string source, [NotNullWhen(true)] out GodotCombatUnitView? unitView)
        {
            if (_unitsMap.TryGetValue(unitIdentifier, out unitView))
                return true;
            Debug.PrintError($"{source}: unit not in view's map ({unitIdentifier})");
            return false;
        }
    }
}
