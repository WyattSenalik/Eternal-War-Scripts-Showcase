// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed class AIConditionHandler_And : AIConditionHandler<AIConditionInfo_And>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_And conditionInfo)
        {
            ValueArray<AIConditionInfo> conditions = conditionInfo.Conditions;
            foreach (AIConditionInfo subConditionInfo in conditions)
            {
                IAIConditionHandler subHandler = CombatHandlersRegistration.GetHandler(subConditionInfo);
                if (!subHandler.DoesSatisfy(context, subConditionInfo))
                    return false;
            }
            return true;
        }
    }
}
