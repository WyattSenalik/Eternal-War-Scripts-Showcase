// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Combat.Actions;
using EternalWar.Client.Application.Run.Combat.Interaction.Actions;
using EternalWar.Client.Application.Run.Combat.Interaction.Controllers;
using EternalWar.Client.Application.Run.Controllers;
using EternalWar.Client.Application.Run.Interaction.Actions;
using EternalWar.Client.Application.Run.Map.Actions;
using EternalWar.Client.Application.Run.Map.Interaction.Actions;
using EternalWar.Client.Application.Run.Map.Interaction.Controllers;

namespace EternalWar.Client.Application.Run.Interaction.Controllers
{
    internal sealed class RunInteractionController : IRunInteractionActions
    {
        public ICombatInteractionActions CombatActions => _combatInteractionController;
        public IMapInteractionActions MapActions => _mapInteractionController;

        private readonly CombatInteractionController _combatInteractionController;
        private readonly MapInteractionController _mapInteractionController;

        public RunInteractionController(ClientGameSession clientSession, RunActions runActions)
        {
            ICombatActions combatActions = runActions.CombatActions;
            IMapActions mapActions = runActions.MapActions;

            _combatInteractionController = new CombatInteractionController(combatActions);
            _mapInteractionController = new MapInteractionController(mapActions);
        }
    }
}
