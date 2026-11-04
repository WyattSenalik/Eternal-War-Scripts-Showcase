// Original Authors - Wyatt Senalik
namespace EternalWar.Client.Application.Run.Combat.Interaction.Actions
{
    public interface ICombatInteractionActions
    {
        void ButtonPressed_EndTurn();
        void ButtonPressed_CombatAction(string combatActionSequenceId);
        void ButtonPressed_MainMenu();
        // TODO: More probably
    }
}
