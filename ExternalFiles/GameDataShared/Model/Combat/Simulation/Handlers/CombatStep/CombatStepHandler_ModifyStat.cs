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
    public sealed class CombatStepHandler_ModifyStat : CombatStepHandler<CombatStepInfo_ModifyStat>
    {
        protected override void Simulate(CombatStepSimulationContext context, CombatStepInfo_ModifyStat modifyStatInfo)
        {
            CombatCommandProcessor commandProcessor = context.CommandProcessor;
            GameDatabase database = commandProcessor.Database;
            CombatInstance combatInstance = commandProcessor.CombatInstance;
            ActionInvolvedUnit whoseStatsToUse = modifyStatInfo.WhoseStatsToUse;
            StatType statToModify = modifyStatInfo.StatToModify;
            ExpirationInfoGroup expirationInfo = modifyStatInfo.ExpirationInfo;
            HitInfo hitInfo = modifyStatInfo.HitInfo;
            HitRulesInfo hitRules = hitInfo.HitRules;
            ActionExecutionContext actionExecutionContext = context.ActionContext;
            CombatUnitUniqueIdentifier userIdentifier = actionExecutionContext.UserIdentifier;
            string actionSequenceId = actionExecutionContext.ActionSequenceId;
            int selectionGroupIndex = context.SelectionGroupIndex;
            int step = context.Step;

            ActionSequenceTimingContext actionTimingContext = new(actionSequenceId, step);
            IEnumerable<CombatUnitUniqueIdentifier> targetIdentifiers = CombatTargetingHelpers.GetTargets(database, commandProcessor.CurrentState, actionExecutionContext, selectionGroupIndex, modifyStatInfo, hitRules, userIdentifier);
            foreach (CombatUnitUniqueIdentifier singleTargetIdentifier in targetIdentifiers)
            {
                CombatUnitUniqueIdentifier unitWhoseStatsToUseIdentifier = whoseStatsToUse.GetInvolvedUnit(userIdentifier, singleTargetIdentifier);
                CombatStats unitWhoseStatsToUseStats = CombatStatHelpers.CalculateStats(database, combatInstance, commandProcessor.CurrentState, unitWhoseStatsToUseIdentifier);

                ValueFunctionContext valueFunctionContext = context.CreateValueFunctionContext(unitWhoseStatsToUseStats);
                float modifyValue = hitInfo.GetValue(valueFunctionContext);
                int roundedValue = (int)MathF.Floor(modifyValue);
                TemporaryStatModifierState statModifier = new(statToModify, roundedValue, expirationInfo, ExpirationProgress: 0);

                CombatCommand_ApplyStatModifier statModifierAppliedCommand = new(userIdentifier, actionTimingContext, userIdentifier, singleTargetIdentifier, statModifier);
                commandProcessor.Apply(statModifierAppliedCommand);
            }
        }
    }
}


