// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Model.Interaction;
using EternalWar.Client.Application.Network;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using NUnit.Framework;
using System;

namespace EternalWar.Client.Application.Tests
{
    [TestFixture]
    internal sealed class ClientGameSessionTests
    {
        private const string DATA_FOLDER_PATH = "../../../../../Data";
        private static readonly ClientInfo _client = new(Guid.Parse("10000000-0000-0000-0000-000000000001"), 0);

        [SetUp]
        public void SetUp()
        {
            NetworkHelpers.DisconnectFromServer();
        }

        [TearDown]
        public void TearDown()
        {
            NetworkHelpers.DisconnectFromServer();
        }

        [Test]
        public void ConnectionTransitionsPublishChangesAndUpdateThePresentedState()
        {
            ClientGameSession session = CreateSession();
            Assert.That(session.ConnectionState, Is.TypeOf<ClientConnectionState_Disconnected>());
            Assert.That(session.TryPeekClientGameStateChange(out _), Is.False);

            session.SetConnecting();
            Assert.That(session.ConnectionState, Is.TypeOf<ClientConnectionState_Connecting>());
            Assert.That(session.TryPeekClientGameStateChange(out ClientGameStateChange? connecting), Is.True);
            Assert.That(connecting, Is.TypeOf<ClientGameStateChange_ConnectionConnecting>());
            session.ConsumeClientGameStateChange();

            NetworkGameState connectedState = new(new ValueArray<ClientInfo>(_client), GameState.InitialState);
            session.SetConnected(_client, connectedState, sequenceId: 15);
            Assert.That(session.ConnectionState, Is.EqualTo(new ClientConnectionState_Connected(_client)));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(15));
            Assert.That(session.GetAuthoritativeClientGameState().GetMyClientInfo(), Is.EqualTo(_client));
            Assert.That(session.TryPeekClientGameStateChange(out ClientGameStateChange? connected), Is.True);
            Assert.That(connected, Is.TypeOf<ClientGameStateChange_ConnectionConnected>());
            session.ConsumeClientGameStateChange();

            session.SetDisconnected();
            Assert.That(session.ConnectionState, Is.TypeOf<ClientConnectionState_Disconnected>());
            Assert.That(session.AuthoritativeGameState, Is.EqualTo(DatedNetworkGameState.InitialState));
            Assert.That(session.TryPeekClientGameStateChange(out ClientGameStateChange? disconnected), Is.True);
            Assert.That(disconnected, Is.TypeOf<ClientGameStateChange_ConnectionDisconnected>());
        }

        [Test]
        public void TellCurrentConnectionStatePublishesTheCurrentStateForEachConnectionMode()
        {
            ClientGameSession session = CreateSession();

            session.TellCurrentConnectionState();
            Assert.That(session.TryPeekClientGameStateChange(out ClientGameStateChange? disconnected), Is.True);
            Assert.That(disconnected, Is.TypeOf<ClientGameStateChange_ConnectionDisconnected>());
            session.ConsumeClientGameStateChange();

            session.SetConnecting();
            session.ConsumeClientGameStateChange();
            session.TellCurrentConnectionState();
            Assert.That(session.TryPeekClientGameStateChange(out ClientGameStateChange? connecting), Is.True);
            Assert.That(connecting, Is.TypeOf<ClientGameStateChange_ConnectionConnecting>());
            session.ConsumeClientGameStateChange();

            session.SetConnected(_client, NetworkGameState.InitialState, 0);
            session.ConsumeClientGameStateChange();
            session.TellCurrentConnectionState();
            Assert.That(session.TryPeekClientGameStateChange(out ClientGameStateChange? connected), Is.True);
            Assert.That(connected, Is.TypeOf<ClientGameStateChange_ConnectionConnected>());
        }

        [Test]
        public void MissingQueuedChangesCannotBeConsumedAndLocalRequestsAreDeniedWhenOffline()
        {
            ClientGameSession session = CreateSession();
            Assert.Throws<InvalidOperationException>(() => session.ConsumeClientGameStateChange());

            session.SetConnected(_client, new NetworkGameState(new ValueArray<ClientInfo>(_client), GameState.InitialState), 0);
            session.ConsumeClientGameStateChange();
            session.ApplyLocalCommandRequestAndSendToServer(new NetworkGameCommandRequest_Lobby_ToggleReady(_client));

            Assert.That(session.GetFinalPresentationClientGameState().GetLobbyRuntime().State.ReadyClients.Count, Is.Zero);
            Assert.That(session.GetCurrentPresentationClientGameState().GetLobbyRuntime().State.ReadyClients.Count, Is.Zero);
            Assert.That(session.TryPeekClientGameStateChange(out _), Is.False);
        }

        [Test]
        public void InteractionSessionSeparatesPredictedAndVisualStateAndResetClearsQueue()
        {
            const string IP_ADDRESS = "127.0.0.1";
            const string PORT = "7777";

            PresentationInteractionSession session = new();
            InteractionStateChange first = new InteractionStateChange_Lobby_IPAddress(IP_ADDRESS);
            InteractionStateChange second = new InteractionStateChange_Lobby_Port(PORT);

            session.ApplyChange(first);
            session.ApplyChange(second);
            Assert.That(session.PredictedInteractionState.GetLobbyState(), Is.EqualTo(new InteractionState_Lobby(IP_ADDRESS, PORT)));
            Assert.That(session.CurrentVisualizedState.GetLobbyState(), Is.EqualTo(InteractionState_Lobby.Empty));
            Assert.That(session.TryPeekVisualizationChange(out InteractionStateChange? peeked), Is.True);
            Assert.That(peeked, Is.SameAs(first));

            session.ConsumeVisualizationChange();
            Assert.That(session.CurrentVisualizedState.GetLobbyState().IPAddress, Is.EqualTo(IP_ADDRESS));
            session.ConsumeVisualizationChange();
            Assert.That(session.CurrentVisualizedState.GetLobbyState().Port, Is.EqualTo(PORT));
            Assert.That(session.TryPeekVisualizationChange(out _), Is.False);
            Assert.Throws<InvalidOperationException>(() => session.ConsumeVisualizationChange());

            session.Reset();
            Assert.That(session.PredictedInteractionState, Is.EqualTo(InteractionState.Initial));
            Assert.That(session.CurrentVisualizedState, Is.EqualTo(InteractionState.Initial));
            Assert.That(session.TryPeekVisualizationChange(out _), Is.False);
        }

        [Test]
        public void InteractionStateAccessorsReturnFalseOrThrowForNonLobbyVariants()
        {
            InteractionState otherState = new OtherInteractionState();
            Assert.That(otherState.TryGetLobbyState(out InteractionState_Lobby? lobby), Is.False);
            Assert.That(lobby, Is.Null);
            Assert.Throws<Exception>(() => otherState.GetLobbyState());
            Assert.That(InteractionState.Initial.TryGetLobbyState(out InteractionState_Lobby? initial), Is.True);
            Assert.That(initial, Is.EqualTo(InteractionState_Lobby.Empty));
        }

        private static ClientGameSession CreateSession()
        {
            return new ClientGameSession(DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH));
        }

        private sealed record OtherInteractionState : InteractionState;
    }
}
