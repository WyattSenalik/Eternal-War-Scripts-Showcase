// Original Authors - Wyatt Senalik
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Combat.Nodes
{
    public sealed partial class GodotCombatNodeCollection : Node
    {
        public Node Root => _root ?? throw new ArgumentNullException(nameof(_root));
        public Node UnitsParentNode => _unitsParentNode ?? throw new ArgumentNullException(nameof(_unitsParentNode));
        public Node BlocksParentNode => _blocksParentNode ?? throw new ArgumentNullException(nameof(_blocksParentNode));
        public Control CombatViewport => _combatViewport ?? throw new ArgumentNullException(nameof(_combatViewport));
        public Node3D LightsParentNode => _lightsParentNode ?? throw new ArgumentNullException(nameof(_lightsParentNode));
        public GodotCombatUINodeCollection UINodeCollection => _uiNodeCollection ?? throw new ArgumentNullException(nameof(_uiNodeCollection));

        [Export] private Node? _root;
        [Export] private Node? _unitsParentNode;
        [Export] private Node? _blocksParentNode;
        [Export] private Control? _combatViewport;
        [Export] private Node3D? _lightsParentNode;
        [Export] private GodotCombatUINodeCollection? _uiNodeCollection;
    }
}
