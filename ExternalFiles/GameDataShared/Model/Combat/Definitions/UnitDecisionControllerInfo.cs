// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "decisionControllerType")]
    [JsonDerivedType(typeof(UnitDecisionControllerInfo_GoalOriented), "GoalOriented")]
    [JsonDerivedType(typeof(UnitDecisionControllerInfo_None), "None")]
    public abstract record UnitDecisionControllerInfo;

    public sealed record UnitDecisionControllerInfo_None : UnitDecisionControllerInfo;

    public sealed record UnitDecisionControllerInfo_GoalOriented(ValueArray<AIGoalInfo> Goals) : UnitDecisionControllerInfo
    {
        public static readonly UnitDecisionControllerInfo_GoalOriented Empty = new(ValueArray<AIGoalInfo>.Empty);
    }
}
