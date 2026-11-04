// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using EternalWar.Core.Model.Random;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep
{
    public sealed class CombatStepHandler_Melee : CombatStepHandler<CombatStepInfo_Melee>
    {
        protected override void Simulate(CombatStepSimulationContext context, CombatStepInfo_Melee meleeInfo)
        {
            CombatCommandProcessor commandProcessor = context.CommandProcessor;
            GameDatabase database = commandProcessor.Database;
            CombatInstance combatInstance = commandProcessor.CombatInstance;
            DamageInfo damageInfo = meleeInfo.DamageInfo;
            HitInfo hitInfo = damageInfo.HitInfo;
            HitRulesInfo hitRules = hitInfo.HitRules;
            int moveAccuracy = meleeInfo.BaseAccuracy;
            IRandomResolutionResolver randomResolver = commandProcessor.RandomResolver;
            ActionExecutionContext actionExecutionContext = context.ActionContext;
            CombatUnitUniqueIdentifier userIdentifier = actionExecutionContext.UserIdentifier;
            string actionSequenceId = actionExecutionContext.ActionSequenceId;
            int selectionGroupIndex = context.SelectionGroupIndex;
            int step = context.Step;

            ActionSequenceTimingContext actionTimingContext = new(actionSequenceId, step);
            IEnumerable<CombatUnitUniqueIdentifier> targetIdentifiers = CombatTargetingHelpers.GetTargets(database, commandProcessor.CurrentState, actionExecutionContext, selectionGroupIndex, meleeInfo, hitRules, userIdentifier);
            IEnumerable<GridPosition> targetPositions = targetIdentifiers
                .Select(singleTargetIdentifier => commandProcessor.CurrentState.GetUnit(singleTargetIdentifier).GridPosition)
                .Distinct();
            CombatCommand_PerformAttack attackSwungCommand = new(userIdentifier, actionTimingContext, targetPositions.ToValueArray());
            commandProcessor.Apply(attackSwungCommand);
            foreach (CombatUnitUniqueIdentifier singleTargetIdentifier in targetIdentifiers)
            {
                int hitChance = CombatHitHelpers.CalculateHitChance(database, combatInstance, commandProcessor.CurrentState, userIdentifier, singleTargetIdentifier, moveAccuracy);
                HitRollResult rollResult = randomResolver.RollHitChance(hitChance);
                bool didHit = rollResult.DidHit;
                if (!didHit)
                {
                    CombatCommand_ResolveAttackMiss attackMissedCommand = new(userIdentifier, actionTimingContext, singleTargetIdentifier);
                    commandProcessor.Apply(attackMissedCommand);
                    continue;
                }
                bool didCrit = rollResult.DidCrit;

                CaculateDamage(database, combatInstance, commandProcessor.CurrentState, userIdentifier, singleTargetIdentifier, damageInfo, didCrit, out int damageDealt, out int resultingHealth, context.PassiveStacks);

                bool wasFatal = resultingHealth <= 0;
                CombatCommand_ResolveAttackHit attackHitCommand = new(userIdentifier, actionTimingContext, singleTargetIdentifier, didCrit, wasFatal, damageInfo.DamageType, damageDealt, resultingHealth);
                commandProcessor.Apply(attackHitCommand);

                UnitState target = commandProcessor.CurrentState.GetUnit(singleTargetIdentifier);
                if (target.IsDead())
                {
                    CombatCommand_NotifyUnitDied unitDiedCommand = new(userIdentifier, actionTimingContext, singleTargetIdentifier);
                    commandProcessor.Apply(unitDiedCommand);
                }
            }
        }

        private static void CaculateDamage(GameDatabase database, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, CombatUnitUniqueIdentifier targetIdentifier, DamageInfo damageInfo, bool didCrit, out int damageDealt, out int resultingHealth, int? passiveStacks = null)
        {
            UnitState target = combatState.GetUnit(targetIdentifier);
            CombatStats targetStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, targetIdentifier);
            HealthInfo targetHealthInfo = targetStats.MaxHealthInfo;

            bool hasInfiniteHealth = targetHealthInfo.HasInfiniteHealth;
            if (hasInfiniteHealth)
            {
                damageDealt = 0;
                resultingHealth = targetHealthInfo.Health;
                return;
            }

            int currentTargetHealth = target.CurrentHealth;
            int damageToDeal = CombatHitHelpers.CalculateDamageToDeal(database, combatInstance, combatState, userIdentifier, targetIdentifier, damageInfo, didCrit, passiveStacks);
            resultingHealth = Math.Max(0, currentTargetHealth - damageToDeal);
            damageDealt = currentTargetHealth - resultingHealth;
        }
    }
}


