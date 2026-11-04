// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Combat.Actions;
using EternalWar.Client.Application.Run.Combat.Controllers;
using EternalWar.Client.Application.Run.Map.Actions;
using EternalWar.Client.Application.Run.Map.Controllers;

namespace EternalWar.Client.Application.Run.Controllers
{
    internal sealed record RunActions(ICombatActions CombatActions, IMapActions MapActions);

    internal sealed class RunController
    {
        public ICombatActions CombatActions => RunActions.CombatActions;
        public IMapActions MapActions => RunActions.MapActions;
        public RunActions RunActions { get; }

        public RunController(ClientGameSession clientSession)
        {
            CombatController combatController = new(clientSession);
            MapController mapController = new(clientSession);

            RunActions = new RunActions(combatController, mapController);
        }
    }
}
