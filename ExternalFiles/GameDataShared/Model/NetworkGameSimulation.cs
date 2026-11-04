// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Core.Model
{
    public static class NetworkGameSimulation
    {
        public static NetworkGameState DisconnectClient(this NetworkGameState networkGameState, Guid clientId) => DisconnectClient(networkGameState, clientId, out _);

        public static NetworkGameState DisconnectClient(this NetworkGameState networkGameState, Guid clientId, out ClientInfo disconnectedClient)
        {
            ValueArray<ClientInfo> oldClients = networkGameState.ConnectedClients;

            using PooledList<ClientInfo> newClientsList = new();
            newClientsList.AddRange(oldClients);
            int foundIndex = newClientsList.FindIndex(client => client.Id == clientId);
            disconnectedClient = newClientsList[foundIndex];
            newClientsList.RemoveAt(foundIndex);

            GameState oldGameState = networkGameState.GameState;
            GameState newGameState = GameSimulation.HandleClientDisconnect(oldGameState, disconnectedClient);

            return new NetworkGameState(newClientsList.ToValueArray(), newGameState);
        }
    }
}
