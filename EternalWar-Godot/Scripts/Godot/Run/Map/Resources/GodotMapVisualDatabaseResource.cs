// Original Authors - Jack Dekko
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Map.Resources
{
    [GlobalClass]
    public sealed partial class GodotMapVisualDatabaseResource : Resource
    {
        public PackedScene NodePrefab => _nodePrefab ?? throw new ArgumentNullException(nameof(_nodePrefab));
        public PackedScene NodeRowPrefab => _nodeRowPrefab ?? throw new ArgumentNullException(nameof(_nodeRowPrefab));

        [Export] private PackedScene? _nodePrefab;
        [Export] private PackedScene? _nodeRowPrefab;
    }
}
