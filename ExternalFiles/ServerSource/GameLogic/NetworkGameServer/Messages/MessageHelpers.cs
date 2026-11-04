// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using EternalWar.Core.Network.Messages;
using EternalWar.Core.Network.Messages.HighLevelGame;
using EternalWar.Core.Serialization;
using System;
using System.Collections.Generic;

namespace EternalWar.Server.Application.Messages
{
    internal static class MessageHelpers
    {
        public static void SendCommandBatchToAllClients(this IMessageSender messageSender, DatedNetworkGameState datedNetworkGameState, ValueArray<NetworkGameCommand> commands, ChangeRequestId? clientRequestedChangeId)
        {
            NetworkGameState networkGameState = datedNetworkGameState.NetworkGameState;
            ValueArray<ClientInfo> clients = networkGameState.ConnectedClients;
            messageSender.SendCommandBatchToClients(clients, networkGameState, commands, datedNetworkGameState.Sequence, clientRequestedChangeId);
        }

        public static void SendCommandBatchToAllClientsExcept(this IMessageSender messageSender, Guid ignoreClientGuid, NetworkGameState networkGameState, ValueArray<NetworkGameCommand> commands, long sequenceId, ChangeRequestId? clientRequestedChangeId)
        {
            using PooledList<ClientInfo> clients = new(networkGameState.ConnectedClients);
            for (int i = 0; i < clients.Count; ++i)
            {
                if (clients[i].Id != ignoreClientGuid)
                    continue;
                clients.RemoveAt(i);
                break;
            }
            messageSender.SendCommandBatchToClients(clients, networkGameState, commands, sequenceId, clientRequestedChangeId);
        }

        public static void SendCommandBatchToClients(this IMessageSender messageSender, IEnumerable<ClientInfo> clients, NetworkGameState networkGameState, ValueArray<NetworkGameCommand> commands, long sequenceId, ChangeRequestId? clientRequestedChangeId)
        {
            GameDataHash stateHash = GameDataHashing.Hash(networkGameState);
            DatedNetworkGameCommandBatch commandBatch = new(sequenceId, commands);
            TypedMessageData_NetworkGameCommandsApplied messageData = new(stateHash, commandBatch, clientRequestedChangeId);
            TypedMessage message = new(NetworkMessageTypeNames.NETWORK_GAME_COMMANDS_APPLIED, messageData);
            string serializedMessage = message.Serialize();

            foreach (ClientInfo client in clients)
            {
                messageSender.SendMessageToClient(client.Id, serializedMessage);
            }
        }

        public static void SendNetworkGameStateSnapshotToAllClients(this IMessageSender messageSender, DatedNetworkGameState datedNetworkGameState)
        {
            TypedMessageData_NetworkGameStateSnapshot messageData = new(datedNetworkGameState);
            messageSender.SendMessageToAllClients(NetworkMessageTypeNames.NETWORK_GAME_STATE_SNAPSHOT, messageData, datedNetworkGameState.NetworkGameState.ConnectedClients);
        }

        public static void SendNetworkGameStateSnapshotToSingleClient(this IMessageSender messageSender, Guid clientGuid, DatedNetworkGameState datedNetworkGameState)
        {
            TypedMessageData_NetworkGameStateSnapshot messageData = new(datedNetworkGameState);
            messageSender.SendMessageToClient(clientGuid, NetworkMessageTypeNames.NETWORK_GAME_STATE_SNAPSHOT, messageData);
        }

        public static void SendMessageToAllClients(this IMessageSender messageSender, string messageTypeName, object? messageData, IEnumerable<ClientInfo> clients)
        {
            TypedMessage message = new(messageTypeName, messageData);
            string serializedMessage = message.Serialize();
            foreach (ClientInfo client in clients)
            {
                messageSender.SendMessageToClient(client.Id, serializedMessage);
            }
        }

        public static void SendMessageToClient(this IMessageSender messageSender, Guid clientGuid, string messageTypeName, object? messageData)
        {
            TypedMessage typedMessage = new(messageTypeName, messageData);
            string serializedMessage = typedMessage.Serialize();
            messageSender.SendMessageToClient(clientGuid, serializedMessage);
        }
    }
}
