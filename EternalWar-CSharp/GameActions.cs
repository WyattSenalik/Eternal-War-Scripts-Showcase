// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelGame.Actions;
using EternalWar.Client.Application.HighLevelNetwork.Actions;
using EternalWar.Client.Application.Lobby.Actions;
using EternalWar.Client.Application.Run.Combat.Actions;
using EternalWar.Client.Application.Run.Controllers;

namespace EternalWar.Client.Application
{
    internal sealed record GameActions(IHighLevelNetworkActions HighLevelNetworkActions, IHighLevelGameActions HighLevelGameActions, ILobbyActions LobbyActions, RunActions RunActions);

    internal static class GameActionsExtensions
    {
        public static ICombatActions GetCombatActions(this GameActions gameActions)
        {
            RunActions runActions = gameActions.RunActions;
            return runActions.CombatActions;
        }
    }
}
