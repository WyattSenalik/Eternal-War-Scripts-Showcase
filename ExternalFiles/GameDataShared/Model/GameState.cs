// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model
{
    public sealed record GameState(GamePhase Phase)
    {
        public static readonly GameState InitialState = new(LobbyRuntime.InitialState);
    }

    [JsonPolymorphic]
    [JsonDerivedType(typeof(LobbyRuntime), "Lobby")]
    [JsonDerivedType(typeof(RunRuntime), "Run")]
    public abstract record GamePhase { }
}
