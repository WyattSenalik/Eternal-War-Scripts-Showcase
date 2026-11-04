// Original Authors - Wyatt Senalik
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Combat.Nodes
{
    public sealed partial class GodotEndUINodeCollection : Node
    {
        public Control DefeatScreenRootNode => _defeatScreenRootNode ?? throw new ArgumentNullException(nameof(_defeatScreenRootNode));
        public Control VictoryScreenRootNode => _victoryScreenRootNode ?? throw new ArgumentNullException(nameof(_victoryScreenRootNode));
        public Button DefeatMainMenuButton => _defeatMainMenuButton ?? throw new ArgumentNullException(nameof(_defeatMainMenuButton));

        [Export] private Control? _defeatScreenRootNode;
        [Export] private Control? _victoryScreenRootNode;
        [Export] private Button? _defeatMainMenuButton;
    }
}
