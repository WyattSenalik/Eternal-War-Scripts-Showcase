// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Interaction.Actions;
using EternalWar.Engine.Godot.Run.Combat.Nodes;

namespace EternalWar.Engine.Godot.Run.Combat.Interaction
{
    public static class GodotCombatUIBinding
    {
        public static void Bind(GodotCombatUINodeCollection combatNodeCollection, ICombatInteractionActions combatInteractionActions)
        {
            GodotCombatActionsUINodeCollection combatActionsUINodeCollection = combatNodeCollection.CombatActionsNodeCollection;
            GodotEndUINodeCollection endUINodeCollection = combatNodeCollection.EndNodeCollection;

            combatActionsUINodeCollection.EndTurnButton.Pressed += combatInteractionActions.ButtonPressed_EndTurn;
            endUINodeCollection.DefeatMainMenuButton.Pressed += combatInteractionActions.ButtonPressed_MainMenu;
        }
    }
}
