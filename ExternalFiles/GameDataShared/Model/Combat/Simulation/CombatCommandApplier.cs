// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Simulation
{
    public static class CombatCommandApplier
    {
        public static ValueArray<CombatEvent> CreateEvents(this CombatCommand combatCommand)
        {
            return combatCommand switch
            {
                CombatCommand_BeginAction beginActionCommand => CreateEvents_BeginAction(beginActionCommand),
                CombatCommand_ResolveAttackHit attackHitCommand => CreateEvents_ResolveAttackHit(attackHitCommand),
                CombatCommand_ReducePassiveStacks reducePassiveStacksCommand => CreateEvents_ReducePassiveStacks(reducePassiveStacksCommand),
                _ => One(combatCommand switch
                {
                    CombatCommand_CompleteAction => CombatEvent_ActionEnded.Instance,
                    CombatCommand_BeginActionStep command => new CombatEvent_StepStarted(command.UserIdentifier, command.ActionTimingContext),
                    CombatCommand_CompleteActionStep command => new CombatEvent_StepEnded(command.UserIdentifier, command.ActionTimingContext),
                    CombatCommand_PerformAttack command => new CombatEvent_AttackSwung(command.UserIdentifier, command.ActionTimingContext, command.TargetPositions),
                    CombatCommand_ResolveAttackMiss command => new CombatEvent_AttackMissed(command.UserIdentifier, command.ActionTimingContext, command.TargetIdentifier),
                    CombatCommand_NotifyUnitDied command => new CombatEvent_UnitDied(command.UserIdentifier, command.ActionTimingContext, command.DeadUnitIdentifier),
                    CombatCommand_MoveUnit command => new CombatEvent_Moved(command.UserIdentifier, command.ActionTimingContext, command.EffectedUnitIdentifier, command.FromPosition, command.ToPosition, command.MovementType, command.IsFirstMove, command.IsLastMove),
                    CombatCommand_ChangeFacing command => new CombatEvent_FacingChanged(command.UserIdentifier, command.ActionTimingContext, command.EffectedUnitIdentifier, command.FromDirection, command.ToDirection),
                    CombatCommand_ApplyHealing command => new CombatEvent_HealApplied(command.UserIdentifier, command.ActionTimingContext, command.SourceIdentifier, command.TargetIdentifier, command.HealthRestored, command.ResultingHealth),
                    CombatCommand_ApplyStatModifier command => new CombatEvent_StatModifierApplied(command.UserIdentifier, command.ActionTimingContext, command.SourceIdentifier, command.TargetIdentifier, command.Modifier),
                    CombatCommand_RemoveStatModifier command => new CombatEvent_StatModifierRemoved(command.UserIdentifier, command.ActionTimingContext, command.TargetIdentifier, command.Modifier),
                    CombatCommand_AddPassive command => new CombatEvent_PassiveAdded(command.UserIdentifier, command.ActionTimingContext, command.SourceIdentifier, command.TargetIdentifier, command.PassiveId, command.StacksAdded, command.ResultingPassive),
                    CombatCommand_ActivatePassive command => new CombatEvent_PassiveActivated(command.OwnerIdentifier, command.SourceIdentifier, command.PassiveId, command.ActivateTime, command.ResultingPassive),
                    CombatCommand_ProgressPassiveExpiration command => new CombatEvent_PassiveExpirationProgressed(command.OwnerIdentifier, command.PassiveId, command.ResultingPassive),
                    CombatCommand_StartUnitTurn command => new CombatEvent_UnitTurnStarted(command.UnitIdentifier),
                    CombatCommand_EndUnitTurn command => new CombatEvent_UnitTurnEnded(command.UnitIdentifier),
                    CombatCommand_RefreshUnitTurn command => new CombatEvent_UnitTurnRefreshed(command.UnitIdentifier, command.ResultingActionPoints),
                    CombatCommand_AddNonStackableEffect command => new CombatEvent_NonStackableEffectAdded(command.UserIdentifier, command.ActionTimingContext, command.SourceIdentifier, command.TargetIdentifier, command.EffectId),
                    CombatCommand_RemoveNonStackableEffect command => new CombatEvent_NonStackableEffectRemoved(command.UserIdentifier, command.ActionTimingContext, command.SourceIdentifier, command.TargetIdentifier, command.EffectId),
                    CombatCommand_PerformShove command => new CombatEvent_Shoved(command.UserIdentifier, command.ActionTimingContext),
                    _ => throw new NotImplementedException($"Unhandled {nameof(CombatCommand)}: ({combatCommand.GetType()})")
                })
            };
        }
        private static ValueArray<CombatEvent> CreateEvents_BeginAction(CombatCommand_BeginAction combatCommand)
        {
            combatCommand.Deconstruct(out CombatUnitUniqueIdentifier userIdentifier, out int actionPointsSpent, out int resultingActionPoints);

            if (actionPointsSpent <= 0)
                return One(CombatEvent_ActionStarted.Instance);

            CombatEvent_ActionPointsSpent actionPointsSpentEvent = new(userIdentifier, actionPointsSpent, resultingActionPoints);
            return new ValueArray<CombatEvent>(CombatEvent_ActionStarted.Instance, actionPointsSpentEvent);
        }
        private static ValueArray<CombatEvent> CreateEvents_ResolveAttackHit(CombatCommand_ResolveAttackHit combatCommand)
        {
            combatCommand.Deconstruct(out CombatUnitUniqueIdentifier userIdentifier, out ActionSequenceTimingContext timingContext, out CombatUnitUniqueIdentifier targetIdentifier, out bool wasCrit, out bool wasFatal, out DamageType damageType, out int damageDealt, out int resultingHealth);

            CombatEvent_AttackHit hitEvent = new(userIdentifier, timingContext, targetIdentifier, wasCrit, wasFatal);
            CombatEvent_DamageApplied damageEvent = new(userIdentifier, timingContext, userIdentifier, targetIdentifier, damageType, damageDealt, resultingHealth, wasCrit);
            return new ValueArray<CombatEvent>(hitEvent, damageEvent);
        }
        private static ValueArray<CombatEvent> CreateEvents_ReducePassiveStacks(CombatCommand_ReducePassiveStacks combatCommand)
        {
            combatCommand.Deconstruct(out CombatUnitUniqueIdentifier ownerIdentifier, out string passiveId, out int stacksReduced, out PassiveState? resultingPassive);

            CombatEvent_PassiveStacksReduced stacksReducedEvent = new(ownerIdentifier, passiveId, stacksReduced, resultingPassive);
            if (resultingPassive == null)
            {
                CombatEvent_PassiveRemoved passiveRemovedEvent = new(ownerIdentifier, passiveId);
                return new ValueArray<CombatEvent>(stacksReducedEvent, passiveRemovedEvent);
            }
            return One(stacksReducedEvent);
        }

        public static CombatState ApplyCommand(this CombatCommand combatCommand, CombatState combatState) => ApplyCommand(combatState, combatCommand);
        public static CombatState ApplyCommand(this CombatState combatState, CombatCommand combatCommand)
        {
            return combatCommand switch
            {
                // Commands whose observer output is their only direct combat-state effect.
                CombatCommand_CompleteAction => combatState,
                CombatCommand_BeginActionStep => combatState,
                CombatCommand_CompleteActionStep => combatState,
                CombatCommand_PerformAttack => combatState,
                CombatCommand_ResolveAttackMiss => combatState,
                CombatCommand_NotifyUnitDied => combatState,
                CombatCommand_StartUnitTurn => combatState,
                CombatCommand_PerformShove => combatState,

                // State-mutating commands
                CombatCommand_BeginAction command_beginAction => ApplyCommand_BeginAction(combatState, command_beginAction),
                CombatCommand_EndUnitTurn command_unitTurnEnded => ApplyCommand_UnitTurnEnded(combatState, command_unitTurnEnded),
                CombatCommand_RefreshUnitTurn command_unitTurnRefreshed => ApplyCommand_UnitTurnRefreshed(combatState, command_unitTurnRefreshed),
                CombatCommand_MoveUnit command_moved => ApplyCommand_Moved(combatState, command_moved),
                CombatCommand_ChangeFacing command_facingChanged => ApplyCommand_FacingChanged(combatState, command_facingChanged),
                CombatCommand_ResolveAttackHit command_attackHit => ApplyCommand_ResolveAttackHit(combatState, command_attackHit),
                CombatCommand_ApplyHealing command_healApplied => ApplyCommand_HealApplied(combatState, command_healApplied),
                CombatCommand_ApplyStatModifier command_statModifierApplied => ApplyCommand_StatModifierApplied(combatState, command_statModifierApplied),
                CombatCommand_RemoveStatModifier command_statModifierRemoved => ApplyCommand_StatModifierRemoved(combatState, command_statModifierRemoved),
                CombatCommand_AddPassive command_passiveAdded => ApplyCommand_PassiveAdded(combatState, command_passiveAdded),
                CombatCommand_ActivatePassive command_passiveActivated => ApplyCommand_PassiveActivated(combatState, command_passiveActivated),
                CombatCommand_ProgressPassiveExpiration command_passiveExpirationProgressed => ApplyCommand_PassiveExpirationProgressed(combatState, command_passiveExpirationProgressed),
                CombatCommand_ReducePassiveStacks command_passiveStacksReduced => ApplyCommand_PassiveStacksReduced(combatState, command_passiveStacksReduced),
                CombatCommand_AddNonStackableEffect command_nonStackableEffectAdded => ApplyCommand_NonStackableEffectAdded(combatState, command_nonStackableEffectAdded),
                CombatCommand_RemoveNonStackableEffect command_nonStackableEffectRemoved => ApplyCommand_NonStackableEffectRemoved(combatState, command_nonStackableEffectRemoved),

                _ => throw new NotImplementedException($"Unhandled {nameof(CombatCommand)}: ({combatCommand.GetType()})")
            };
        }

        private static CombatState ApplyCommand_BeginAction(CombatState combatState, CombatCommand_BeginAction beginActionCommand)
        {
            CombatUnitUniqueIdentifier userIdentifier = beginActionCommand.UserIdentifier;

            return combatState.TransformUnit(userIdentifier, oldUnitState => oldUnitState with
            {
                CurrentActionPoints = beginActionCommand.ResultingActionPoints
            });
        }

        private static CombatState ApplyCommand_Moved(CombatState combatState, CombatCommand_MoveUnit movedCommand)
        {
            CombatUnitUniqueIdentifier effectedUnitIdentifier = movedCommand.EffectedUnitIdentifier;

            return combatState.TransformUnit(effectedUnitIdentifier, oldUnitState => oldUnitState with
            {
                GridPosition = movedCommand.ToPosition
            });
        }

        private static CombatState ApplyCommand_FacingChanged(CombatState combatState, CombatCommand_ChangeFacing facingChangedCommand)
        {
            CombatUnitUniqueIdentifier effectedUnitIdentifier = facingChangedCommand.EffectedUnitIdentifier;

            return combatState.TransformUnit(effectedUnitIdentifier, oldUnitState => oldUnitState with
            {
                FacingDirection = facingChangedCommand.ToDirection
            });
        }

        private static CombatState ApplyCommand_ResolveAttackHit(CombatState combatState, CombatCommand_ResolveAttackHit command)
        {
            CombatUnitUniqueIdentifier targetIdentifier = command.TargetIdentifier;
            int resultingHealth = command.ResultingHealth;
            CombatUnitUniqueIdentifier userIdentifier = command.UserIdentifier;

            return combatState.TransformUnit(targetIdentifier, oldUnitState => oldUnitState with
            {
                CurrentHealth = resultingHealth,
                UnitIdentifierWhoMostRecentlyDamagedMe = userIdentifier
            });
        }

        private static CombatState ApplyCommand_HealApplied(CombatState combatState, CombatCommand_ApplyHealing healAppliedCommand)
        {
            CombatUnitUniqueIdentifier targetIdentifier = healAppliedCommand.TargetIdentifier;

            return combatState.TransformUnit(targetIdentifier, oldUnitState => oldUnitState with
            {
                CurrentHealth = healAppliedCommand.ResultingHealth
            });
        }

        private static CombatState ApplyCommand_StatModifierApplied(CombatState combatState, CombatCommand_ApplyStatModifier statModifierAppliedCommand)
        {
            CombatUnitUniqueIdentifier targetIdentifier = statModifierAppliedCommand.TargetIdentifier;
            TemporaryStatModifierState modifier = statModifierAppliedCommand.Modifier;

            return combatState.TransformUnit(targetIdentifier, oldUnitState =>
            {
                ValueArray<TemporaryStatModifierState> oldTemporaryStateModifiers = oldUnitState.TemporaryStatModifiers;
                return oldUnitState with
                {
                    TemporaryStatModifiers = oldTemporaryStateModifiers.Add(modifier)
                };
            });
        }

        private static CombatState ApplyCommand_StatModifierRemoved(CombatState combatState, CombatCommand_RemoveStatModifier statModifierRemovedCommand)
        {
            CombatUnitUniqueIdentifier targetIdentifier = statModifierRemovedCommand.TargetIdentifier;
            TemporaryStatModifierState modifier = statModifierRemovedCommand.Modifier;

            return combatState.TransformUnit(targetIdentifier, oldUnitState =>
            {
                ValueArray<TemporaryStatModifierState> oldTemporaryStateModifiers = oldUnitState.TemporaryStatModifiers;
                int index = oldTemporaryStateModifiers.IndexOf(modifier);
                if (index < 0)
                    return oldUnitState;

                return oldUnitState with
                {
                    TemporaryStatModifiers = oldTemporaryStateModifiers.RemoveAt(index)
                };
            });
        }

        private static CombatState ApplyCommand_PassiveAdded(CombatState combatState, CombatCommand_AddPassive passiveAddedCommand)
        {
            CombatUnitUniqueIdentifier targetIdentifier = passiveAddedCommand.TargetIdentifier;
            string passiveId = passiveAddedCommand.PassiveId;
            PassiveState passive = passiveAddedCommand.ResultingPassive;

            return combatState.TransformUnit(targetIdentifier, oldUnitState =>
            {
                ValueArray<PassiveState> oldPassives = oldUnitState.ActivePassives;
                int existingIndex = oldPassives.FindIndexOfPassive(passiveId);

                ValueArray<PassiveState> newPassives = existingIndex < 0 
                    ? oldPassives.Add(passive) 
                    : oldPassives.ReplaceAt(existingIndex, passive);

                return oldUnitState with
                {
                    ActivePassives = newPassives
                };
            });
        }

        private static CombatState ApplyCommand_PassiveActivated(CombatState combatState, CombatCommand_ActivatePassive passiveActivatedCommand)
        {
            CombatUnitUniqueIdentifier ownerIdentifier = passiveActivatedCommand.OwnerIdentifier;
            string passiveId = passiveActivatedCommand.PassiveId;
            PassiveState resultingPassive = passiveActivatedCommand.ResultingPassive;

            return combatState.TransformUnit(ownerIdentifier, oldUnitState =>
            {
                ValueArray<PassiveState> oldPassives = oldUnitState.ActivePassives;
                int existingIndex = oldPassives.FindIndexOfPassive(passiveId);
                if (existingIndex < 0)
                    return oldUnitState;

                return oldUnitState with
                {
                    ActivePassives = oldPassives.ReplaceAt(existingIndex, resultingPassive)
                };
            });
        }

        private static ValueArray<CombatEvent> One(CombatEvent combatEvent) => new(new[] { combatEvent });

        private static CombatState ApplyCommand_UnitTurnEnded(CombatState combatState, CombatCommand_EndUnitTurn unitTurnEndedCommand)
        {
            return combatState.TransformUnit(unitTurnEndedCommand.UnitIdentifier, oldUnitState => oldUnitState with
            {
                HasTakenTurn = true
            });
        }

        private static CombatState ApplyCommand_UnitTurnRefreshed(CombatState combatState, CombatCommand_RefreshUnitTurn unitTurnRefreshedCommand)
        {
            return combatState.TransformUnit(unitTurnRefreshedCommand.UnitIdentifier, oldUnitState => oldUnitState with
            {
                HasTakenTurn = false,
                CurrentActionPoints = unitTurnRefreshedCommand.ResultingActionPoints
            });
        }
        private static CombatState ApplyCommand_PassiveExpirationProgressed(CombatState combatState, CombatCommand_ProgressPassiveExpiration progressCommand)
        {
            return combatState.TransformUnit(progressCommand.OwnerIdentifier, oldUnitState =>
            {
                ValueArray<PassiveState> oldPassives = oldUnitState.ActivePassives;
                int existingIndex = oldPassives.FindIndexOfPassive(progressCommand.PassiveId);
                return existingIndex < 0
                    ? oldUnitState
                    : oldUnitState with { ActivePassives = oldPassives.ReplaceAt(existingIndex, progressCommand.ResultingPassive) };
            });
        }

        private static CombatState ApplyCommand_PassiveStacksReduced(CombatState combatState, CombatCommand_ReducePassiveStacks stacksReducedCommand)
        {
            return combatState.TransformUnit(stacksReducedCommand.OwnerIdentifier, oldUnitState =>
            {
                ValueArray<PassiveState> oldPassives = oldUnitState.ActivePassives;
                int existingIndex = oldPassives.FindIndexOfPassive(stacksReducedCommand.PassiveId);
                if (existingIndex < 0)
                    return oldUnitState;

                ValueArray<PassiveState> newPassives = stacksReducedCommand.ResultingPassive == null
                    ? oldPassives.RemoveAt(existingIndex)
                    : oldPassives.ReplaceAt(existingIndex, stacksReducedCommand.ResultingPassive);
                return oldUnitState with { ActivePassives = newPassives };
            });
        }

        private static CombatState ApplyCommand_NonStackableEffectAdded(CombatState combatState, CombatCommand_AddNonStackableEffect nonStackableEffectAddedCommand)
        {
            CombatUnitUniqueIdentifier targetIdentifier = nonStackableEffectAddedCommand.TargetIdentifier;
            string effectId = nonStackableEffectAddedCommand.EffectId;

            return combatState.TransformUnit(targetIdentifier, oldUnitState =>
            {
                ValueArray<string> oldEffectIds = oldUnitState.ActiveNonStackableEffectIds;
                if (oldEffectIds.Contains(effectId))
                    return oldUnitState;

                return oldUnitState with
                {
                    ActiveNonStackableEffectIds = oldEffectIds.Add(effectId)
                };
            });
        }

        private static CombatState ApplyCommand_NonStackableEffectRemoved(CombatState combatState, CombatCommand_RemoveNonStackableEffect nonStackableEffectRemovedCommand)
        {
            CombatUnitUniqueIdentifier targetIdentifier = nonStackableEffectRemovedCommand.TargetIdentifier;
            string effectId = nonStackableEffectRemovedCommand.EffectId;

            return combatState.TransformUnit(targetIdentifier, oldUnitState =>
            {
                ValueArray<string> oldEffectIds = oldUnitState.ActiveNonStackableEffectIds;
                int existingIndex = oldEffectIds.IndexOf(effectId);
                if (existingIndex < 0)
                    return oldUnitState;

                return oldUnitState with
                {
                    ActiveNonStackableEffectIds = oldEffectIds.RemoveAt(existingIndex)
                };
            });
        }
    }
}

