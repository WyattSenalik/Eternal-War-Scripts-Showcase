// Original Authors - Wyatt Senalik
using EternalWar.Core.Network;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Common
{
    [JsonPolymorphic]
    [JsonDerivedType(typeof(CombatUnitOwner_Player), "Player")]
    [JsonDerivedType(typeof(CombatUnitOwner_AI), "AI")]
    public abstract record CombatUnitOwner;

    public sealed record CombatUnitOwner_Player(ClientInfo PlayerClient) : CombatUnitOwner;
    public sealed record CombatUnitOwner_AI : CombatUnitOwner;
}