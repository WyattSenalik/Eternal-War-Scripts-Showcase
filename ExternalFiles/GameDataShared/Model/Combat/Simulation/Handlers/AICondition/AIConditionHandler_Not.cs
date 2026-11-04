// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed class AIConditionHandler_Not : AIConditionHandler<AIConditionInfo_Not>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_Not conditionInfo)
        {
            AIConditionInfo subConditionInfo = conditionInfo.Condition;
            IAIConditionHandler subHandler = CombatHandlersRegistration.GetHandler(subConditionInfo);
            return !subHandler.DoesSatisfy(context, subConditionInfo);
        }
    }
}
