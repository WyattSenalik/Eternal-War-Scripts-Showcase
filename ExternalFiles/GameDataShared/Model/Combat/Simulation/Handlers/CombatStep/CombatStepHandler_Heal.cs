// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep
{
    public sealed class CombatStepHandler_Heal : CombatStepHandler<CombatStepInfo_Heal>
    {
        protected override void Simulate(CombatStepSimulationContext context, CombatStepInfo_Heal healInfo)
        {
            CombatCommandProcessor commandProcessor = context.CommandProcessor;
            GameDatabase database = commandProcessor.Database;
            CombatInstance combatInstance = commandProcessor.CombatInstance;
            HitInfo hitInfo = healInfo.HitInfo;
            HitRulesInfo hitRules = hitInfo.HitRules;
            ActionExecutionContext actionExecutionContext = context.ActionContext;
            CombatUnitUniqueIdentifier userIdentifier = actionExecutionContext.UserIdentifier;
            string actionSequenceId = actionExecutionContext.ActionSequenceId;
            int selectionGroupIndex = context.SelectionGroupIndex;
            int step = context.Step;

            ActionSequenceTimingContext actionTimingContext = new(actionSequenceId, step);
            IEnumerable<CombatUnitUniqueIdentifier> targetIdentifiers = CombatTargetingHelpers.GetTargets(database, commandProcessor.CurrentState, actionExecutionContext, selectionGroupIndex, healInfo, hitRules, userIdentifier);
            foreach (CombatUnitUniqueIdentifier singleTargetIdentifier in targetIdentifiers)
            {
                CombatCommand_ApplyHealing healCommand = CreateHealCommand(commandProcessor.CurrentState, database, combatInstance, userIdentifier, singleTargetIdentifier, hitInfo, actionTimingContext, context.PassiveStacks);
                commandProcessor.Apply(healCommand);
            }
        }

        public static CombatCommand_ApplyHealing CreateHealCommand(CombatState combatState, GameDatabase database, CombatInstance combatInstance, CombatUnitUniqueIdentifier userIdentifier, CombatUnitUniqueIdentifier targetIdentifier, HitInfo hitInfo, ActionSequenceTimingContext actionTimingContext, int? passiveStacks = null)
        {
            UnitState target = combatState.GetUnit(targetIdentifier);
            int targetCurrentHealth = target.CurrentHealth;

            CombatStats targetStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, targetIdentifier);
            HealthInfo maxHealthInfo = targetStats.MaxHealthInfo;
            if (maxHealthInfo.HasInfiniteHealth)
                return new CombatCommand_ApplyHealing(userIdentifier, actionTimingContext, userIdentifier, targetIdentifier, 0, int.MaxValue);   // TODO: Still apply any other bonuses that may resolve from this.

            CombatStats userStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, userIdentifier);
            ValueFunctionContext valueFunctionContext = new(userStats, passiveStacks);
            float healAmountRaw = hitInfo.GetValue(valueFunctionContext);
            int healAmount = (int)MathF.Floor(healAmountRaw);

            int newHealth = targetCurrentHealth + healAmount;
            int targetMaxHealth = maxHealthInfo.Health;
            int resultingHealth = Math.Clamp(newHealth, 0, targetMaxHealth);
            int actualAmountRestored = resultingHealth - targetCurrentHealth;

            return new CombatCommand_ApplyHealing(userIdentifier, actionTimingContext, userIdentifier, targetIdentifier, actualAmountRestored, resultingHealth);
        }
    }
}


