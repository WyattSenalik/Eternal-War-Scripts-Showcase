// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Events
{
    public static class CombatEventAccessors
    {
        public static CombatUnitUniqueIdentifier? GetEffectedUnit(this CombatEvent combatEvent)
        {
            return combatEvent switch
            {
                // Structural / Presentation Events (no effect)
                CombatEvent_ActionStarted => null,
                CombatEvent_ActionEnded => null,
                CombatEvent_StepStarted => null,
                CombatEvent_StepEnded => null,
                CombatEvent_AttackSwung => null,
                CombatEvent_AttackMissed => null,
                CombatEvent_AttackHit => null,
                CombatEvent_UnitDied event_unitDied => event_unitDied.DeadUnitIdentifier,
                CombatEvent_UnitTurnStarted event_unitTurnStarted => event_unitTurnStarted.UnitIdentifier,
                CombatEvent_Shoved event_shoved => null,
                // State mutating events
                CombatEvent_ActionPointsSpent event_actionPointsSpent => event_actionPointsSpent.EffectedUnitIdentifier,
                CombatEvent_Moved event_moved => event_moved.EffectedUnitIdentifier,
                CombatEvent_FacingChanged event_facingChanged => event_facingChanged.EffectedUnitIdentifier,
                CombatEvent_DamageApplied event_damageApplied => event_damageApplied.TargetIdentifier,
                CombatEvent_HealApplied event_healApplied => event_healApplied.TargetIdentifier,
                CombatEvent_StatModifierApplied event_statModifierApplied => event_statModifierApplied.TargetIdentifier,
                CombatEvent_StatModifierRemoved event_statModifierRemoved => event_statModifierRemoved.TargetIdentifier,
                CombatEvent_PassiveAdded event_passiveAdded => event_passiveAdded.TargetIdentifier,
                CombatEvent_PassiveRemoved event_passiveRemoved => event_passiveRemoved.EffectedUnitIdentifier,
                CombatEvent_PassiveActivated event_passiveActivated => event_passiveActivated.OwnerIdentifier,
                CombatEvent_PassiveExpirationProgressed event_passiveExpirationProgressed => event_passiveExpirationProgressed.OwnerIdentifier,
                CombatEvent_PassiveStacksReduced event_passiveStacksReduced => event_passiveStacksReduced.OwnerIdentifier,
                CombatEvent_NonStackableEffectAdded event_nonStackableEffectAdded => event_nonStackableEffectAdded.TargetIdentifier,
                CombatEvent_NonStackableEffectRemoved event_nonStackableEffectRemoved => event_nonStackableEffectRemoved.TargetIdentifier,
                CombatEvent_UnitTurnEnded event_unitTurnEnded => event_unitTurnEnded.UnitIdentifier,
                CombatEvent_UnitTurnRefreshed event_unitTurnRefreshed => event_unitTurnRefreshed.UnitIdentifier,

                _ => throw new NotImplementedException($"Unhandled {nameof(CombatEvent)}: ({combatEvent.GetType()})")
            };
        }
    }
}
