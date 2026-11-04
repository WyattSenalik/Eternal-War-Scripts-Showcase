using Godot;
using System;

namespace EternalWar.Engine.Godot.View.Resources.Combat.Prefabs
{
    public sealed partial class GodotTurnOrderUnitUINodeCollection : Node
    {
        public TextureRect Image => _image ?? throw new ArgumentNullException(nameof(_image));
        public TextureRect Outline => _outline ?? throw new ArgumentNullException(nameof(_outline));
        public ProgressBar HealthBar => _healthBar ?? throw new ArgumentNullException(nameof(_healthBar));
        public MarginContainer HealthBarContainer => _healthBarContainer ?? throw new ArgumentNullException(nameof(_healthBarContainer));
        public AnimationPlayer AnimationPlayer => _animationPlayer ?? throw new ArgumentNullException(nameof(_animationPlayer));

        [Export] private TextureRect? _image;
        [Export] private TextureRect? _outline;
        [Export] private ProgressBar? _healthBar;
        [Export] private MarginContainer? _healthBarContainer;
        [Export] private AnimationPlayer? _animationPlayer;
    }
}
