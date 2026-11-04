// Original Authors - Wyatt Senalik
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using Godot;
using GodotExtensions;
using System.Collections.Generic;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatView_UI_CombatActions
    {
        private readonly List<Button> _buttons = new();
        private readonly GodotCombatActionsUINodeCollection _combatActionsUINodeCollectionInstance;
        private readonly ButtonFactory<string> _combatActionButtonFactory;

        public GodotCombatView_UI_CombatActions(GodotCombatActionsUINodeCollection combatActionsUINodeCollectionInstance, ButtonFactory<string> combatActionButtonFactory)
        {
            _combatActionsUINodeCollectionInstance = combatActionsUINodeCollectionInstance;
            _combatActionButtonFactory = combatActionButtonFactory;
        }

        public void CreateActionButton(string combatActionSequenceId)
        {
            Node actionButtonsParentNode = _combatActionsUINodeCollectionInstance.ActionButtonsParentNode;
            Button newActionButton = _combatActionButtonFactory.Create(combatActionSequenceId, actionButtonsParentNode);
            newActionButton.Text = combatActionSequenceId;

            _buttons.Add(newActionButton);
        }

        public void DeleteAllActionButtons()
        {
            foreach (Button button in _buttons)
            {
                button.QueueFree();
            }
            _buttons.Clear();
        }

        public void ShowEndTurnButton()
        {
            _combatActionsUINodeCollectionInstance.EndTurnButton.Show();
        }

        public void HideEndTurnButton()
        {
            _combatActionsUINodeCollectionInstance.EndTurnButton.Hide();
        }
    }
}
