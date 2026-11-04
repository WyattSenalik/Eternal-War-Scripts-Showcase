// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Network;
using EternalWar.Core.Network.Messages;
using EternalWar.Core.Network.Messages.HighLevelGame;
using EternalWar.Server.Application;
using EternalWar.Server.Application.Actions;
using EternalWar.Server.Application.Controllers;
using EternalWar.Server.Application.Messages;
using EternalWar.Server.Application.Messages.HighLevelGame;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NetworkGameServer.Tests
{
    [TestFixture]
    internal sealed class HighLevelGameControllerTests
    {
        private const string DATA_FOLDER_PATH = "../../../../../Data";
        private static readonly ClientInfo _clientA = new(Guid.Parse("60000000-0000-0000-0000-000000000001"), 0);
        private static readonly ClientInfo _clientB = new(Guid.Parse("60000000-0000-0000-0000-000000000002"), 1);

        [Test]
        public void RequestHandlerPreservesTransportSenderIdentity()
        {
            RecordingActions actions = new();
            ChangeRequest requestedChange = CreateRequest(_clientA.Id);
            TypedMessage serializedMessage = new TypedMessage(NetworkMessageTypeNames.REQUEST_COMMAND, new TypedMessageData_RequestCommand(requestedChange))
                .Serialize()
                .DeserializeIntoTypedMessage()!;
            MessageHandler_RequestCommand handler = new(actions);

            handler.Handle(new TypedMessageWithClient(serializedMessage, _clientB.Id));

            Assert.That(actions.RequestingClientId, Is.EqualTo(_clientB.Id));
            Assert.That(actions.RequestedChange, Is.EqualTo(requestedChange));
        }

        [Test]
        public void ControllerRejectsSpoofedChangeOwnerAndAuthorizesFromTransportIdentity()
        {
            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
            RecordingMessageSender sender = new();
            ServerGameSession session = new(sender, database);
            session.ApplyCommand(new NetworkGameCommand_ConnectedClientsChanged(new ValueArray<ClientInfo>(_clientA, _clientB)));
            HighLevelGameController controller = new(sender, session);
            ChangeRequest spoofed = CreateRequest(_clientA.Id);

            controller.ClientRequestCommand(_clientB.Id, spoofed);

            Assert.That(session.DatedNetworkGameState.Sequence, Is.EqualTo(1));
            Assert.That(session.NetworkGameState.GetLobbyState().ReadyClients.Count, Is.Zero);
            Assert.That(sender.Messages.Count, Is.EqualTo(1));
            Assert.That(sender.Messages[0].ClientId, Is.EqualTo(_clientB.Id));
            TypedMessage denial = sender.Messages[0].Message.DeserializeIntoTypedMessage()!;
            Assert.That(denial.Type, Is.EqualTo(NetworkMessageTypeNames.CHANGE_REQUEST_DENIED));
            Assert.That(denial.ExtractData<TypedMessageData_ChangeRequestDenied>().ChangeId, Is.EqualTo(spoofed.LocalChangeId));

            sender.Messages.Clear();
            ChangeRequest valid = CreateRequest(_clientA.Id);
            controller.ClientRequestCommand(_clientA.Id, valid);

            Assert.That(session.DatedNetworkGameState.Sequence, Is.EqualTo(2));
            Assert.That(session.NetworkGameState.GetLobbyState().IsClientReady(_clientA), Is.True);
            Assert.That(sender.Messages.Count, Is.EqualTo(2));
            Assert.That(sender.Messages.TrueForAll(message => message.Message.DeserializeIntoTypedMessage()!.Type == NetworkMessageTypeNames.NETWORK_GAME_COMMANDS_APPLIED), Is.True);
        }

        [Test]
        public void ControllerRequiresLocalRequestIdentifierAndBroadcastsCurrentStateOnDivergence()
        {
            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
            RecordingMessageSender sender = new();
            ServerGameSession session = new(sender, database);
            session.ApplyCommand(new NetworkGameCommand_ConnectedClientsChanged(new ValueArray<ClientInfo>(_clientA)));
            HighLevelGameController controller = new(sender, session);

            Assert.Throws<Exception>(() => controller.ClientRequestCommand(_clientA.Id, new ChangeRequest(new NetworkGameCommandRequest_Lobby_ToggleReady(_clientA), null)));
            Assert.That(sender.Messages, Is.Empty);

            controller.ClientStateDiverged(_clientA.Id);
            Assert.That(sender.Messages.Count, Is.EqualTo(1));
            TypedMessage stateMessage = sender.Messages[0].Message.DeserializeIntoTypedMessage()!;
            Assert.That(stateMessage.Type, Is.EqualTo(NetworkMessageTypeNames.NETWORK_GAME_STATE_SNAPSHOT));
            TypedMessageData_NetworkGameStateSnapshot data = stateMessage.ExtractData<TypedMessageData_NetworkGameStateSnapshot>();
            Assert.That(data.State.Sequence, Is.EqualTo(1));
            Assert.That(data.State.NetworkGameState, Is.EqualTo(session.NetworkGameState));
        }

        private static ChangeRequest CreateRequest(Guid requesterId)
        {
            ChangeRequestId id = new(requesterId, Guid.NewGuid());
            return new ChangeRequest(new NetworkGameCommandRequest_Lobby_ToggleReady(_clientA), id);
        }

        private sealed class RecordingMessageSender : IMessageSender
        {
            public List<(Guid ClientId, string Message)> Messages { get; } = new();
            public void SendMessageToClient(Guid clientId, string message) => Messages.Add((clientId, message));
        }

        private sealed class RecordingActions : IHighLevelGameActions
        {
            public Guid? RequestingClientId { get; private set; }
            public ChangeRequest? RequestedChange { get; private set; }
            public void ClientStateDiverged(Guid clientGuid) { }
            public void ClientRequestCommand(Guid requestingClientId, ChangeRequest requestedChange)
            {
                RequestingClientId = requestingClientId;
                RequestedChange = requestedChange;
            }
        }
    }
}
