// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Lobby.Simulation;
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Core.Model
{
    public static class GameSimulation
    {
        public static GameState HandleClientDisconnect(GameState gameState, ClientInfo disconnectedClient)
        {
            GamePhase oldGamePhase = gameState.Phase;
            GamePhase newGamePhase = oldGamePhase switch
            {
                LobbyRuntime lobbyRuntime => LobbySimulation.HandleClientDisconnect(lobbyRuntime, disconnectedClient),
                RunRuntime runRuntime => runRuntime, // TODO: Handle this?
                _ => throw new NotImplementedException($"Unhandled {nameof(GamePhase)}: ({oldGamePhase?.GetType()})")
            };
            return gameState with
            {
                Phase = newGamePhase
            };
        }
    }
}
