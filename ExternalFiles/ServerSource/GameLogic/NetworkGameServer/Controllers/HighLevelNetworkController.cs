// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using EternalWar.Core.Network.Messages;
using EternalWar.Core.Network.Messages.HighLevelNetwork;
using EternalWar.Server.Application.Actions;
using EternalWar.Server.Application.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Server.Application.Controllers
{
    internal sealed class HighLevelNetworkController(IMessageSender messageSender, ServerGameSession session) : IHighLevelNetworkActions
    {
        private readonly IMessageSender _messageSender = messageSender;
        private readonly ServerGameSession _session = session;

        public void ClientConnected(Guid clientGuid)
        {
            Debug.Print($"Client Connected: ({clientGuid})");

            // Create the connected client's info and update the game session's connections
            NetworkGameState oldNetworkGameState = _session.NetworkGameState;
            ValueArray<ClientInfo> oldConnectedClients = oldNetworkGameState.ConnectedClients;
            ClientInfo newClientInfo = CreateNewClientInfo(clientGuid, oldConnectedClients);

            ValueArray<ClientInfo> newConnectedClients = oldConnectedClients.Add(newClientInfo);
            NetworkGameCommand_ConnectedClientsChanged connectedClientsCommand = new(newConnectedClients);
            ValueArray<NetworkGameCommand> commands = new(new NetworkGameCommand[] { connectedClientsCommand });
            _session.ApplyCommand(connectedClientsCommand);
            DatedNetworkGameState newDatedNetworkGameState = _session.DatedNetworkGameState;
            long sequenceId = newDatedNetworkGameState.Sequence;
            NetworkGameState newNetworkGameState = newDatedNetworkGameState.NetworkGameState;

            // Tell the connected client the server has received them and inform them about other connections.
            TypedMessageData_SelfClientConnected selfClientConnectedData = new(newClientInfo, newNetworkGameState, sequenceId);
            _messageSender.SendMessageToClient(clientGuid, NetworkMessageTypeNames.SELF_CLIENT_CONNECTED, selfClientConnectedData);

            // Tell the other existing clients that a new client has connected
            _messageSender.SendCommandBatchToAllClientsExcept(clientGuid, newNetworkGameState, commands, sequenceId, clientRequestedChangeId: null);
        }

        public void ClientDisconnected(Guid disconnectedClientGuid)
        {
            Debug.Print($"Client Disconnected: ({disconnectedClientGuid})");

            NetworkGameState oldNetworkGameState = _session.NetworkGameState;

            // Disconnect the client
            ClientInfo disconnectedClient = oldNetworkGameState.FindClientWithGuid(disconnectedClientGuid);
            NetworkGameCommand_ClientDisconnected disconnectedCommand = new(disconnectedClient);

            // Send the full state as a change because it then auto handles things like un-readying while in the lobby. Also we expect that disconnecting to be something that doesn't happen very often, so it should be ok. 
            _session.ApplyCommandsAndRelay(new ValueArray<NetworkGameCommand>(new NetworkGameCommand[] { disconnectedCommand }), clientRequestedChangeId: null);
        }

        private static ClientInfo CreateNewClientInfo(Guid newClientId, IEnumerable<ClientInfo> existingClients)
        {
            int newPlayerIndex = GetNextPlayerIndex(existingClients);
            return new ClientInfo(newClientId, newPlayerIndex);
        }

        private static int GetNextPlayerIndex(IEnumerable<ClientInfo> existingClients)
        {
            using PooledHashSet<int> existingIndices = new();
            // Create a list of the existing player indices (sorted)
            foreach (ClientInfo client in existingClients)
            {
                int playerIndex = client.PlayerIndex;
                if (!existingIndices.Add(playerIndex))
                {
                    Debug.PrintError($"Duplicate player index found ({playerIndex}) [{string.Join(", ", existingClients.Where(clientInfo => clientInfo.PlayerIndex == playerIndex))}]");
                    continue;
                }
            }
            int expectedPlayerIndex = 0;
            while (existingIndices.Contains(expectedPlayerIndex))
            {
                ++expectedPlayerIndex;
            }
            return expectedPlayerIndex;
        }
    }
}
