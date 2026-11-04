// Original Authors - Wyatt Senalik
using Godot;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatBlockView
    {
        public Node3D VisualInstance => _visualInstance;

        private readonly Node3D _visualInstance;

        public GodotCombatBlockView(Node3D visualInstance)
        {
            _visualInstance = visualInstance;
        }
    }
}
