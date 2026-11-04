// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using System;
using System.Numerics;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    // After that get rid of Update and create new function for when a full change happens.
    internal sealed class CombatView_World_Units(GameDatabase database, ICombatViewActions combatViewActions)
    {
        private const string SLEEP_NON_STACKABLE_EFFECT_ID = "Sleep"; // TODO: Make an animation system where non stackable effect ids have optional states defined in editor

        private readonly GameDatabase _database = database;
        private readonly ICombatViewActions _combatViewActions = combatViewActions;
        private readonly CombatUnitsAnimationController _unitsAnimationController = new(combatViewActions);

        public void Activate(CombatRuntime combatRuntime)
        {
            CombatInstance combatInstance = combatRuntime.Instance;
            TileGridInstance tileGridInstance = combatInstance.TileGrid;
            CombatState combatState = combatRuntime.State;
            string combatId = combatInstance.DefinitionId;

            ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild> unitMap = combatInstance.UnitMap;
            foreach (CombatUnitUniqueIdentifier unitIdentifier in unitMap.Keys)
            {
                UnitState unitState = combatState.GetUnit(unitIdentifier);
                GridPosition unitPosition = unitState.GridPosition;
                Vector3 worldPosition = tileGridInstance.GetWorldPosition(_database, combatId, unitPosition);

                _combatViewActions.CreateUnit(unitIdentifier, worldPosition, unitState.FacingDirection);
            }
            _unitsAnimationController.Initialize(unitMap.Keys);
        }

        public void Deactivate()
        {
            _unitsAnimationController.Clear();
            _combatViewActions.DeleteAllUnits();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            return gameEvent is CombatEvent combatEvent 
                ? HandleChange_CombatEvent(combatEvent, context)
                : ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult HandleChange_CombatEvent(CombatEvent combatEvent, ViewHandleChangeContext context)
        {
            return combatEvent switch
            {
                CombatEvent_AttackHit attackHitEvent => HandleChange_CombatEvent_AttackHit(attackHitEvent),
                CombatEvent_AttackMissed attackMissedEvent => HandleChange_CombatEvent_AttackMissed(attackMissedEvent),
                CombatEvent_AttackSwung attackSwungEvent => HandleChange_CombatEvent_AttackSwung(attackSwungEvent),
                CombatEvent_DamageApplied damagedAppliedEvent => HandleChange_CombatEvent_DamageApplied(damagedAppliedEvent, context),
                CombatEvent_FacingChanged facingChangedEvent => HandleChange_CombatEvent_FacingChanged(facingChangedEvent),
                CombatEvent_HealApplied healAppliedEvent => HandleChange_CombatEvent_HealApplied(healAppliedEvent, context),
                CombatEvent_Moved movedEvent => HandleChange_CombatEvent_Moved(movedEvent, context),
                CombatEvent_NonStackableEffectAdded nonStackableEffectAddedEvent => HandleChange_CombatEvent_NonStackableEffectAdded(nonStackableEffectAddedEvent, context),
                CombatEvent_NonStackableEffectRemoved nonStackableEffectRemovedEvent => HandleChange_CombatEvent_NonStackableEffectRemoved(nonStackableEffectRemovedEvent, context),
                CombatEvent_PassiveAdded passiveAddedEvent => HandleChange_CombatEvent_PassiveAdded(passiveAddedEvent, context),
                CombatEvent_PassiveRemoved passiveRemovedEvent => HandleChange_CombatEvent_PassiveRemoved(passiveRemovedEvent, context),
                CombatEvent_StatModifierApplied statModifierAddedEvent => HandleChange_CombatEvent_StatModifierApplied(statModifierAddedEvent, context),
                CombatEvent_StatModifierRemoved statModifierRemovedEvent => HandleChange_CombatEvent_StatModifierRemoved(statModifierRemovedEvent, context),
                CombatEvent_UnitDied unitDiedEvent => HandleChange_CombatEvent_UnitDied(unitDiedEvent),
                CombatEvent_Shoved shoveEvent => HandleChange_CombatEvent_Shoved(shoveEvent),
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        private ChangeHandledResult_Finished HandleChange_CombatEvent_AttackHit(CombatEvent_AttackHit combatEvent)
        {
            // If it was fatal, play the death animation instead of the flinch animation and do delay
            if (combatEvent.WasFatal)
            {
                // Let the UnitDied combat event handle this.
                return ChangeHandledResult_Finished.Instance;
            }

            HandleChange_CombatEvent_GenericPlayAnimation(combatEvent.TargetIdentifier, combatEvent, UnitOneShotAnimationType.Flinch);
            // Don't wait for the flinch animaiton before applying damage
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_OngoingTask HandleChange_CombatEvent_AttackMissed(CombatEvent_AttackMissed combatEvent) => HandleChange_CombatEvent_GenericPlayAnimation(combatEvent.TargetIdentifier, combatEvent, UnitOneShotAnimationType.Dodge);

        private ChangeHandledResult_OngoingTask HandleChange_CombatEvent_AttackSwung(CombatEvent_AttackSwung combatEvent) => HandleChange_CombatEvent_GenericUserPlayAnimation(combatEvent, UnitOneShotAnimationType.AttackSwing);

        private static ChangeHandledResult_Finished HandleChange_CombatEvent_DamageApplied(CombatEvent_DamageApplied damagedAppliedEvent, ViewHandleChangeContext context)
        {
            // TODO: Pop up numbers for damage applied
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleChange_CombatEvent_FacingChanged(CombatEvent_FacingChanged combatEvent)
        {
            CombatUnitUniqueIdentifier effectedUnitIdentifier = combatEvent.EffectedUnitIdentifier;

            _combatViewActions.SetUnitFacingDirection(effectedUnitIdentifier, combatEvent.ToDirection);

            return ChangeHandledResult_Finished.Instance;
        }

        private static ChangeHandledResult_Finished HandleChange_CombatEvent_HealApplied(CombatEvent_HealApplied healAppliedEvent, ViewHandleChangeContext context)
        {
            // TODO: Pop up numbers for heal applied
            // TODO: And animation for the user of the heal (make a separate combat event for that?)?
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult HandleChange_CombatEvent_Moved(CombatEvent_Moved combatEvent, ViewHandleChangeContext context)
        {
            // The action user and the unit being moved are different for effects such as Push.
            CombatUnitUniqueIdentifier movedUnitIdentifier = combatEvent.EffectedUnitIdentifier;
            ActionSequenceTimingContext actionTimingContext = combatEvent.ActionTimingContext;
            MovementType movementType = combatEvent.MovementType;
            GridPosition toGridPosition = combatEvent.ToPosition;
            ClientGameState currentClientGameState = context.CurrentClientGameState;
            Vector3 toWorldPosition = currentClientGameState.GetWorldPosition(_database, toGridPosition);

            return movementType switch
            {
                MovementType.Teleport => _Teleport(),
                MovementType.Walk => _Walk(),
                MovementType.Push => _Push(),
                _ => throw new NotImplementedException($"Unhandled {nameof(MovementType)}: ({movementType})")
            };


            ChangeHandledResult _Teleport()
            {
                _combatViewActions.SetUnitPosition(movedUnitIdentifier, toWorldPosition);
                return ChangeHandledResult_Finished.Instance;
            }
            ChangeHandledResult _Walk() => _AnimatedSlide(UnitLoopAnimationType.Walking);
            ChangeHandledResult _Push() => _AnimatedSlide(UnitLoopAnimationType.Pushed);

            ChangeHandledResult _AnimatedSlide(UnitLoopAnimationType loopType)
            {
                IReadOnlyPresentationTask moveUnitTask = _combatViewActions.SlideUnit(movedUnitIdentifier, toWorldPosition);

                if (combatEvent.IsFirstMove)
                {
                    // Movement needs to be visible during the slide, even if the previous action's one-shot is still playing. The default/idle loop remains queued behind one-shots.
                    UnitLoopAnimationRequest loopAnimationRequest = new(loopType, actionTimingContext, InterruptsCurrentOneShot: true);
                    _unitsAnimationController.PushUnitAnimationLoop_AutoMemory(movedUnitIdentifier, loopAnimationRequest);
                }

                if (combatEvent.IsLastMove)
                {
                    moveUnitTask = moveUnitTask.ThenDo(() => _unitsAnimationController.PopUnitAnimationLoop_AutoMemory(movedUnitIdentifier, loopType));
                }

                return new ChangeHandledResult_OngoingTask(moveUnitTask);
            }
        }

        private ChangeHandledResult HandleChange_CombatEvent_NonStackableEffectAdded(CombatEvent_NonStackableEffectAdded combatEvent, ViewHandleChangeContext context)
        {
            string effectId = combatEvent.EffectId;
            CombatUnitUniqueIdentifier targetUnitIdentifier = combatEvent.TargetIdentifier;
            ActionSequenceTimingContext actionTimingContext = combatEvent.ActionTimingContext;

            // TODO: idk some vfx

            // TODO: Make a better system for this (in a todo at the top near the const)
            bool isSleep = effectId == SLEEP_NON_STACKABLE_EFFECT_ID;
            if (isSleep)
            {
                UnitOneShotAnimationRequest fallAsleepRequest = new(UnitOneShotAnimationType.FallAsleep, actionTimingContext);
                IReadOnlyPresentationTask fallAsleepTask = _unitsAnimationController.PlayUnitOneShotAnimation(targetUnitIdentifier, fallAsleepRequest);
                fallAsleepTask.ThenDo(() =>
                {
                    // Make action timing context null so that when we try to end this in a separate event, it works fine.
                    UnitLoopAnimationRequest asleepLoopRequest = new(UnitLoopAnimationType.Asleep, ActionTimingContext: null);
                    _unitsAnimationController.PushUnitAnimationLoop_AutoMemory(targetUnitIdentifier, asleepLoopRequest);
                });
                return new ChangeHandledResult_OngoingTask(fallAsleepTask);
            }

            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult HandleChange_CombatEvent_NonStackableEffectRemoved(CombatEvent_NonStackableEffectRemoved combatEvent, ViewHandleChangeContext context)
        {
            string effectId = combatEvent.EffectId;
            CombatUnitUniqueIdentifier targetUnitIdentifier = combatEvent.TargetIdentifier;
            ActionSequenceTimingContext actionTimingContext = combatEvent.ActionTimingContext;

            // TODO: idk some vfx

            // TODO: Make a better system for this (in a todo at the top near the const)
            bool isSleep = effectId == SLEEP_NON_STACKABLE_EFFECT_ID;
            if (isSleep)
            {
                _unitsAnimationController.PopUnitAnimationLoop_AutoMemory(targetUnitIdentifier, UnitLoopAnimationType.Asleep);
                UnitOneShotAnimationRequest wakeUpRequest = new(UnitOneShotAnimationType.WakeUp, actionTimingContext);
                IReadOnlyPresentationTask wakeUpTask = _unitsAnimationController.PlayUnitOneShotAnimation(targetUnitIdentifier, wakeUpRequest);

                return new ChangeHandledResult_OngoingTask(wakeUpTask);
            }

            return ChangeHandledResult_Finished.Instance;
        }

        private static ChangeHandledResult_Finished HandleChange_CombatEvent_PassiveAdded(CombatEvent_PassiveAdded passiveAddedEvent, ViewHandleChangeContext context)
        {
            // TODO: idk some vfx
            return ChangeHandledResult_Finished.Instance;
        }

        private static ChangeHandledResult_Finished HandleChange_CombatEvent_PassiveRemoved(CombatEvent_PassiveRemoved passiveRemovedEvent, ViewHandleChangeContext context)
        {
            // TODO: idk some vfx
            return ChangeHandledResult_Finished.Instance;
        }

        private static ChangeHandledResult_Finished HandleChange_CombatEvent_StatModifierApplied(CombatEvent_StatModifierApplied statModifierAddedEvent, ViewHandleChangeContext context)
        {
            // TODO: idk some vfx
            return ChangeHandledResult_Finished.Instance;
        }

        private static ChangeHandledResult_Finished HandleChange_CombatEvent_StatModifierRemoved(CombatEvent_StatModifierRemoved statModifierRemovedEvent, ViewHandleChangeContext context)
        {
            // TODO: idk some vfx
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_OngoingTask HandleChange_CombatEvent_UnitDied(CombatEvent_UnitDied unitDiedEvent)
        {
            CombatUnitUniqueIdentifier deadUnitIdentifier = unitDiedEvent.DeadUnitIdentifier;
            ActionSequenceTimingContext actionTimingContext = unitDiedEvent.ActionTimingContext;
            UnitOneShotAnimationRequest dieOneShotRequest = new(UnitOneShotAnimationType.Die, actionTimingContext);

            IReadOnlyPresentationTask dieAnimationTask = _unitsAnimationController.PlayUnitOneShotAnimation(deadUnitIdentifier, dieOneShotRequest);
            dieAnimationTask.ThenDo(() =>
            {
                // Can't be revived, so get destroy the unit
                UnitDefinition deadUnitDefinition = _database.GetUnitDefinition(deadUnitIdentifier);
                if (!deadUnitDefinition.CanBeRevived)
                {
                    _combatViewActions.DeleteUnit(deadUnitIdentifier);
                    return;
                }

                // Can be revived, so play a dead loop
                UnitLoopAnimationRequest deadLoopRequest = new(UnitLoopAnimationType.Dead, actionTimingContext);
                _unitsAnimationController.PushUnitAnimationLoop_AutoMemory(deadUnitIdentifier, deadLoopRequest);
            });

            return new ChangeHandledResult_OngoingTask(dieAnimationTask);
        }

        private ChangeHandledResult_OngoingTask HandleChange_CombatEvent_Shoved(CombatEvent_Shoved combatEvent) => HandleChange_CombatEvent_GenericUserPlayAnimation(combatEvent, UnitOneShotAnimationType.Push);

        private ChangeHandledResult_OngoingTask HandleChange_CombatEvent_GenericUserPlayAnimation(CombatEvent_CombatStep combatEvent, UnitOneShotAnimationType animationType) => HandleChange_CombatEvent_GenericPlayAnimation(combatEvent.UserIdentifier, combatEvent, animationType);

        private ChangeHandledResult_OngoingTask HandleChange_CombatEvent_GenericPlayAnimation(CombatUnitUniqueIdentifier unitIdentifier, CombatEvent_CombatStep combatEvent, UnitOneShotAnimationType animationType)
        {
            ActionSequenceTimingContext actionTimingContext = combatEvent.ActionTimingContext;

            UnitOneShotAnimationRequest animationRequest = new(animationType, actionTimingContext);
            IReadOnlyPresentationTask unitAnimationTask = _unitsAnimationController.PlayUnitOneShotAnimation(unitIdentifier, animationRequest);

            return new ChangeHandledResult_OngoingTask(unitAnimationTask);
        }
    }
}
