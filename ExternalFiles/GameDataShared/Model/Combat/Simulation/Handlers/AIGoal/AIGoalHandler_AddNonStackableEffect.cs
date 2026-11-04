// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Events;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal sealed class AIGoalHandler_AddNonStackableEffect : AIGoalHandler<AIGoalInfo_AddNonStackableEffect>
    {
        protected override EvaluationResult Evaluate(AIGoalHandlerContext context, AIGoalInfo_AddNonStackableEffect goalInfo)
        {
            string desiredEffectId = goalInfo.EffectId;
            ValueArray<CombatEvent> relevantEvents = context.RelevantEvents;

            using PooledHashSet<CombatUnitUniqueIdentifier> effectedUnitIdentifiers = new();
            foreach (CombatEvent combatEvent in relevantEvents)
            {
                if (combatEvent is CombatEvent_NonStackableEffectAdded effectAddedEvent)
                {
                    string addedEffectId = effectAddedEvent.EffectId;
                    if (addedEffectId != desiredEffectId)
                        continue;

                    effectedUnitIdentifiers.Add(effectAddedEvent.TargetIdentifier);
                }
                else if (combatEvent is CombatEvent_NonStackableEffectRemoved effectRemovedEvent)
                {
                    string removedEffectId = effectRemovedEvent.EffectId;
                    if (removedEffectId != desiredEffectId)
                        continue;

                    effectedUnitIdentifiers.Remove(effectRemovedEvent.TargetIdentifier);
                }
            }

            int effectedUnitCount = effectedUnitIdentifiers.Count;
            if (effectedUnitCount <= 0)
                return EvaluationResult.Failure;

            ValueArray<int> heuristics = new(effectedUnitCount);
            return new EvaluationResult(heuristics);
        }
    }
}