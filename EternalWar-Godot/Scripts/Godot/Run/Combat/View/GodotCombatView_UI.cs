// Original Authors - Wyatt Senalik
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using GodotExtensions;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatView_UI(GodotGameVisualDatabase visualDatabase, GodotCombatUINodeCollection combatUINodeCollectionInstance, ButtonFactory<string> combatActionButtonFactory)
    {
        public GodotCombatView_UI_TurnOrder TurnOrderView => _turnOrderView;
        public GodotCombatView_UI_CombatActions CombatActionsView => _combatActionsView;
        public GodotCombatView_UI_End EndView => _endView;

        private readonly GodotCombatView_UI_TurnOrder _turnOrderView = new(visualDatabase, combatUINodeCollectionInstance.TurnOrderNodeCollection);
        private readonly GodotCombatView_UI_CombatActions _combatActionsView = new(combatUINodeCollectionInstance.CombatActionsNodeCollection, combatActionButtonFactory);
        private readonly GodotCombatView_UI_End _endView = new(combatUINodeCollectionInstance.EndNodeCollection);
    }
}
