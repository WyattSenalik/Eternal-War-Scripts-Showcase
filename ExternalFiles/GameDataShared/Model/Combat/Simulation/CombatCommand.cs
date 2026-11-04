// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Simulation
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "commandType")]
    [JsonDerivedType(typeof(CombatCommand_BeginAction), "BeginAction")]
    [JsonDerivedType(typeof(CombatCommand_CompleteAction), "CompleteAction")]
    [JsonDerivedType(typeof(CombatCommand_BeginActionStep), "BeginActionStep")]
    [JsonDerivedType(typeof(CombatCommand_CompleteActionStep), "CompleteActionStep")]
    [JsonDerivedType(typeof(CombatCommand_PerformAttack), "PerformAttack")]
    [JsonDerivedType(typeof(CombatCommand_ResolveAttackMiss), "ResolveAttackMiss")]
    [JsonDerivedType(typeof(CombatCommand_ResolveAttackHit), "ResolveAttackHit")]
    [JsonDerivedType(typeof(CombatCommand_NotifyUnitDied), "NotifyUnitDied")]
    [JsonDerivedType(typeof(CombatCommand_MoveUnit), "MoveUnit")]
    [JsonDerivedType(typeof(CombatCommand_ChangeFacing), "ChangeFacing")]
    [JsonDerivedType(typeof(CombatCommand_ApplyHealing), "ApplyHealing")]
    [JsonDerivedType(typeof(CombatCommand_ApplyStatModifier), "ApplyStatModifier")]
    [JsonDerivedType(typeof(CombatCommand_RemoveStatModifier), "RemoveStatModifier")]
    [JsonDerivedType(typeof(CombatCommand_AddPassive), "AddPassive")]
    [JsonDerivedType(typeof(CombatCommand_ActivatePassive), "ActivatePassive")]
    [JsonDerivedType(typeof(CombatCommand_ProgressPassiveExpiration), "ProgressPassiveExpiration")]
    [JsonDerivedType(typeof(CombatCommand_ReducePassiveStacks), "ReducePassiveStacks")]
    [JsonDerivedType(typeof(CombatCommand_StartUnitTurn), "StartUnitTurn")]
    [JsonDerivedType(typeof(CombatCommand_EndUnitTurn), "EndUnitTurn")]
    [JsonDerivedType(typeof(CombatCommand_RefreshUnitTurn), "RefreshUnitTurn")]
    [JsonDerivedType(typeof(CombatCommand_AddNonStackableEffect), "AddNonStackableEffect")]
    [JsonDerivedType(typeof(CombatCommand_RemoveNonStackableEffect), "RemoveNonStackableEffect")]
    [JsonDerivedType(typeof(CombatCommand_PerformShove), "PerformShove")]
    public abstract record CombatCommand : NetworkGameCommand_Run_Combat;

    public sealed record CombatCommand_BeginAction(CombatUnitUniqueIdentifier UserIdentifier, int ActionPointsSpent, int ResultingActionPoints) : CombatCommand;
    public sealed record CombatCommand_CompleteAction : CombatCommand
    {
        public static readonly CombatCommand_CompleteAction Instance = new();
    }
    public sealed record CombatCommand_BeginActionStep(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext) : CombatCommand;
    public sealed record CombatCommand_CompleteActionStep(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext) : CombatCommand;
    public sealed record CombatCommand_PerformAttack(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, ValueArray<GridPosition> TargetPositions) : CombatCommand;
    public sealed record CombatCommand_ResolveAttackMiss(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier TargetIdentifier) : CombatCommand;
    public sealed record CombatCommand_NotifyUnitDied(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier DeadUnitIdentifier) : CombatCommand;
    public sealed record CombatCommand_ResolveAttackHit(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier TargetIdentifier, bool WasCrit, bool WasFatal, DamageType DamageType, int DamageDealt, int ResultingHealth) : CombatCommand;
    public sealed record CombatCommand_MoveUnit(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier EffectedUnitIdentifier, GridPosition FromPosition, GridPosition ToPosition, MovementType MovementType, bool IsFirstMove, bool IsLastMove) : CombatCommand;
    public sealed record CombatCommand_ChangeFacing(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier EffectedUnitIdentifier, EightDirection FromDirection, EightDirection ToDirection) : CombatCommand;
    public sealed record CombatCommand_ApplyHealing(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, int HealthRestored, int ResultingHealth) : CombatCommand;
    public sealed record CombatCommand_ApplyStatModifier(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, TemporaryStatModifierState Modifier) : CombatCommand;
    public sealed record CombatCommand_RemoveStatModifier(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier TargetIdentifier, TemporaryStatModifierState Modifier) : CombatCommand;
    public sealed record CombatCommand_AddPassive(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, string PassiveId, int StacksAdded, PassiveState ResultingPassive) : CombatCommand;
    public sealed record CombatCommand_ActivatePassive(CombatUnitUniqueIdentifier OwnerIdentifier, CombatUnitUniqueIdentifier SourceIdentifier, string PassiveId, PassiveActivateTime ActivateTime, PassiveState ResultingPassive) : CombatCommand;
    public sealed record CombatCommand_ProgressPassiveExpiration(CombatUnitUniqueIdentifier OwnerIdentifier, string PassiveId, PassiveState ResultingPassive) : CombatCommand;
    public sealed record CombatCommand_ReducePassiveStacks(CombatUnitUniqueIdentifier OwnerIdentifier, string PassiveId, int StacksReduced, PassiveState? ResultingPassive) : CombatCommand;
    public sealed record CombatCommand_StartUnitTurn(CombatUnitUniqueIdentifier UnitIdentifier) : CombatCommand;
    public sealed record CombatCommand_EndUnitTurn(CombatUnitUniqueIdentifier UnitIdentifier) : CombatCommand;
    public sealed record CombatCommand_RefreshUnitTurn(CombatUnitUniqueIdentifier UnitIdentifier, int ResultingActionPoints) : CombatCommand;
    public sealed record CombatCommand_AddNonStackableEffect(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, string EffectId) : CombatCommand;
    public sealed record CombatCommand_RemoveNonStackableEffect(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext, CombatUnitUniqueIdentifier SourceIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, string EffectId) : CombatCommand;
    public sealed record CombatCommand_PerformShove(CombatUnitUniqueIdentifier UserIdentifier, ActionSequenceTimingContext ActionTimingContext) : CombatCommand;
}
