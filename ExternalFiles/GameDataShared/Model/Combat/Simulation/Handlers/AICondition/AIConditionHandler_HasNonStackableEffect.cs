// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed class AIConditionHandler_HasNonStackableEffect : AIConditionHandler_HasSubject<AIConditionInfo_HasNonStackableEffect>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_HasNonStackableEffect conditionInfo, CombatUnitUniqueIdentifier subjectIdentifier)
        {
            CombatState combatState = context.PreviousState;
            string effectId = conditionInfo.EffectId;

            UnitState subject = combatState.GetUnit(subjectIdentifier);

            return subject.HasNonStackableEffect(effectId);
        }
    }
}