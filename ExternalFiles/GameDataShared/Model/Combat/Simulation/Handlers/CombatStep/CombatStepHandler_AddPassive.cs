// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
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
    public sealed class CombatStepHandler_AddPassive : CombatStepHandler<CombatStepInfo_AddPassive>
    {
        protected override void Simulate(CombatStepSimulationContext context, CombatStepInfo_AddPassive addPassiveInfo)
        {
            CombatCommandProcessor commandProcessor = context.CommandProcessor;
            GameDatabase database = commandProcessor.Database;
            CombatInstance combatInstance = commandProcessor.CombatInstance;
            string passiveId = addPassiveInfo.PassiveId;
            NCalcFunctionInfo stacksToAddCalcFunction = addPassiveInfo.StacksToAddCalcFunction;
            HitRulesInfo hitRules = addPassiveInfo.HitRules;
            ActionExecutionContext actionExecutionContext = context.ActionContext;
            CombatUnitUniqueIdentifier userIdentifier = actionExecutionContext.UserIdentifier;
            string actionSequenceId = actionExecutionContext.ActionSequenceId;
            int selectionGroupIndex = context.SelectionGroupIndex;
            int step = context.Step;

            PassiveDefinition passiveDefinition = database.GetPassiveDefinition(passiveId);
            bool canStack = passiveDefinition.CanStack;

            CombatStats userStats = CombatStatHelpers.CalculateStats(database, combatInstance, commandProcessor.CurrentState, userIdentifier);
            ValueFunctionContext valueFunctionContext = context.CreateValueFunctionContext(userStats);
            float stacksToAddRaw = stacksToAddCalcFunction.Evaluate(valueFunctionContext);
            int stacksToAdd = (int)MathF.Floor(stacksToAddRaw);

            IEnumerable<CombatUnitUniqueIdentifier> targetIdentifiers = CombatTargetingHelpers.GetTargets(database, commandProcessor.CurrentState, actionExecutionContext, selectionGroupIndex, addPassiveInfo, hitRules, userIdentifier);
            ActionSequenceTimingContext actionTimingContext = new(actionSequenceId, step);
            foreach (CombatUnitUniqueIdentifier singleTargetIdentifier in targetIdentifiers)
            {
                if (!CanApplyPassive(database, commandProcessor.CurrentState, singleTargetIdentifier, passiveId))
                    continue;

                CombatCommand_AddPassive addPassiveCommand = CreatePassiveAddedCommand(database, commandProcessor.CurrentState, userIdentifier, singleTargetIdentifier, passiveId, canStack, stacksToAdd, actionTimingContext);
                commandProcessor.Apply(addPassiveCommand);
            }
        }

        private static CombatCommand_AddPassive CreatePassiveAddedCommand(GameDatabase database, CombatState combatState, CombatUnitUniqueIdentifier sourceIdentifier, CombatUnitUniqueIdentifier targetIdentifier, string passiveId, bool canStack, int stacksToAdd, ActionSequenceTimingContext actionTimingContext)
        {
            UnitState target = combatState.GetUnit(targetIdentifier);
            ValueArray<PassiveState> activePassives = target.ActivePassives;

            int existingIndex = activePassives.FindIndexOfPassive(passiveId);
            PassiveState resultingPassive;
            if (existingIndex >= 0)
            {
                PassiveState existingPassive = activePassives[existingIndex];
                // TODO: Maybe also add a check for "should reset expiration/occurrences on double application"
                if (!canStack)
                {
                    resultingPassive = existingPassive;
                }
                else
                {
                    resultingPassive = existingPassive with
                    {
                        Stacks = existingPassive.Stacks + stacksToAdd,
                    };
                }
            }
            else
            {
                resultingPassive = new PassiveState(passiveId, Activations: 0, stacksToAdd, ExpirationProgresses: ValueArray<PassiveExpirationProgress>.Empty);
            }
            return new CombatCommand_AddPassive(sourceIdentifier, actionTimingContext, sourceIdentifier, targetIdentifier, passiveId, stacksToAdd, resultingPassive);
        }

        private static bool CanApplyPassive(GameDatabase database, CombatState combatState, CombatUnitUniqueIdentifier unitIdentifier, string passiveDefinitionId)
        {
            UnitState unit = combatState.GetUnit(unitIdentifier);
            IReadOnlyList<string> activeNonStackableEffectIds = unit.ActiveNonStackableEffectIds;
            foreach (string nonStackableEffectId in activeNonStackableEffectIds)
            {
                NonStackableEffectDefinition nonStackableEffectDefinition = database.GetNonStackableEffectDefinition(nonStackableEffectId);
                if (nonStackableEffectDefinition.Info is not NonStackableEffectInfo_ImmuneToSpecificPassive immuneToSpecificPassiveInfo)
                    continue;
                string immuneToPassiveId = immuneToSpecificPassiveInfo.PassiveId;
                if (immuneToPassiveId == passiveDefinitionId)
                    return false;
            }
            return true;
        }
    }
}


