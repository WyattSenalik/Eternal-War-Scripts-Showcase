// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model.Interaction;
using EternalWar.Client.Application.Network;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Client.Application.Model
{
    public sealed class ClientGameSession(GameDatabase database)
    {
        public GameDatabase Database => _presentationSession.Database;
        public DatedNetworkGameState AuthoritativeGameState => _presentationSession.AuthoritativeGameState;
        public ClientConnectionState ConnectionState { get; private set; } = new ClientConnectionState_Disconnected();
        public PresentationInteractionSession InteractionSession { get; private set; } = new();

        private readonly PresentationGameSession _presentationSession = new(database, DatedNetworkGameState.InitialState);
        private readonly ConcurrentQueue<ClientGameStateChange> _pendingClientGameStateChanges = new();

        public void SetConnected(ClientInfo myClientInfo, NetworkGameState networkGameState, long sequenceId)
        {
            ClientGameState oldClientGameState = GetCurrentPresentationClientGameState();
            DatedNetworkGameState newAuthoritativeState = new(sequenceId, networkGameState);
            _presentationSession.OverwriteAuthoritativeState(newAuthoritativeState);

            ConnectionState = new ClientConnectionState_Connected(myClientInfo);
            ClientGameState newClientGameState = GetCurrentPresentationClientGameState();
            ClientGameStateChange_ConnectionConnected connectedChange = new(oldClientGameState, newClientGameState);
            TellClientGameStateChange(connectedChange);

            Debug.Print("Set as connected");
        }

        public void SetConnecting()
        {
            ClientGameState oldClientGameState = GetCurrentPresentationClientGameState();

            ConnectionState = new ClientConnectionState_Connecting();
            ClientGameState newClientGameState = GetCurrentPresentationClientGameState();
            ClientGameStateChange_ConnectionConnecting connectingChange = new(oldClientGameState, newClientGameState);
            TellClientGameStateChange(connectingChange);

            Debug.Print("Set as connecting");
        }

        public void SetDisconnected()
        {
            ClientGameState oldClientGameState = GetCurrentPresentationClientGameState();

            _presentationSession.Reset();
            ConnectionState = new ClientConnectionState_Disconnected();
            ClientGameState newClientGameState = GetCurrentPresentationClientGameState();
            ClientGameStateChange_ConnectionDisconnected disconnectedChange = new(oldClientGameState, newClientGameState);
            TellClientGameStateChange(disconnectedChange);
        }

        public ApplyAuthoritativeCommandBatchResult ApplyAuthoritativeCommandBatch(DatedNetworkGameCommandBatch datedCommandBatch, ChangeRequestId? clientRequestedChangeId)
        {
            return _presentationSession.ApplyAuthoritativeCommandBatch(datedCommandBatch, clientRequestedChangeId);
        }

        public void ApplyAuthoritativeStateSnapshot(DatedNetworkGameState stateSnapshot)
        {
            ClientGameState oldClientGameState = GetCurrentPresentationClientGameState();
            _presentationSession.OverwriteAuthoritativeState(stateSnapshot);
            ClientGameState newClientGameState = GetCurrentPresentationClientGameState();
            ClientGameStateChange_NetworkGameStateReset stateResetChange = new(oldClientGameState, newClientGameState);
            TellClientGameStateChange(stateResetChange);
        }

        public void ApplyLocalCommandRequestAndSendToServer(NetworkGameCommandRequest request)
        {
            ClientInfo myClientInfo = ConnectionState.GetMyClientInfo();
            Guid myClientGuid = myClientInfo.Id;
            ChangeRequest presentationChange = _presentationSession.ApplyLocalCommandRequest(request, myClientGuid);

            if (!NetworkHelpers.SendRequestCommandMessageToServer(presentationChange))
            {
                DenyCommandRequest(presentationChange.LocalChangeId!);
            }
        }

        public void DenyCommandRequest(ChangeRequestId changeRequestId)
        {
            ClientGameState currentClientGameState = GetCurrentPresentationClientGameState();
            _presentationSession.DenyCommandRequest(changeRequestId);
            ClientGameState afterApplyClientGameState = GetCurrentPresentationClientGameState();
            if (currentClientGameState.NetworkGameState != afterApplyClientGameState.NetworkGameState)
            {
                ClientGameStateChange_NetworkGameStateReset wrappedChange = new(currentClientGameState, afterApplyClientGameState);
                TellClientGameStateChange(wrappedChange);
            }
        }

        public void ConsumeVisualizationEvent() => _presentationSession.ConsumeVisualizationEvent();

        public bool TryPeekVisualizationEvent([NotNullWhen(true)] out NetworkGameEvent? visualizationEvent) => _presentationSession.TryPeekVisualizationEvent(out visualizationEvent);

        public ClientGameState GetAuthoritativeClientGameState()
        {
            DatedNetworkGameState authoritativeDatedNetworkGameState = _presentationSession.AuthoritativeGameState;
            NetworkGameState authoritativeNetworkGameState = authoritativeDatedNetworkGameState.NetworkGameState;
            return new ClientGameState(ConnectionState, authoritativeNetworkGameState);
        }

        public ClientGameState GetFinalPresentationClientGameState()
        {
            NetworkGameState finalNetworkGameState = _presentationSession.PredictedGameState;
            return new ClientGameState(ConnectionState, finalNetworkGameState);
        }

        public ClientGameState GetCurrentPresentationClientGameState()
        {
            NetworkGameState currentNetworkGameState = _presentationSession.CurrentGameState;
            return new ClientGameState(ConnectionState, currentNetworkGameState);
        }

        internal void TellCurrentConnectionState()
        {
            ClientGameState currentClientGameState = GetCurrentPresentationClientGameState();
            ClientGameStateChange change = currentClientGameState.ConnectionState switch
            {
                ClientConnectionState_Disconnected => new ClientGameStateChange_ConnectionDisconnected(currentClientGameState, currentClientGameState),
                ClientConnectionState_Connecting => new ClientGameStateChange_ConnectionConnecting(currentClientGameState, currentClientGameState),
                ClientConnectionState_Connected => new ClientGameStateChange_ConnectionConnected(currentClientGameState, currentClientGameState),
                _ => throw new NotImplementedException($"Unhandled {nameof(ClientConnectionState)}: ({currentClientGameState.ConnectionState.GetType()})")
            };
            TellClientGameStateChange(change);
        }

        internal void TellClientGameStateChange(ClientGameStateChange change) => _pendingClientGameStateChanges.Enqueue(change);

        internal bool TryPeekClientGameStateChange([NotNullWhen(true)] out ClientGameStateChange? change)
        {
            if (_pendingClientGameStateChanges.TryPeek(out change))
                return true;

            change = null;
            return false;
        }

        internal void ConsumeClientGameStateChange()
        {
            if (!_pendingClientGameStateChanges.TryDequeue(out _))
                throw new InvalidOperationException("Cannot consume a client game change when none are queued");
        }
    }
}

