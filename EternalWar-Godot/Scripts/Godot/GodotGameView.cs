// Original Authors - Wyatt Senalik
using EternalWar.Client.Application;
using EternalWar.Client.Application.Lobby.Views.Actions;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.Run.Map.Views.Actions;
using EternalWar.Engine.Godot.Lobby;
using EternalWar.Engine.Godot.Lobby.Nodes;
using EternalWar.Engine.Godot.Run;
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using EternalWar.Engine.Godot.Run.Map.Nodes;
using GodotExtensions;

namespace EternalWar.Engine.Godot
{
    public sealed class GodotGameView
    {
        public GameViewActions GameViewActions { get; }
        public ILobbyViewActions LobbyViewActions => GameViewActions.LobbyViewActions;
        public ICombatViewActions CombatViewActions => GameViewActions.CombatViewActions;
        public IMapViewActions MapViewActions => GameViewActions.MapViewActions;

        public GodotGameView(GodotGameVisualDatabase visualDatabase, GodotEngineRandomResolver engineRandomResolver, GodotLobbyNodeCollection lobbyNodeCollection, GodotCombatNodeCollection combatNodeCollection, GodotMapNodeCollection mapNodeCollection, ButtonFactory<string> combatActionButtonsFactory, ButtonFactory<int> mapNodeButtonsFactory)
        {
            GodotLobbyView lobbyView = new(lobbyNodeCollection);
            GodotRunView runView = new(visualDatabase, engineRandomResolver, combatNodeCollection, mapNodeCollection, combatActionButtonsFactory, mapNodeButtonsFactory);

            ICombatViewActions combatViewActions = runView.CombatViewActions;
            IMapViewActions mapViewActions = runView.MapViewActions;
            GameViewActions = new GameViewActions(lobbyView, combatViewActions, mapViewActions);
        }
    }
}
