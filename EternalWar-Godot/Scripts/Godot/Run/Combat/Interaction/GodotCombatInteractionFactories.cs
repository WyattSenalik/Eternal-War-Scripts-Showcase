// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Interaction.Actions;
using GodotExtensions;

namespace EternalWar.Engine.Godot.Run.Combat.Interaction
{
    public sealed class GodotCombatInteractionFactories
    {
        public ButtonFactory<string> CombatActionButtonFactory => _combatActionButtonFactory;

        private readonly ButtonFactory<string> _combatActionButtonFactory;
        private readonly ICombatInteractionActions _combatActions;

        public GodotCombatInteractionFactories(GodotGameVisualDatabase visualDatabase, ICombatInteractionActions combatActions)
        {
            _combatActionButtonFactory = new ButtonFactory<string>(visualDatabase.CombatActionButtonPrefab, OnButtonPressed_CombatAction);
            _combatActions = combatActions;
        }

        private void OnButtonPressed_CombatAction(string combatActionSequenceId)
        {
            _combatActions.ButtonPressed_CombatAction(combatActionSequenceId);
        }
    }
}
