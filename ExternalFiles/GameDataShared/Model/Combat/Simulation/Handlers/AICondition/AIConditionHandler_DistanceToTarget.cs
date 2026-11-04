// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed class AIConditionHandler_DistanceToTarget : AIConditionHandler_HasSubject<AIConditionInfo_DistanceToTarget>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_DistanceToTarget conditionInfo, CombatUnitUniqueIdentifier subjectIdentifier)
        {
            GameDatabase database = context.Database;
            CombatInstance combatInstance = context.CombatInstance;
            CombatState previousCombatState = context.PreviousState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            AITargetSelectorInfo otherSelectorInfo = conditionInfo.Other;
            NumericPredicate distanceComparison = conditionInfo.DistanceComparison;
            DistanceType distanceType = conditionInfo.DistanceType;

            AITargetSelectionHandlerContext targetSelectionContext = new(database, combatInstance, previousCombatState, userIdentifier, subjectIdentifier);
            CombatUnitUniqueIdentifier? otherIdentifier = otherSelectorInfo.GetTargetIdentifier(targetSelectionContext);
            if (otherIdentifier == null)
                return false;

            float distance = previousCombatState.DistanceTo(subjectIdentifier, otherIdentifier, distanceType);
            return distanceComparison.Evaluate(distance);
        }
    }
}