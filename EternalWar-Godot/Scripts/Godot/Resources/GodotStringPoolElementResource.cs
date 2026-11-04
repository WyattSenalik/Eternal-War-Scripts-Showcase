// Original Authors - Wyatt Senalik
using Godot;

namespace EternalWar.Engine.Godot.Resources
{
    [GlobalClass]
    public sealed partial class GodotStringPoolElementResource : Resource
    {
        public string Value => _value ?? string.Empty;
        public float Weight => _weight;

        [Export] private string? _value;
        [Export] private float _weight = 1.0f;
    }
}
