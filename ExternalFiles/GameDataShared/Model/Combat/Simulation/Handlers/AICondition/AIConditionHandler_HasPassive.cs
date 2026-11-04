// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed class AIConditionHandler_HasPassive : AIConditionHandler_HasSubject<AIConditionInfo_HasPassive>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_HasPassive conditionInfo, CombatUnitUniqueIdentifier subjectIdentifier)
        {
            CombatState combatState = context.PreviousState;
            string passiveId = conditionInfo.PassiveId;

            UnitState subject = combatState.GetUnit(subjectIdentifier);

            return subject.HasPassive(passiveId);
        }
    }
}