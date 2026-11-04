// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Events;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal sealed class AIGoalHandler_AddPassive : AIGoalHandler<AIGoalInfo_AddPassive>
    {
        protected override EvaluationResult Evaluate(AIGoalHandlerContext context, AIGoalInfo_AddPassive goalInfo)
        {
            string desiredPassiveId = goalInfo.PassiveId;
            ValueArray<CombatEvent> relevantEvents = context.RelevantEvents;

            int totalStacksAdded = 0;
            using PooledHashSet<CombatUnitUniqueIdentifier> grantedUnitIdentifiers = new();
            foreach (CombatEvent combatEvent in relevantEvents)
            {
                if (combatEvent is not CombatEvent_PassiveAdded passiveAddedEvent)
                    continue;

                string addedPassiveId = passiveAddedEvent.PassiveId;
                if (addedPassiveId != desiredPassiveId)
                    continue;

                int stacksAdded = passiveAddedEvent.StacksAdded;
                if (stacksAdded <= 0)
                    continue;

                totalStacksAdded += stacksAdded;
                grantedUnitIdentifiers.Add(passiveAddedEvent.TargetIdentifier);
            }

            if (totalStacksAdded <= 0)
                return EvaluationResult.Failure;

            int grantedUnitCount = grantedUnitIdentifiers.Count;
            // Prefer spreading the passive across units over piling stacks onto one unit.
            ValueArray<int> heuristics = new(grantedUnitCount, totalStacksAdded);
            return new EvaluationResult(true, heuristics);
        }
    }
}