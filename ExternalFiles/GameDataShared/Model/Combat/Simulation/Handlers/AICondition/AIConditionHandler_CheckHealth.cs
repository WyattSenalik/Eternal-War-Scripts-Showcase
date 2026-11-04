// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed class AIConditionHandler_CheckHealth : AIConditionHandler_HasSubject<AIConditionInfo_CheckHealth>
    {
        protected override bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo_CheckHealth conditionInfo, CombatUnitUniqueIdentifier subjectIdentifier)
        {
            GameDatabase database = context.Database;
            CombatInstance combatInstance = context.CombatInstance;
            CombatState combatState = context.PreviousState;
            ValueAmountPredicate comparison = conditionInfo.Comparison;

            UnitState subject = combatState.GetUnit(subjectIdentifier);
            int subjectCurrentHealth = subject.CurrentHealth;

            CombatStats subjectStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, subjectIdentifier);
            HealthInfo subjectMaxHealthInfo = subjectStats.MaxHealthInfo;
            int subjectMaxHealth = subjectMaxHealthInfo.Health;

            return comparison.Evaluate(subjectMaxHealth, subjectCurrentHealth);
        }
    }
}
