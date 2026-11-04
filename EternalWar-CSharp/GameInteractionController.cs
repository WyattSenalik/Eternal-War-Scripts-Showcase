// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Lobby.Actions;
using EternalWar.Client.Application.Lobby.Interaction.Actions;
using EternalWar.Client.Application.Lobby.Interaction.Controllers;
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Combat.Interaction.Actions;
using EternalWar.Client.Application.Run.Controllers;
using EternalWar.Client.Application.Run.Interaction.Actions;
using EternalWar.Client.Application.Run.Interaction.Controllers;
using EternalWar.Client.Application.Run.Map.Interaction.Actions;

namespace EternalWar.Client.Application
{
    public sealed record GameInteractionActions(ILobbyInteractionActions LobbyActions, IRunInteractionActions RunActions)
    {
        public ICombatInteractionActions CombatActions => RunActions.CombatActions;
        public IMapInteractionActions MapActions => RunActions.MapActions;
    } 

    internal sealed class GameInteractionController
    {
        public GameInteractionActions GameInteractionActions { get; }

        public GameInteractionController(ClientGameSession clientSession, GameActions gameActions)
        {
            ILobbyActions lobbyActions = gameActions.LobbyActions;
            RunActions runActions = gameActions.RunActions;

            LobbyInteractionController lobbyInteractionController = new(clientSession, lobbyActions);
            RunInteractionController runInteractionController = new(clientSession, runActions);

            GameInteractionActions = new GameInteractionActions(lobbyInteractionController, runInteractionController);
        }
    }
}
