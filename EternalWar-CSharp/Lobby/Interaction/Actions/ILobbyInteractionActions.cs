// Original Authors - Wyatt Senalik

namespace EternalWar.Client.Application.Lobby.Interaction.Actions
{
    public interface ILobbyInteractionActions
    {
        void SetPort(string port);
        void SetIPAddress(string ipAddress);
        void ButtonPressed_HostGame();
        void ButtonPressed_JoinGame();
        void ButtonPressed_ToggleReady();
        void ButtonPressed_Disconnect();
    }
}