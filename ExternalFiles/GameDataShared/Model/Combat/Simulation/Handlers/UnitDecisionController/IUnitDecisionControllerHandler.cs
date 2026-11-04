// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.UnitDecisionController
{
    public interface IUnitDecisionControllerHandler
    {
        Type ControllerType { get; }

        AITurnSimulationResult? SimulateTurn(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier);
    }

    internal abstract class UnitDecisionControllerHandler<TDecisionControllerInfo> : IUnitDecisionControllerHandler where TDecisionControllerInfo : UnitDecisionControllerInfo
    {
        public Type ControllerType => typeof(TDecisionControllerInfo);

        AITurnSimulationResult? IUnitDecisionControllerHandler.SimulateTurn(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier)
        {
            UnitDefinition userDefinition = database.GetUnitDefinition(userIdentifier);
            UnitDecisionControllerInfo decisionControllerInfo = userDefinition.DecisionControllerInfo;
            return decisionControllerInfo is TDecisionControllerInfo castedInfo 
                ? SimulateTurn(database, randomState, combatInstance, combatState, userIdentifier, castedInfo) 
                : throw new InvalidCastException($"{GetType().Name} cannot evaluate a {decisionControllerInfo.GetType().Name} for unit ({userIdentifier})");
        }

        protected abstract AITurnSimulationResult? SimulateTurn(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, TDecisionControllerInfo decisionControllerInfo);
    }
}