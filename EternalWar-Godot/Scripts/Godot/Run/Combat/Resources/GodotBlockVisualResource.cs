// Original Authors - Wyatt Senalik
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Combat.Resources
{
    [GlobalClass]
    public sealed partial class GodotBlockVisualResource : Resource
    {
        public string DefinitionId => _definitionId ?? string.Empty;
        public PackedScene VisualPrefab => _visualPrefab ?? throw new ArgumentNullException(nameof(_visualPrefab));

        [Export] private string? _definitionId;
        [Export] private PackedScene? _visualPrefab;
    }
}
