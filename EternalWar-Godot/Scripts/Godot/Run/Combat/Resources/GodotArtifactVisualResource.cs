// Original Authors - Wyatt Senalik
using Godot;

namespace EternalWar.Engine.Godot.Run.Combat.Resources
{
    [GlobalClass]
    public sealed partial class GodotArtifactVisualResource : Resource
    {
        public string DefinitionId => _definitionId ?? string.Empty;
        public string DisplayName => _displayName ?? string.Empty;

        [Export] private string? _definitionId;
        [Export] private string? _displayName;
    }
}
