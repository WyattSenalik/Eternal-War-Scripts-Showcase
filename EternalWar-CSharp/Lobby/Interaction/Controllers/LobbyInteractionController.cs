// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Lobby.Actions;
using EternalWar.Client.Application.Lobby.Interaction.Actions;
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Model.Interaction;
using System;

namespace EternalWar.Client.Application.Lobby.Interaction.Controllers
{
    internal sealed class LobbyInteractionController(ClientGameSession clientSession, ILobbyActions lobbyActions) : ILobbyInteractionActions
    {
        private readonly ClientGameSession _clientSession = clientSession;
        private readonly ILobbyActions _lobbyActions = lobbyActions;

        public void SetPort(string port)
        {
            InteractionStateChange_Lobby_Port portChange = new(port);
            _clientSession.InteractionSession.ApplyChange(portChange);
        }

        public void SetIPAddress(string ipAddress)
        {
            InteractionStateChange_Lobby_IPAddress ipAddressChange = new(ipAddress);
            _clientSession.InteractionSession.ApplyChange(ipAddressChange);
        }

        public void ButtonPressed_HostGame()
        {
            GetLobbyInfo(out string ipAddress, out string port);
            _lobbyActions.HostServer(ipAddress, port);
        }

        public void ButtonPressed_JoinGame()
        {
            GetLobbyInfo(out string ipAddress, out string port);
            _lobbyActions.JoinServer(ipAddress, port);
        }

        public void ButtonPressed_ToggleReady()
        {
            _lobbyActions.ToggleReady();
        }

        public void ButtonPressed_Disconnect()
        {
            _lobbyActions.Disconnect();
        }

        private void GetLobbyInfo(out string ipAddress, out string port)
        {
            InteractionState_Lobby lobbyState = GetLobbyState();
            ipAddress = lobbyState.IPAddress;
            port = lobbyState.Port;
        }

        private InteractionState_Lobby GetLobbyState()
        {
            InteractionState interactionState = _clientSession.InteractionSession.CurrentVisualizedState;
            return interactionState as InteractionState_Lobby ?? throw new Exception($"Interaction state is not in lobby state ({interactionState})");
        }
    }
}