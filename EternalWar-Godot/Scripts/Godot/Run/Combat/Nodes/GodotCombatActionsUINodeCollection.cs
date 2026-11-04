// Original Authors - Wyatt Senalik
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Combat.Nodes
{
    public sealed partial class GodotCombatActionsUINodeCollection : Node
    {
        public Node ActionButtonsParentNode => _actionButtonsParentNode ?? throw new ArgumentNullException(nameof(_actionButtonsParentNode));
        public Button EndTurnButton => _endTurnButton ?? throw new ArgumentNullException(nameof(_endTurnButton));

        [Export] private Node? _actionButtonsParentNode;
        [Export] private Button? _endTurnButton;
    }
}
