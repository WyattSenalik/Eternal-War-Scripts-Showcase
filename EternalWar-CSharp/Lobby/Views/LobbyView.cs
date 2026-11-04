// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Lobby.Views.Actions;
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Model.Interaction;
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Client.Application.Lobby.Views
{
    internal sealed class LobbyView(ILobbyViewActions lobbyViewActions)
    {
        private readonly ILobbyViewActions _lobbyViewActions = lobbyViewActions;

        public void Activate(LobbyRuntime lobbyRuntime)
        {
            _lobbyViewActions.ShowLanding();
        }

        public void Deactivate()
        {
            _lobbyViewActions.Hide();
        }

        public ChangeHandledResult HandleChange(ClientGameStateChange change, ViewHandleChangeContext context)
        {
            return change switch
            {
                ClientGameStateChange_NetworkGameEvent networkEvent => HandleChange_NetworkGameEvent(networkEvent.Event, context),
                ClientGameStateChange_NetworkGameStateReset => HandleChange_StateSnapshot(context),
                ClientGameStateChange_ConnectionConnecting => HandleChange_ConnectionConnecting(),
                ClientGameStateChange_ConnectionConnected => HandleChange_ConnectionConnected(context),
                ClientGameStateChange_ConnectionDisconnected => HandleChange_ConnectionDisconnected(),
                _ => ChangeHandledResult_Finished.Instance
            };
        }

#pragma warning disable CA1822 // Mark members as static
        public ChangeHandledResult HandleInteractionChange(InteractionStateChange change, ViewHandleInteractionChangeContext context)
#pragma warning restore CA1822 // Mark members as static
        {
            return change switch
            {
                // TODO: Add some as needed
                // Don't need to explicitly set the text for ipaddress / port here because they are set engine-side anyway and the interaction data is just updated when the player changes them.
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        private ChangeHandledResult_Finished HandleChange_NetworkGameEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            return gameEvent switch
            {
                NetworkGameEvent_ConnectedClientsChanged or NetworkGameEvent_ClientDisconnected => RefreshLobbyFromClientGameState(context),
                NetworkGameEvent_Lobby_ReadyToggled => RefreshLobbyFromClientGameState(context),
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        private ChangeHandledResult_Finished HandleChange_StateSnapshot(ViewHandleChangeContext context)
        {
            if (!context.AfterApplyNetworkGameState.TryGetLobbyRuntime(out _))
                _lobbyViewActions.Hide();
            else
                RefreshLobbyFromClientGameState(context);
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleChange_ConnectionConnecting()
        {
            _lobbyViewActions.ShowLoading();
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleChange_ConnectionConnected(ViewHandleChangeContext context)
        {
            return RefreshLobbyFromClientGameState(context);
        }

        private ChangeHandledResult_Finished HandleChange_ConnectionDisconnected()
        {
            _lobbyViewActions.ShowLanding();
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished RefreshLobbyFromClientGameState(ViewHandleChangeContext context)
        {
            ClientGameState clientGameState = context.AfterApplyClientGameState;
            if (clientGameState.GetGamePhase() is not LobbyRuntime lobbyRuntime)
                return ChangeHandledResult_Finished.Instance;

            ClientConnectionState connectionState = clientGameState.ConnectionState;
            switch (connectionState)
            {
                case ClientConnectionState_Disconnected:
                    _lobbyViewActions.ShowLanding();
                    return ChangeHandledResult_Finished.Instance;

                case ClientConnectionState_Connecting:
                    _lobbyViewActions.ShowLoading();
                    return ChangeHandledResult_Finished.Instance;

                case ClientConnectionState_Connected connected:
                    _lobbyViewActions.ShowConnected();

                    ClientInfo myClientInfo = connected.MyClientInfo;
                    ValueArray<ClientInfo> connectedClients = clientGameState.NetworkGameState.ConnectedClients;
                    UpdateConnected(myClientInfo, connectedClients, lobbyRuntime.State);

                    return ChangeHandledResult_Finished.Instance;

                default:
                    throw new NotImplementedException($"Unhandled {nameof(ClientConnectionState)}: {connectionState?.GetType()}");
            }
        }

        private void UpdateConnected(ClientInfo myClientInfo, ValueArray<ClientInfo> connectedClients, LobbyState lobbyState)
        {
            bool isSelfReady = lobbyState.IsClientReady(myClientInfo);
            _lobbyViewActions.UpdateSelfReady(isSelfReady);

            SortedValueArray<int, ClientInfo> readyClients = lobbyState.ReadyClients;
            int readyClientCount = readyClients.Count;
            int totalClientCount = connectedClients.Count;
            _lobbyViewActions.UpdateReadyPlayers(readyClientCount, totalClientCount);
        }
    }
}
