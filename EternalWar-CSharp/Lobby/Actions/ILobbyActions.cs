// Original Authors - Wyatt Senalik
namespace EternalWar.Client.Application.Lobby.Actions
{
    internal interface ILobbyActions
    {
        void HostServer(string ipAddressString, string portString);
        void JoinServer(string ipAddressString, string portString);
        void ToggleReady();
        void Disconnect();
    }
}