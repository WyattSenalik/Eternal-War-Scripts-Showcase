// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Actions;
using EternalWar.Client.Application.Run.Combat.Interaction.Actions;
using EternalWar.Core.Helpers;

namespace EternalWar.Client.Application.Run.Combat.Interaction.Controllers
{
    internal sealed class CombatInteractionController(ICombatActions combatActions) : ICombatInteractionActions
    {
        private readonly ICombatActions _combatActions = combatActions;

        public void ButtonPressed_EndTurn()
        {
            _combatActions.EndTurn();
        }

        public void ButtonPressed_CombatAction(string combatActionSequenceId)
        {
            Debug.Print($"TODO: Tile Selection for {combatActionSequenceId}");
        }

        public void ButtonPressed_MainMenu()
        {
            // This is a whole thing with handling people disconnecting probably
            Debug.Print($"TODO: Return to main menu");
        }
    }
}
