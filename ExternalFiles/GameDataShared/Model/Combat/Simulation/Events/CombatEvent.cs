// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Simulation.Events
{
    public sealed record ActionSequenceTimingContext(string Id, int Step);

    public enum MovementType
    {
        Walk = 0,
        Push = 1,
        Teleport = 2
    }

    public enum MovedEventTiming { First, Middle, Last }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "eventType")]
    [JsonDerivedType(typeof(CombatEvent_ActionStarted), "ActionStarted")]
    [JsonDerivedType(typeof(CombatEvent_ActionEnded), "ActionEnded")]
    [JsonDerivedType(typeof(CombatEvent_StepStarted), "StepStarted")]
    [JsonDerivedType(typeof(CombatEvent_StepEnded), "StepEnded")]
    [JsonDerivedType(typeof(CombatEvent_AttackSwung), "AttackSwung")]
    [JsonDerivedType(typeof(CombatEvent_AttackMissed), "AttackMissed")]
    [JsonDerivedType(typeof(CombatEvent_AttackHit), "AttackHit")]
    [JsonDerivedType(typeof(CombatEvent_UnitDied), "UnitDied")]
    [JsonDerivedType(typeof(CombatEvent_ActionPointsSpent), "ActionPointsSpent")]
    [JsonDerivedType(typeof(CombatEvent_Moved), "Moved")]
    [JsonDerivedType(typeof(CombatEvent_FacingChanged), "FacingChanged")]
    [JsonDerivedType(typeof(CombatEvent_DamageApplied), "DamageApplied")]
    [JsonDerivedType(typeof(CombatEvent_HealApplied), "HealApplied")]
    [JsonDerivedType(typeof(CombatEvent_StatModifierApplied), "StatModifierApplied")]
    [JsonDerivedType(typeof(CombatEvent_StatModifierRemoved), "StatModifierRemoved")]
    [JsonDerivedType(typeof(CombatEvent_PassiveAdded), "PassiveAdded")]
    [JsonDerivedType(typeof(CombatEvent_PassiveRemoved), "PassiveRemoved")]
    [JsonDerivedType(typeof(CombatEvent_PassiveActivated), "PassiveActivated")]
    [JsonDerivedType(typeof(CombatEvent_PassiveExpirationProgressed), "PassiveExpirationProgressed")]
    [JsonDerivedType(typeof(CombatEvent_PassiveStacksReduced), "PassiveStacksReduced")]
    [JsonDerivedType(typeof(CombatEvent_UnitTurnStarted), "UnitTurnStarted")]
    [JsonDerivedType(typeof(CombatEvent_UnitTurnEnded), "UnitTurnEnded")]
    [JsonDerivedType(typeof(CombatEvent_UnitTurnRefreshed), "UnitTurnRefreshed")]
    [JsonDerivedType(typeof(CombatEvent_NonStackableEffectAdded), "NonStackableEffectAdded")]
    [JsonDerivedType(typeof(CombatEvent_NonStackableEffectRemoved), "NonStackableEffectRemoved")]
    [JsonDerivedType(typeof(CombatEvent_Shoved), "Shoved")]
    public abstract record CombatEvent : NetworkGameEvent;

    public abstract record CombatEvent_CombatStep(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext) : CombatEvent;

    // Structural / Presentation Events (Do not mutate state)

    public sealed record CombatEvent_ActionStarted : CombatEvent
    {
        public static readonly CombatEvent_ActionStarted Instance = new();

        public override string ToString()
        {
            return "ActionStarted";
        }
    }
    public sealed record CombatEvent_ActionEnded : CombatEvent
    {
        public static readonly CombatEvent_ActionEnded Instance = new();

        public override string ToString()
        {
            return "ActionEnded";
        }
    }
    public sealed record CombatEvent_StepStarted(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"StepStarted(UserIdentifier:({UserIdentifier}); ActionTimingContext:({ActionTimingContext}))";
        }
    }
    public sealed record CombatEvent_StepEnded(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"StepEnded(UserIdentifier:({UserIdentifier}); ActionTimingContext:({ActionTimingContext}))";
        }
    }

    public sealed record CombatEvent_AttackSwung(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, ValueArray<GridPosition> TargetPositions) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"AttackSwung(TargetPositions:[{string.Join(", ", TargetPositions)}])";
        }
    }
    public sealed record CombatEvent_AttackMissed(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier TargetIdentifier) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"AttackMissed(TargetIdentifier:({TargetIdentifier}))";
        }
    }
    public sealed record CombatEvent_AttackHit(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier TargetIdentifier, bool WasCrit, bool WasFatal) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"AttackHit(WasCrit:({WasCrit}); WasFatal:({WasFatal}); TargetIdentifier:({TargetIdentifier}))";
        }
    }

    public sealed record CombatEvent_Shoved(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"Shoved";
        }
    }

    public sealed record CombatEvent_UnitDied(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier DeadUnitIdentifier) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"UnitDied(DeadUnitIdentifier:({DeadUnitIdentifier}))";
        }
    }

    public sealed record CombatEvent_UnitTurnStarted(CombatUnitUniqueIdentifier UnitIdentifier) : CombatEvent;
    // State Mutating events

    public sealed record CombatEvent_UnitTurnEnded(CombatUnitUniqueIdentifier UnitIdentifier) : CombatEvent;
    public sealed record CombatEvent_UnitTurnRefreshed(CombatUnitUniqueIdentifier UnitIdentifier, int ResultingActionPoints) : CombatEvent;

    public sealed record CombatEvent_ActionPointsSpent(CombatUnitUniqueIdentifier EffectedUnitIdentifier, int ActionPointsSpent, int ResultingActionPoints) : CombatEvent
    {
        public override string ToString()
        {
            return $"ActionPointsSpent(ActionPointsSpent:({ActionPointsSpent}); ResultingActionPoints:({ResultingActionPoints}); EffectedUnitIdentifier:({EffectedUnitIdentifier}))";
        }
    }

    public sealed record CombatEvent_Moved(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier EffectedUnitIdentifier, GridPosition FromPosition, GridPosition ToPosition, MovementType MovementType, bool IsFirstMove, bool IsLastMove) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"Moved(From:({FromPosition}) -> To:({ToPosition}); Type:({MovementType});{(IsFirstMove ? " IsFirst;" : string.Empty)}{(IsLastMove ? " IsLast;" : string.Empty)}({EffectedUnitIdentifier}))";
        }
    }
    public sealed record CombatEvent_FacingChanged(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier EffectedUnitIdentifier, EightDirection FromDirection, EightDirection ToDirection) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"FacingChanged(From:({FromDirection}) -> To:({ToDirection}); ({EffectedUnitIdentifier}))";
        }
    }

    public sealed record CombatEvent_DamageApplied(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, DamageType DamageType, int DamageDealt, int ResultingHealth, bool WasCrit) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"DamageApplied(DamageType:({DamageType}); DamageDealt:({DamageDealt}); ResultingHealth:({ResultingHealth}); WasCrit:({WasCrit}) SourceIdentifier:({SourceIdentifier}); TargetIdentifier:({TargetIdentifier}))";
        }
    }

    public sealed record CombatEvent_HealApplied(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, int HealthRestored, int ResultingHealth) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"HealApplied(HealthRestored:({HealthRestored}); ResultingHealth:({ResultingHealth}); SourceIdentifier:({SourceIdentifier}); TargetIdentifier:({TargetIdentifier}))";
        }
    }

    public sealed record CombatEvent_StatModifierApplied(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, TemporaryStatModifierState Modifier) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"StatModifierApplied(Modifier:({Modifier}); SourceIdentifier:({SourceIdentifier}); TargetIdentifier:({TargetIdentifier}))";
        }
    }
    // TODO: May need some better way of removal than holding the whole modifier (index/id?)
    public sealed record CombatEvent_StatModifierRemoved(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier TargetIdentifier, TemporaryStatModifierState Modifier) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"StatModifierRemoved(Modifier:({Modifier}); TargetIdentifier:({TargetIdentifier}))";
        }
    }

    public sealed record CombatEvent_PassiveAdded(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, string PassiveId, int StacksAdded, PassiveState ResultingPassive) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"PassiveAdded(PassiveId:({PassiveId}); StacksAdded:({StacksAdded}); ({ResultingPassive}); SourceIdentifier:({SourceIdentifier}); TargetIdentifier:({TargetIdentifier}))";
        }
    }
    public sealed record CombatEvent_PassiveRemoved(CombatUnitUniqueIdentifier EffectedUnitIdentifier, string PassiveId) : CombatEvent
    {
        public override string ToString()
        {
            return $"PassiveRemoved(PassiveId:({PassiveId}); TargetIdentifier:({EffectedUnitIdentifier}))";
        }
    }
    public sealed record CombatEvent_PassiveActivated(CombatUnitUniqueIdentifier OwnerIdentifier, CombatUnitUniqueIdentifier SourceIdentifier, string PassiveId, PassiveActivateTime ActivateTime, PassiveState ResultingPassive) : CombatEvent
    {
        public override string ToString()
        {
            return $"PassiveActivated(PassiveId:({PassiveId}); ActivateTime:({ActivateTime}); ResultingPassive:({ResultingPassive}); OwnerIdentifier:({OwnerIdentifier}); SourceIdentifier:({SourceIdentifier}))";
        }
    }

    public sealed record CombatEvent_PassiveExpirationProgressed(CombatUnitUniqueIdentifier OwnerIdentifier, string PassiveId, PassiveState ResultingPassive) : CombatEvent;
    public sealed record CombatEvent_PassiveStacksReduced(CombatUnitUniqueIdentifier OwnerIdentifier, string PassiveId, int StacksReduced, PassiveState? ResultingPassive) : CombatEvent;

    public sealed record CombatEvent_NonStackableEffectAdded(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, string EffectId) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"NonStackableEffectAdded(EffectId:({EffectId}); SourceIdentifier:({SourceIdentifier}); TargetIdentifier:({TargetIdentifier}))";
        }
    }
    public sealed record CombatEvent_NonStackableEffectRemoved(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, string EffectId) : CombatEvent_CombatStep(UserIdentifier, ActionTimingContext)
    {
        public override string ToString()
        {
            return $"NonStackableEffectRemoved(EffectId:({EffectId}); SourceIdentifier:({SourceIdentifier}); TargetIdentifier:({TargetIdentifier}))";
        }
    }
}
