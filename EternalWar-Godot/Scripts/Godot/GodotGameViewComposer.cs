// Original Authors - Wyatt Senalik
using EternalWar.Client.Application;
using EternalWar.Client.Application.Lobby.Interaction.Actions;
using EternalWar.Client.Application.Run.Combat.Interaction.Actions;
using EternalWar.Client.Application.Run.Map.Interaction.Actions;
using EternalWar.Engine.Godot.Lobby;
using EternalWar.Engine.Godot.Lobby.Nodes;
using EternalWar.Engine.Godot.Run.Combat.Interaction;
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using EternalWar.Engine.Godot.Run.Map.Interaction;
using EternalWar.Engine.Godot.Run.Map.Nodes;

namespace EternalWar.Engine.Godot
{
    /// <summary>
    /// Creates the Godot Engine views and binds interactions.
    /// </summary>
    public sealed class GodotGameViewComposer(GodotGameNodeCollection gameNodeCollection, GodotGameVisualDatabase visualDatabase, GodotEngineRandomResolver engineRandomResolver) : IGameViewComposer
    {
        private readonly GodotGameNodeCollection _gameNodeCollection = gameNodeCollection;
        private readonly GodotGameVisualDatabase _visualDatabase = visualDatabase;
        private readonly GodotEngineRandomResolver _engineRandomResolver = engineRandomResolver;

        public GameViewActions Compose(GameInteractionActions interactionActions)
        {
            GodotLobbyNodeCollection lobbyNodeCollection = _gameNodeCollection.LobbyNodeCollection;
            GodotCombatNodeCollection combatNodeCollection = _gameNodeCollection.CombatNodeCollection;
            GodotMapNodeCollection mapNodeCollection = _gameNodeCollection.MapNodeCollection;
            GodotCombatUINodeCollection combatUINodeCollection = combatNodeCollection.UINodeCollection;

            ILobbyInteractionActions lobbyInteractionActions = interactionActions.LobbyActions;
            ICombatInteractionActions combatInteractionActions = interactionActions.CombatActions;
            IMapInteractionActions mapInteractionActions = interactionActions.MapActions;

            // Static UI binding
            GodotLobbyUIBinding.Bind(lobbyNodeCollection, lobbyInteractionActions);
            GodotCombatUIBinding.Bind(combatUINodeCollection, combatInteractionActions);

            // Create the interaction factories
            GodotCombatInteractionFactories combatInteractionFactories = new(_visualDatabase, combatInteractionActions);
            GodotMapInteractionFactories mapInteractionFactories = new(_visualDatabase, mapInteractionActions);

            // Setup the view
            GodotGameView godotGameView = new(_visualDatabase, _engineRandomResolver, lobbyNodeCollection, combatNodeCollection, mapNodeCollection, combatInteractionFactories.CombatActionButtonFactory, mapInteractionFactories.MapActionButtonFactory);

            return godotGameView.GameViewActions;
        }
    }
}
