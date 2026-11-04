// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Interaction.Actions;
using EternalWar.Client.Application.Run.Map.Interaction.Actions;

namespace EternalWar.Client.Application.Run.Interaction.Actions
{
    public interface IRunInteractionActions
    {
        ICombatInteractionActions CombatActions { get; }
        IMapInteractionActions MapActions { get; }
    }
}
