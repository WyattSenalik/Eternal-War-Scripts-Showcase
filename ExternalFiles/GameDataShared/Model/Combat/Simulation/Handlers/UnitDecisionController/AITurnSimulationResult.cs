// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.UnitDecisionController
{
    public sealed record AITurnSimulationResult(CombatSimulationResult CombatSimulationResult, ValueArray<ActionExecutionContext> ExecutionContexts);

    public static class AITurnSimulationResultExtensions
    {
        public static AITurnSimulationResult Append(this AITurnSimulationResult firstResult, AITurnSimulationResult secondResult)
        {
            CombatSimulationResult firstCombatSimulationResult = firstResult.CombatSimulationResult;
            CombatSimulationResult secondCombatSimulationResult = secondResult.CombatSimulationResult;
            CombatSimulationResult combinedCombatSimulationResult = firstCombatSimulationResult.Append(secondCombatSimulationResult);

            ValueArray<ActionExecutionContext> firstExecutionContexts = firstResult.ExecutionContexts;
            ValueArray<ActionExecutionContext> secondExecutionContexts = secondResult.ExecutionContexts;
            ValueArray<ActionExecutionContext> combinedExecutionContexts = firstExecutionContexts.Add(secondExecutionContexts);

            return new AITurnSimulationResult(combinedCombatSimulationResult, combinedExecutionContexts);
        }
    }
}