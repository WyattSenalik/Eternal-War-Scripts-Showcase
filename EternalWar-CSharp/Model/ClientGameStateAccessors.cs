// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Client.Application.Model
{
    public static class ClientGameStateAccessors
    {
        public static ClientInfo GetMyClientInfo(this ClientGameState clientGameState)
        {
            var connectedState = clientGameState.GetConnectionState<ClientConnectionState_Connected>();
            return connectedState.MyClientInfo;
        }

        public static T GetConnectionState<T>(this ClientGameState clientGameState) where T : ClientConnectionState
        {
            ClientConnectionState connectionState = clientGameState.ConnectionState;
            return connectionState is T castedConnectionState ? castedConnectionState : throw new Exception($"Expected connection state to be ({typeof(T)}) but was ({connectionState?.GetType()})");
        }

        public static GamePhase GetGamePhase(this ClientGameState clientGameState)
        {
            NetworkGameState networkGameState = clientGameState.NetworkGameState;
            return networkGameState.GetGamePhase();
        }

        public static RunPhase GetRunPhase(this ClientGameState clientGameState)
        {
            NetworkGameState networkGameState = clientGameState.NetworkGameState;
            return networkGameState.GetRunPhase();
        }

        public static T GetRunPhase<T>(this ClientGameState clientGameState) where T : RunPhase
        {
            RunPhase runPhase = GetRunPhase(clientGameState);
            return runPhase as T ?? throw new Exception($"Game State's run phase is not {typeof(T).Name}! ({clientGameState})");
        }

        public static CombatRuntime GetCombatRuntime(this ClientGameState clientGameState) => clientGameState.GetRunPhase<CombatRuntime>();

        public static CombatInstance GetCombatInstance(this ClientGameState clientGameState)
        {
            CombatRuntime combatRuntime = clientGameState.GetCombatRuntime();
            return combatRuntime.Instance;
        }

        public static CombatState GetCombatState(this ClientGameState clientGameState)
        {
            CombatRuntime combatRuntime = clientGameState.GetCombatRuntime();
            return combatRuntime.State;
        }

        public static LobbyRuntime GetLobbyRuntime(this ClientGameState clientGameState)
        {
            if (!clientGameState.NetworkGameState.TryGetLobbyRuntime(out LobbyRuntime? lobbyRuntime))
                throw new Exception($"Tried to get lobby runtime from a state not in that phase");
            return lobbyRuntime;
        }

        public static TileGridInstance GetTileGridInstance(this ClientGameState clientGameState)
        {
            CombatInstance combatInstance = clientGameState.GetCombatInstance();
            return combatInstance.TileGrid;
        }
    }
}
