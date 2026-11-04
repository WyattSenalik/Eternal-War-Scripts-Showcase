// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Handlers;
using EternalWar.Core.Model.Combat.Simulation.Handlers.UnitDecisionController;
using EternalWar.Core.Model.Run.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatAISimulationHelpers
    {
        public static AITurnSimulationResult? SimulateAITurn(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier)
        {
            UnitDefinition userDefinition = database.GetUnitDefinition(userIdentifier);
            UnitDecisionControllerInfo userDecisionControllerInfo = userDefinition.DecisionControllerInfo;
            IUnitDecisionControllerHandler decisionControllerHandler = CombatHandlersRegistration.GetHandler(userDecisionControllerInfo);

            return decisionControllerHandler.SimulateTurn(database, randomState, combatInstance, combatState, userIdentifier);
        }
    }
}
