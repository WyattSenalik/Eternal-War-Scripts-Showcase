// Original Authors - Wyatt Senalik
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Combat.Nodes
{
    public sealed partial class GodotCombatUINodeCollection : Node
    {
        public GodotCombatActionsUINodeCollection CombatActionsNodeCollection => _combatActionsNodeCollection ?? throw new ArgumentNullException(nameof(_combatActionsNodeCollection));
        public GodotTurnOrderUINodeCollection TurnOrderNodeCollection => _turnOrderNodeCollection ?? throw new ArgumentNullException(nameof(_turnOrderNodeCollection));
        public GodotEndUINodeCollection EndNodeCollection => _endNodeCollection ?? throw new ArgumentNullException(nameof(_endNodeCollection));

        [Export] private GodotCombatActionsUINodeCollection? _combatActionsNodeCollection;
        [Export] private GodotTurnOrderUINodeCollection? _turnOrderNodeCollection;
        [Export] private GodotEndUINodeCollection? _endNodeCollection;
    }
}
