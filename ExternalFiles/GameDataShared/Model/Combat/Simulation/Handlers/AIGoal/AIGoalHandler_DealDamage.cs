// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Events;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal sealed class AIGoalHandler_DealDamage : AIGoalHandler<AIGoalInfo_DealDamage>
    {
        protected override EvaluationResult Evaluate(AIGoalHandlerContext context, AIGoalInfo_DealDamage goalInfo)
        {
            ValueArray<CombatEvent> relevantEvents = context.RelevantEvents;

            int totalDamageDealt = 0;
            int totalRemainingHealth = 0;
            using PooledHashSet<CombatUnitUniqueIdentifier> damagedUnitIdentifiers = new();
            foreach (CombatEvent combatEvent in relevantEvents)
            {
                if (combatEvent is not CombatEvent_DamageApplied damageEvent)
                    continue;

                int damageDealt = damageEvent.DamageDealt;
                if (damageDealt <= 0)
                    continue;

                totalDamageDealt += damageDealt;
                totalRemainingHealth += damageEvent.ResultingHealth;
                damagedUnitIdentifiers.Add(damageEvent.TargetIdentifier);
            }

            if (totalDamageDealt <= 0)
                return EvaluationResult.Failure;

            int damagedUnitCount = damagedUnitIdentifiers.Count;
            // Prefer more damage, then dealing more to less units, then leaving targets as low as possible.
            ValueArray<int> heuristics = new(totalDamageDealt, -damagedUnitCount, -totalRemainingHealth);

            return new EvaluationResult(heuristics);
        }
    }
}