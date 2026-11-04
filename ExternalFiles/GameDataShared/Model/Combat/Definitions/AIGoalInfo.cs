// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "aiGoalType")]
    [JsonDerivedType(typeof(AIGoalInfo_Kill), "Kill")]
    [JsonDerivedType(typeof(AIGoalInfo_DealDamage), "DealDamage")]
    [JsonDerivedType(typeof(AIGoalInfo_AddNonStackableEffect), "AddNonStackableEffect")]
    [JsonDerivedType(typeof(AIGoalInfo_MoveCloserToTarget), "MoveCloserToTarget")]
    [JsonDerivedType(typeof(AIGoalInfo_AddPassive), "AddPassive")]
    public abstract record AIGoalInfo
    {
        public ValueArray<AIConditionInfo> Conditions { get; init; }
    }

    public sealed record AIGoalInfo_Kill : AIGoalInfo;
    public sealed record AIGoalInfo_DealDamage : AIGoalInfo;
    public sealed record AIGoalInfo_AddNonStackableEffect(string EffectId) : AIGoalInfo;
    public sealed record AIGoalInfo_AddPassive(string PassiveId) : AIGoalInfo;

    public sealed record AIGoalInfo_MoveCloserToTarget(AITargetSelectorInfo Target) : AIGoalInfo
    {
        // TODO: Make this not care about the mover here and use conditions to limit?
        public AITargetSelectorInfo Mover { get; init; } = new AITargetSelectorInfo_User();
        public DistanceType DistanceComparisonType { get; init; } = DistanceType.Manhattan;
    }
}