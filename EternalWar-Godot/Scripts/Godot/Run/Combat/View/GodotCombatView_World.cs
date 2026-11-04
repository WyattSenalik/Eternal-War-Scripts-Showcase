// Original Authors - Wyatt Senalik
using EternalWar.Engine.Godot.Run.Combat.Nodes;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatView_World(GodotGameVisualDatabase visualDatabase, GodotCombatNodeCollection combatNodeCollectionInstance, GodotEngineRandomResolver engineRandomResolver)
    {
        public GodotCombatView_World_Units UnitsView => _unitsView;
        public GodotCombatView_World_Blocks BlocksView => _blocksView;

        private readonly GodotCombatView_World_Units _unitsView = new GodotCombatView_World_Units(visualDatabase, combatNodeCollectionInstance, engineRandomResolver);
        private readonly GodotCombatView_World_Blocks _blocksView = new GodotCombatView_World_Blocks(visualDatabase, combatNodeCollectionInstance);
    }
}
