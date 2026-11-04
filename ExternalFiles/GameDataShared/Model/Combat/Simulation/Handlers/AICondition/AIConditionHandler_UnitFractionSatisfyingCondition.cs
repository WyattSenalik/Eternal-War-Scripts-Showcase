// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    internal sealed class AIConditionHandler_UnitFractionSatisfyingCondition : AIConditionHandler<AIConditionInfo_UnitFractionSatisfyingCondition>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_UnitFractionSatisfyingCondition conditionInfo)
        {
            CombatState previousState = context.PreviousState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            TeamRelation relation = conditionInfo.Relation;
            AIConditionInfo unitCondition = conditionInfo.Condition;

            int eligibleUnitCount = 0;
            int satisfyingUnitCount = 0;
            foreach (UnitState candidateUnit in previousState.UnitMap.Values)
            {
                if (candidateUnit.IsDead() || previousState.DetermineRelationBetween(userIdentifier, candidateUnit.UniqueIdentifier) != relation)
                    continue;

                ++eligibleUnitCount;
                AIConditionHandlerContext candidateContext = context with { SubjectIdentifier = candidateUnit.UniqueIdentifier };
                IAIConditionHandler conditionHandler = CombatHandlersRegistration.GetHandler(unitCondition);
                if (conditionHandler.DoesSatisfy(candidateContext, unitCondition))
                    ++satisfyingUnitCount;
            }

            if (eligibleUnitCount <= 0)
                return false;

            float satisfyingFraction = (float)satisfyingUnitCount / eligibleUnitCount;
            return conditionInfo.FractionComparison.Evaluate(satisfyingFraction);
        }
    }
}
