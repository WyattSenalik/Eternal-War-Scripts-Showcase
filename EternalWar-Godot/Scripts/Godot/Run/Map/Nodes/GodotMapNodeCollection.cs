// Original Authors - Jack Dekko
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Map.Nodes
{
    [GlobalClass]
    public sealed partial class GodotMapNodeCollection : Node
    {
        public Node NodeRowsParentNode => _nodeRowsParentNode ?? throw new ArgumentNullException(nameof(_nodeRowsParentNode));

        [Export] private Node? _nodeRowsParentNode;
    }
}
