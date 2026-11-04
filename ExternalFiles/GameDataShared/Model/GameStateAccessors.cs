// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Core.Model
{
    public static class GameStateAccessors
    {
        public static T GetPhase<T>(GameState gameState) where T : GamePhase
        {
            GamePhase rawPhase = gameState.Phase;
            return rawPhase is T castedPhase ? castedPhase : throw new Exception($"GameState is not in the {nameof(T)} phase");
        }

        public static RunState GetRunState(GameState gameState)
        {
            var runRuntime = GetPhase<RunRuntime>(gameState);
            return runRuntime.State;
        }

        public static LobbyState GetLobbyState(GameState gameState)
        {
            var lobbyRuntime = GetPhase<LobbyRuntime>(gameState);
            return lobbyRuntime.State;
        }
    }
}
