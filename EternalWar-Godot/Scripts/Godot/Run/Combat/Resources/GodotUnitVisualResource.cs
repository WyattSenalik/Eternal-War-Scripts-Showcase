// Original Authors - Wyatt Senalik
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Combat.Resources
{
    [GlobalClass]
    public sealed partial class GodotUnitVisualResource : Resource
    {
        public string DefinitionId => _definitionId ?? string.Empty;
        public PackedScene AnimationPrefab => _animationPrefab ?? throw new ArgumentNullException(nameof(_animationPrefab));
        public Texture2D Icon => _icon ?? throw new ArgumentNullException(nameof(_icon));
        public GodotUnitAnimationSetResource? AnimationSet => _animationSet;

        [Export] private string? _definitionId;
        [Export] private PackedScene? _animationPrefab;
        [Export] private Texture2D? _icon;
        [Export] private GodotUnitAnimationSetResource? _animationSet;
    }
}
