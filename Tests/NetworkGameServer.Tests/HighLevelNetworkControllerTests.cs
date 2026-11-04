// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using EternalWar.Core.Network.Messages;
using EternalWar.Core.Network.Messages.HighLevelGame;
using EternalWar.Core.Network.Messages.HighLevelNetwork;
using EternalWar.Server.Application;
using EternalWar.Server.Application.Controllers;
using EternalWar.Server.Application.Messages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkGameServer.Tests
{
    [TestFixture]
    internal sealed class HighLevelNetworkControllerTests
    {
        private static readonly ClientInfo _clientA = new(Guid.Parse("70000000-0000-0000-0000-000000000001"), 0);
        private static readonly ClientInfo _clientB = new(Guid.Parse("70000000-0000-0000-0000-000000000002"), 2);
        private static readonly Guid _clientCId = Guid.Parse("70000000-0000-0000-0000-000000000003");

        [Test]
        public void ConnectingClientGetsFirstAvailablePlayerIndexAndExistingClientsAreNotified()
        {
            ServerEnvironment environment = CreateEnvironment(new ValueArray<ClientInfo>(_clientA, _clientB));
            HighLevelNetworkController controller = new(environment.Sender, environment.Session);

            controller.ClientConnected(_clientCId);

            Assert.That(environment.Session.DatedNetworkGameState.Sequence, Is.EqualTo(2));
            Assert.That(environment.Session.NetworkGameState.ConnectedClients, Is.EqualTo(new ValueArray<ClientInfo>(_clientA, _clientB, new ClientInfo(_clientCId, 1))));
            Assert.That(environment.Sender.Messages.Count, Is.EqualTo(3));
            Assert.That(environment.Sender.Messages[0].ClientId, Is.EqualTo(_clientCId));

            TypedMessage initial = environment.Sender.Messages[0].Message.DeserializeIntoTypedMessage()!;
            Assert.That(initial.Type, Is.EqualTo(NetworkMessageTypeNames.SELF_CLIENT_CONNECTED));
            TypedMessageData_SelfClientConnected data = initial.ExtractData<TypedMessageData_SelfClientConnected>();
            Assert.That(data.SelfInfo, Is.EqualTo(new ClientInfo(_clientCId, 1)));
            Assert.That(data.SequenceId, Is.EqualTo(2));

            Assert.That(environment.Sender.Messages.Skip(1).Select(message => message.ClientId), Is.EquivalentTo(new[] { _clientA.Id, _clientB.Id }));
            foreach ((Guid _, string serialized) in environment.Sender.Messages.Skip(1))
            {
                TypedMessage message = serialized.DeserializeIntoTypedMessage()!;
                Assert.That(message.Type, Is.EqualTo(NetworkMessageTypeNames.NETWORK_GAME_COMMANDS_APPLIED));
                TypedMessageData_NetworkGameCommandsApplied stateChange = message.ExtractData<TypedMessageData_NetworkGameCommandsApplied>();
                Assert.That(stateChange.DatedCommandBatch.Sequence, Is.EqualTo(2));
                Assert.That(stateChange.DatedCommandBatch.Commands[0], Is.TypeOf<NetworkGameCommand_ConnectedClientsChanged>());
            }
        }

        [Test]
        public void DisconnectRemovesClientAndBroadcastsTheDisconnectCommandToRemainingClients()
        {
            ServerEnvironment environment = CreateEnvironment(new ValueArray<ClientInfo>(_clientA, _clientB, new ClientInfo(_clientCId, 1)));
            HighLevelNetworkController controller = new(environment.Sender, environment.Session);

            controller.ClientDisconnected(_clientB.Id);

            Assert.That(environment.Session.DatedNetworkGameState.Sequence, Is.EqualTo(2));
            Assert.That(environment.Session.NetworkGameState.ConnectedClients, Is.EqualTo(new ValueArray<ClientInfo>(_clientA, new ClientInfo(_clientCId, 1))));
            Assert.That(environment.Sender.Messages.Select(message => message.ClientId), Is.EquivalentTo(new[] { _clientA.Id, _clientCId }));
            foreach ((Guid _, string serialized) in environment.Sender.Messages)
            {
                TypedMessage message = serialized.DeserializeIntoTypedMessage()!;
                Assert.That(message.Type, Is.EqualTo(NetworkMessageTypeNames.NETWORK_GAME_COMMANDS_APPLIED));
                TypedMessageData_NetworkGameCommandsApplied stateChange = message.ExtractData<TypedMessageData_NetworkGameCommandsApplied>();
                Assert.That(stateChange.DatedCommandBatch.Sequence, Is.EqualTo(2));
                Assert.That(stateChange.DatedCommandBatch.Commands[0], Is.TypeOf<NetworkGameCommand_ClientDisconnected>());
            }
        }

        private static ServerEnvironment CreateEnvironment(ValueArray<ClientInfo> clients)
        {
            RecordingMessageSender sender = new();
            ServerGameSession session = new(sender, DatabaseLoader.LoadDatabase("../../../../../Data"));
            session.ApplyCommand(new NetworkGameCommand_ConnectedClientsChanged(clients));
            sender.Messages.Clear();
            return new ServerEnvironment(sender, session);
        }

        private sealed record ServerEnvironment(RecordingMessageSender Sender, ServerGameSession Session);

        private sealed class RecordingMessageSender : IMessageSender
        {
            public List<(Guid ClientId, string Message)> Messages { get; } = new();
            public void SendMessageToClient(Guid clientId, string message) => Messages.Add((clientId, message));
        }
    }
}
