// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "aiConditionType")]
    [JsonDerivedType(typeof(AIConditionInfo_And), "And")]
    [JsonDerivedType(typeof(AIConditionInfo_Or), "Or")]
    [JsonDerivedType(typeof(AIConditionInfo_Not), "Not")]
    [JsonDerivedType(typeof(AIConditionInfo_HasNonStackableEffect), "HasNonStackableEffect")]
    [JsonDerivedType(typeof(AIConditionInfo_DistanceToTarget), "DistanceToTarget")]
    [JsonDerivedType(typeof(AIConditionInfo_IsRelation), "IsRelation")]
    [JsonDerivedType(typeof(AIConditionInfo_CheckHealth), "CheckHealth")]
    [JsonDerivedType(typeof(AIConditionInfo_HasPassive), "HasPassive")]
    [JsonDerivedType(typeof(AIConditionInfo_UnitFractionSatisfyingCondition), "UnitFractionSatisfyingCondition")]
    public abstract record AIConditionInfo;

    public abstract record AIConditionInfo_HasSubject : AIConditionInfo
    {
        public AITargetSelectorInfo Subject { get; } = new AITargetSelectorInfo_CurrentSubject();
    }

    public sealed record AIConditionInfo_And(ValueArray<AIConditionInfo> Conditions) : AIConditionInfo;
    public sealed record AIConditionInfo_Or(ValueArray<AIConditionInfo> Conditions) : AIConditionInfo;
    public sealed record AIConditionInfo_Not(AIConditionInfo Condition) : AIConditionInfo;
    public sealed record AIConditionInfo_HasNonStackableEffect(string EffectId) : AIConditionInfo_HasSubject;
    public sealed record AIConditionInfo_DistanceToTarget(AITargetSelectorInfo Other, NumericPredicate DistanceComparison) : AIConditionInfo_HasSubject
    {
        public DistanceType DistanceType { get; init; } = DistanceType.Manhattan;
    }
    public sealed record AIConditionInfo_IsRelation(TeamRelation Relation) : AIConditionInfo_HasSubject;
    public sealed record AIConditionInfo_CheckHealth(ValueAmountPredicate Comparison) : AIConditionInfo_HasSubject;
    public sealed record AIConditionInfo_HasPassive(string PassiveId) : AIConditionInfo_HasSubject;
    public sealed record AIConditionInfo_UnitFractionSatisfyingCondition(TeamRelation Relation, AIConditionInfo Condition, NumericPredicate FractionComparison) : AIConditionInfo;
}
