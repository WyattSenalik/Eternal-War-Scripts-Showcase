// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelGame.Actions;
using EternalWar.Client.Application.HighLevelGame.Controllers;
using EternalWar.Client.Application.HighLevelNetwork.Actions;
using EternalWar.Client.Application.HighLevelNetwork.Controllers;
using EternalWar.Client.Application.Lobby.Actions;
using EternalWar.Client.Application.Lobby.Controllers;
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Combat.Actions;
using EternalWar.Client.Application.Run.Controllers;
using System;

namespace EternalWar.Client.Application
{
    internal sealed class GameController : IDisposable
    {
        public IHighLevelNetworkActions HighLevelNetworkActions => GameActions.HighLevelNetworkActions;
        public IHighLevelGameActions HighLevelGameActions => GameActions.HighLevelGameActions;
        public ILobbyActions LobbyActions => GameActions.LobbyActions;
        public ICombatActions CombatActions => GameActions.GetCombatActions();
        public GameActions GameActions { get; }

        private readonly LobbyController _lobbyController;

        public GameController(ClientGameSession clientSession, string dataFolderAbsolutePath)
        {
            HighLevelNetworkController highLevelNetworkController = new(clientSession);
            HighLevelGameController highLevelGameController = new(clientSession);
            _lobbyController = new LobbyController(clientSession, highLevelNetworkController, dataFolderAbsolutePath);
            RunController runController = new(clientSession);

            GameActions = new GameActions(highLevelNetworkController, highLevelGameController, _lobbyController, runController.RunActions);
        }

        public void Dispose()
        {
            _lobbyController.Dispose();
        }
    }
}
