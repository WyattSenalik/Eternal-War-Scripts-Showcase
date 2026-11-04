// Original Authors - Wyatt Senalik
using EVLibrary.Godot.Extensions;
using Godot;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatUnitView(Node3D visualInstance)
    {
        public Node3D VisualInstance { get; } = visualInstance;
        public AnimationPlayer? AnimationPlayer { get; } = visualInstance.FindChildOfType<AnimationPlayer>();
    }
}
