// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Lobby.Views.Actions;
using EternalWar.Engine.Godot.Lobby.Nodes;
using System.Globalization;

namespace EternalWar.Engine.Godot.Lobby
{
    public sealed class GodotLobbyView : ILobbyViewActions
    {
        private const string PLAYERS_READY_TEXT_FORMAT = "{0} / {1} Ready";

        private readonly GodotLobbyNodeCollection _nodeCollectionInstance;

        public GodotLobbyView(GodotLobbyNodeCollection nodeCollectionInstance)
        {
            _nodeCollectionInstance = nodeCollectionInstance;
        }

        public void ShowLanding()
        {
            _nodeCollectionInstance.Root.Show();
            _nodeCollectionInstance.LandingScreenParent.Show();

            _nodeCollectionInstance.LoadingScreenParent.Hide();
            _nodeCollectionInstance.ConnectedScreenParent.Hide();
        }

        public void ShowLoading()
        {
            _nodeCollectionInstance.Root.Show();
            _nodeCollectionInstance.LoadingScreenParent.Show();

            _nodeCollectionInstance.LandingScreenParent.Hide();
            _nodeCollectionInstance.ConnectedScreenParent.Hide();
        }

        public void ShowConnected()
        {
            _nodeCollectionInstance.Root.Show();
            _nodeCollectionInstance.ConnectedScreenParent.Show();

            _nodeCollectionInstance.LandingScreenParent.Hide();
            _nodeCollectionInstance.LoadingScreenParent.Hide();
        }

        public void Hide()
        {
            _nodeCollectionInstance.Root.Hide();
            _nodeCollectionInstance.LandingScreenParent.Hide();
            _nodeCollectionInstance.LoadingScreenParent.Hide();
            _nodeCollectionInstance.ConnectedScreenParent.Hide();
        }

        public void UpdateSelfReady(bool isActive)
        {
            if (isActive)
                _nodeCollectionInstance.IAmReadyLabel.Show();
            else
                _nodeCollectionInstance.IAmReadyLabel.Hide();
        }

        public void UpdateReadyPlayers(int readyPlayersCount, int totalPlayersCount)
        {
            _nodeCollectionInstance.PlayersReadyTextLabel.Text = string.Format(CultureInfo.CurrentCulture, PLAYERS_READY_TEXT_FORMAT, readyPlayersCount, totalPlayersCount);
        }
    }
}
