// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.Run.Map.Views.Actions;
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using EternalWar.Engine.Godot.Run.Combat.View;
using EternalWar.Engine.Godot.Run.Map.Nodes;
using EternalWar.Engine.Godot.Run.Map.View;
using GodotExtensions;

namespace EternalWar.Engine.Godot.Run
{
    public sealed class GodotRunView(GodotGameVisualDatabase visualDatabase, GodotEngineRandomResolver engineRandomResolver, GodotCombatNodeCollection combatNodeCollectionInstance, GodotMapNodeCollection mapNodeCollectionInstance, ButtonFactory<string> combatActionButtonsFactory, ButtonFactory<int> mapNodeButtonsFactory)
    {
        public ICombatViewActions CombatViewActions => _combatView;
        public IMapViewActions MapViewActions => _mapView;

        private readonly GodotCombatView _combatView = new(visualDatabase, engineRandomResolver, combatNodeCollectionInstance, combatActionButtonsFactory);
        private readonly GodotMapView _mapView = new(visualDatabase, mapNodeCollectionInstance, mapNodeButtonsFactory);
    }
}
