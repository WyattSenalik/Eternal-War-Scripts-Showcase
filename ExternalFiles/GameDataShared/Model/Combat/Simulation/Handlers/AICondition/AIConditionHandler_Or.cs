// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed class AIConditionHandler_Or : AIConditionHandler<AIConditionInfo_Or>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_Or conditionInfo)
        {
            ValueArray<AIConditionInfo> conditions = conditionInfo.Conditions;
            foreach (AIConditionInfo subConditionInfo in conditions)
            {
                IAIConditionHandler subHandler = CombatHandlersRegistration.GetHandler(subConditionInfo);
                if (subHandler.DoesSatisfy(context, subConditionInfo))
                    return true;
            }
            return false;
        }
    }
}
