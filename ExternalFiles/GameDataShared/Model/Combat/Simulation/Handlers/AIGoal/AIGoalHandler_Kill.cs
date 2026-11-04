// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Events;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal sealed class AIGoalHandler_Kill : AIGoalHandler<AIGoalInfo_Kill>
    {
        protected override EvaluationResult Evaluate(AIGoalHandlerContext context, AIGoalInfo_Kill killInfo)
        {
            ValueArray<CombatEvent> relevantEvents = context.RelevantEvents;
            // TODO: Guard against user killing itself?

            // Heuristics
            int killCount = 0;
            int totalDamageDealt = 0;
            foreach (CombatEvent combatEvent in relevantEvents)
            {
                if (combatEvent is CombatEvent_UnitDied)
                {
                    ++killCount;
                    continue;
                }
                if (combatEvent is CombatEvent_DamageApplied damageAppliedEvent)
                {
                    totalDamageDealt += damageAppliedEvent.DamageDealt;
                    continue;
                }
            }

            if (killCount <= 0)
                return EvaluationResult.Failure;

            ValueArray<int> heuristics = new(killCount, totalDamageDealt);
            return new EvaluationResult(heuristics);
        }
    }
}