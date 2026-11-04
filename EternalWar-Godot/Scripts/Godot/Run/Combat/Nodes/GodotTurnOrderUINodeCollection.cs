// Original Authors - Wyatt Senalik
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Combat.Nodes
{
    public sealed partial class GodotTurnOrderUINodeCollection : Node
    {
        public Node UnitCellParentNode => _unitCellParentNode ?? throw new ArgumentNullException(nameof(_unitCellParentNode));

        [Export] private Node? _unitCellParentNode;
    }
}
