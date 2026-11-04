// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "targetType")]
    [JsonDerivedType(typeof(AITargetSelectorInfo_User), "User")]
    [JsonDerivedType(typeof(AITargetSelectorInfo_CurrentSubject), "CurrentSubject")]
    [JsonDerivedType(typeof(AITargetSelectorInfo_ClosestSpecificUnit), "ClosestSpecificUnit")]
    [JsonDerivedType(typeof(AITargetSelectorInfo_ClosestUnitWithRelation), "ClosestUnitWithRelation")]
    public abstract record AITargetSelectorInfo;
    public abstract record AITargetSelectorInfo_Closest(AITargetSelectorInfo RelativeTo) : AITargetSelectorInfo
    {
        public DistanceType DistanceComparisonType { get; init; } = DistanceType.Manhattan;
    }

    public sealed record AITargetSelectorInfo_User : AITargetSelectorInfo;
    public sealed record AITargetSelectorInfo_CurrentSubject : AITargetSelectorInfo;
    public sealed record AITargetSelectorInfo_ClosestSpecificUnit(string UnitDefinitionId, AITargetSelectorInfo RelativeTo) : AITargetSelectorInfo_Closest(RelativeTo);
    public sealed record AITargetSelectorInfo_ClosestUnitWithRelation(TeamRelation Relation, AITargetSelectorInfo RelativeTo) : AITargetSelectorInfo_Closest(RelativeTo);
}
