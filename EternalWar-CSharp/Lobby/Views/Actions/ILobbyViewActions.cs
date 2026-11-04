// Original Authors - Wyatt Senalik
namespace EternalWar.Client.Application.Lobby.Views.Actions
{
    public interface ILobbyViewActions
    {
        void ShowLanding();
        void ShowLoading();
        void ShowConnected();
        void Hide();
        void UpdateSelfReady(bool isActive);
        void UpdateReadyPlayers(int readyPlayersCount, int totalPlayersCount);
    }
}