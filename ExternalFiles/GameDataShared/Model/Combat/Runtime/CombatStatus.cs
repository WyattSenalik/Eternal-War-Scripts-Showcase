// Original Authors - Eslis
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Runtime
{
    [JsonPolymorphic]
    [JsonDerivedType(typeof(CombatStatus_Active), "Active")]
    [JsonDerivedType(typeof(CombatStatus_Victory), "Victory")]
    [JsonDerivedType(typeof(CombatStatus_Defeat), "Defeat")]
    public abstract record CombatStatus;

    public sealed record CombatStatus_Active : CombatStatus;
    public sealed record CombatStatus_Victory : CombatStatus;
    public sealed record CombatStatus_Defeat : CombatStatus;
}
