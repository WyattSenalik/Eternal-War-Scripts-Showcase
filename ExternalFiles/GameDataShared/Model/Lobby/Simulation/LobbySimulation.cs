// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Network;
using System;
using System.Linq;

namespace EternalWar.Core.Model.Lobby.Simulation
{
    public static class LobbySimulation
    {
        public static LobbyRuntime HandleClientDisconnect(this LobbyRuntime lobbyRuntime, ClientInfo disconnectedClient)
        {
            LobbyState oldLobbyState = lobbyRuntime.State;

            LobbyState newLobbyState = HandleClientDisconnect(oldLobbyState, disconnectedClient);
            return lobbyRuntime with
            {
                State = newLobbyState
            };
        }

        public static LobbyState HandleClientDisconnect(this LobbyState lobbyState, ClientInfo disconnectedClient)
        {
            SortedValueArray<int, ClientInfo> oldReadyClients = lobbyState.ReadyClients;

            using SortedValueArray<int, ClientInfo>.Builder newReadyClientsBuilder = new(oldReadyClients);
            if (!newReadyClientsBuilder.Remove(disconnectedClient.PlayerIndex))
                return lobbyState;

            return lobbyState with
            {
                ReadyClients = newReadyClientsBuilder.ToSortedValueArray()
            };
        }

        public static LobbyState ToggleClientReady(this LobbyState lobbyState, ClientInfo clientInfo)
        {
            SortedValueArray<int, ClientInfo> readyClients = lobbyState.ReadyClients;
            using SortedValueArray<int, ClientInfo>.Builder modifiedReadyClients = new(readyClients);
            bool isReady = readyClients.Contains(clientInfo);
            if (isReady)
                modifiedReadyClients.Remove(clientInfo.PlayerIndex);
            else if (modifiedReadyClients.ContainsKey(clientInfo.PlayerIndex))
            {
                modifiedReadyClients.Remove(clientInfo.PlayerIndex);
                modifiedReadyClients.Add(clientInfo.PlayerIndex, clientInfo);
            }
            else
                modifiedReadyClients.Add(clientInfo.PlayerIndex, clientInfo);

            return lobbyState with
            {
                ReadyClients = modifiedReadyClients.ToSortedValueArray()
            };
        }
    }
}
