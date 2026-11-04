// Original Authors - Wyatt Senalik
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "conditionType")]
    [JsonDerivedType(typeof(PassiveConditionInfo_AttackedWithinDistance), "AttackedWithinDistance")]
    [JsonDerivedType(typeof(PassiveConditionInfo_HealthReducedBelow), "HealthReducedBelow")]
    [JsonDerivedType(typeof(PassiveConditionInfo_Always), "Always")]
    public abstract record PassiveConditionInfo;

    public sealed record PassiveConditionInfo_AttackedWithinDistance(int TileDistance) : PassiveConditionInfo
    {
        public static readonly PassiveConditionInfo_AttackedWithinDistance Empty = new(0);
    }

    public sealed record PassiveConditionInfo_HealthReducedBelow(int HealthThreshold) : PassiveConditionInfo
    {
        public static readonly PassiveConditionInfo_HealthReducedBelow Empty = new(0);
    }

    public sealed record PassiveConditionInfo_Always() : PassiveConditionInfo;
}
