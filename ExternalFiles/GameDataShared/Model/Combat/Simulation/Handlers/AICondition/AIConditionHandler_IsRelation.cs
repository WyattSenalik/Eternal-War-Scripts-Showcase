// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed class AIConditionHandler_IsRelation : AIConditionHandler_HasSubject<AIConditionInfo_IsRelation>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_IsRelation conditionInfo, CombatUnitUniqueIdentifier subjectIdentifier)
        {
            CombatState combatState = context.PreviousState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            TeamRelation desiredRelation = conditionInfo.Relation;

            TeamRelation actualRelation = combatState.DetermineRelationBetween(userIdentifier, subjectIdentifier);

            return desiredRelation == actualRelation;
        }
    }
}