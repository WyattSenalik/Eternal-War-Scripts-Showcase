// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Network;
using System;
using System.Linq;

namespace EternalWar.Core.Model
{
    public sealed record DatedNetworkGameState(long Sequence, NetworkGameState NetworkGameState)
    {
        public static readonly DatedNetworkGameState InitialState = new(0, NetworkGameState.InitialState);
    }

    public sealed record NetworkGameState(ValueArray<ClientInfo> ConnectedClients, GameState GameState)
    {
        public static readonly NetworkGameState InitialState = new(ValueArray<ClientInfo>.Empty, GameState.InitialState);
    }

    public static class NetworkGameStateExtensions
    {
        public static ClientInfo FindClientWithGuid(this NetworkGameState networkGameState, Guid clientGuid)
        {
            ValueArray<ClientInfo> connectedClients = networkGameState.ConnectedClients;
            return connectedClients.First(client => client.Id == clientGuid);
        }

        public static ClientInfo FindClientWithGuid(this NetworkGameState networkGameState, Guid clientGuid, out int index)
        {
            ValueArray<ClientInfo> connectedClients = networkGameState.ConnectedClients;
            for (index = 0; index < connectedClients.Count; ++index)
            {
                ClientInfo client = connectedClients[index];
                Guid currentClientGuid = client.Id;
                if (currentClientGuid == clientGuid)
                    return client;
            }
            throw new Exception($"Found no connected client with guid ({clientGuid}) in ({connectedClients})");
        }
    }
}
