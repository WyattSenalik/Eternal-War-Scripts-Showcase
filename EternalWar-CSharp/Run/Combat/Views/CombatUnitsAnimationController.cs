// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    internal sealed class CombatUnitsAnimationController(ICombatViewActions combatViewActions)
    {
        private readonly Dictionary<CombatUnitUniqueIdentifier, UnitLoopAnimationController> _unitsControllerMap = new();
        private readonly Dictionary<(CombatUnitUniqueIdentifier, UnitLoopAnimationType), Guid> _autoMemoryLoopRequestMap = new();
        private readonly ICombatViewActions _combatViewActions = combatViewActions;

        public void Initialize(IEnumerable<CombatUnitUniqueIdentifier> unitIdentifiers)
        {
            Clear();
            foreach (CombatUnitUniqueIdentifier identifier in unitIdentifiers)
            {
                Add(identifier);
                _combatViewActions.SetUnitAnimationLoop(identifier, UnitLoopAnimationController.DefaultLoopRequest);
            }
        }

        public void Clear()
        {
            _unitsControllerMap.Clear();
            _autoMemoryLoopRequestMap.Clear();
        }

        public void Add(CombatUnitUniqueIdentifier unitIdentifier)
        {
            UnitLoopAnimationController unitController = new(_combatViewActions, unitIdentifier);
            _unitsControllerMap.Add(unitIdentifier, unitController);
        }

        public IReadOnlyPresentationTask PlayUnitOneShotAnimation(CombatUnitUniqueIdentifier unitIdentifier, UnitOneShotAnimationRequest animationRequest)
        {
            if (!TryGetUnitController(unitIdentifier, nameof(PlayUnitOneShotAnimation), out UnitLoopAnimationController? unitAnimationController))
                return PresentationTask.Instant;

            return unitAnimationController.PlayUnitOneShotAnimation(animationRequest);
        }

        public Guid PushUnitAnimationLoop(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationRequest animationRequest)
        {
            if (!TryGetUnitController(unitIdentifier, nameof(PushUnitAnimationLoop), out UnitLoopAnimationController? unitAnimationController))
                return Guid.Empty;

            return unitAnimationController.PushAnimationLoop(animationRequest);
        }

        public void PopUnitAnimationLoop(CombatUnitUniqueIdentifier unitIdentifier, Guid requestId)
        {
            if (!TryGetUnitController(unitIdentifier, nameof(PopUnitAnimationLoop), out UnitLoopAnimationController? unitAnimationController))
                return;

            unitAnimationController.PopAnimationLoop(requestId);
        }

        public void PushUnitAnimationLoop_AutoMemory(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationRequest animationRequest)
        {
            Guid id = PushUnitAnimationLoop(unitIdentifier, animationRequest);
            (CombatUnitUniqueIdentifier, UnitLoopAnimationType) key = (unitIdentifier, animationRequest.AnimationType);
            _autoMemoryLoopRequestMap.Add(key, id);
        }

        public void PopUnitAnimationLoop_AutoMemory(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationType loopType)
        {
            (CombatUnitUniqueIdentifier, UnitLoopAnimationType) key = (unitIdentifier, loopType);
            if (!_autoMemoryLoopRequestMap.TryGetValue(key, out Guid requestId))
                return;
            _autoMemoryLoopRequestMap.Remove(key);
            PopUnitAnimationLoop(unitIdentifier, requestId);
        }

        private bool TryGetUnitController(CombatUnitUniqueIdentifier unitIdentifier, string source, [NotNullWhen(true)] out UnitLoopAnimationController? unitAnimationController)
        {
            if (!_unitsControllerMap.TryGetValue(unitIdentifier, out unitAnimationController))
            {
                Debug.PrintError($"{source}: unit who does not exist ({unitIdentifier})");
                return false;
            }
            return true;
        }

        internal sealed class UnitLoopAnimationController(ICombatViewActions combatViewActions, CombatUnitUniqueIdentifier unitIdentifier)
        {
            public static readonly UnitLoopAnimationRequest DefaultLoopRequest = new(UnitLoopAnimationType.Idle, ActionTimingContext: null);

            public UnitLoopAnimationRequest ActiveLoopRequest { get; private set; } = DefaultLoopRequest;

            private readonly ICombatViewActions _combatViewActions = combatViewActions;
            private readonly CombatUnitUniqueIdentifier _unitIdentifier = unitIdentifier;
            private readonly List<(Guid Id, UnitLoopAnimationRequest Request)> _recentLoopRequests = new();
            private readonly List<IReadOnlyPresentationTask> _activeOneShotTasks = new();

            public Guid PushAnimationLoop(UnitLoopAnimationRequest animationRequest)
            {
                Guid id = Guid.NewGuid();
                _recentLoopRequests.Add((id, animationRequest));
                int newPriority = animationRequest.AnimationType.GetPriority();
                int existingPriority = ActiveLoopRequest.AnimationType.GetPriority();
                if (newPriority < existingPriority)
                    return id;

                // If it's the same as the current request, only refresh if dirty
                if (animationRequest == ActiveLoopRequest)
                    return id;

                // When equal, recency takes priority
                ActiveLoopRequest = animationRequest;

                // Only tell the engine to set the animation loop if there aren't current one shots playing
                if (_activeOneShotTasks.Count > 0)
                    return id;

                RefreshAnimationLoop();
                return id;
            }

            public void PopAnimationLoop(Guid requestId)
            {
                int foundIndex = _recentLoopRequests.FindIndex(item => item.Id == requestId);
                if (foundIndex < 0)
                    return;

                UnitLoopAnimationRequest animationRequest = _recentLoopRequests[foundIndex].Request;
                _recentLoopRequests.RemoveAt(foundIndex);

                if (ActiveLoopRequest != animationRequest)
                    return;

                // Determine the new highest priority animation request
                UnitLoopAnimationRequest newActiveLoopRequest = DefaultLoopRequest;
                foreach ((_, UnitLoopAnimationRequest candidateRequest) in _recentLoopRequests)
                {
                    int currentHighestPriority = newActiveLoopRequest.AnimationType.GetPriority();
                    int candidatePriority = candidateRequest.AnimationType.GetPriority();
                    // When equal, recency takes priority
                    if (candidatePriority >= currentHighestPriority)
                        newActiveLoopRequest = candidateRequest;
                }

                // If it's the same as the current request, no need to refresh
                if (newActiveLoopRequest == ActiveLoopRequest)
                    return;
                ActiveLoopRequest = newActiveLoopRequest;

                // Only tell the engine to set the animation loop if there aren't current one shots playing
                if (_activeOneShotTasks.Count > 0)
                    return;
                RefreshAnimationLoop();
            }

            public IReadOnlyPresentationTask PlayUnitOneShotAnimation(UnitOneShotAnimationRequest animationRequest)
            {
                IReadOnlyPresentationTask animationTask = _combatViewActions.PlayUnitOneShotAnimation(_unitIdentifier, ActiveLoopRequest, animationRequest);
                _activeOneShotTasks.Add(animationTask);
                return animationTask.ThenDo(() =>
                {
                    _activeOneShotTasks.Remove(animationTask);
                    if (_activeOneShotTasks.Count <= 0)
                        RefreshAnimationLoop();
                });
            }

            private void RefreshAnimationLoop()
            {
                _combatViewActions.SetUnitAnimationLoop(_unitIdentifier, ActiveLoopRequest);
            }
        }
    }
}
