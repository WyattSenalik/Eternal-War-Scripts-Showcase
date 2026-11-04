// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Run.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.UnitDecisionController
{
    internal sealed class UnitDecisionControllerHandler_None : UnitDecisionControllerHandler<UnitDecisionControllerInfo_None>
    {
        protected override AITurnSimulationResult? SimulateTurn(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, UnitDecisionControllerInfo_None decisionControllerInfo)
        {
            return null;
        }
    }
}