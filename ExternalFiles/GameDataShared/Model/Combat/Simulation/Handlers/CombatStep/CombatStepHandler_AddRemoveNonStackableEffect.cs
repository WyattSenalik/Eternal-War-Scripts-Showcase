// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep
{
    public sealed class CombatStepHandler_AddRemoveNonStackableEffect : CombatStepHandler<CombatStepInfo_AddRemoveNonStackableEffect>
    {
        protected override void Simulate(CombatStepSimulationContext context, CombatStepInfo_AddRemoveNonStackableEffect addRemoveNonStackableEffectInfo)
        {
            CombatCommandProcessor commandProcessor = context.CommandProcessor;
            GameDatabase database = commandProcessor.Database;
            HitRulesInfo hitRules = addRemoveNonStackableEffectInfo.HitRules;
            string nonStackableEffectId = addRemoveNonStackableEffectInfo.NonStackableEffectId;
            AddRemoveType addRemoveType = addRemoveNonStackableEffectInfo.AddRemoveType;
            ActionExecutionContext actionExecutionContext = context.ActionContext;
            CombatUnitUniqueIdentifier userIdentifier = actionExecutionContext.UserIdentifier;
            string actionSequenceId = actionExecutionContext.ActionSequenceId;
            int selectionGroupIndex = context.SelectionGroupIndex;
            int step = context.Step;

            ActionSequenceTimingContext actionTimingContext = new(actionSequenceId, step);
            IEnumerable<CombatUnitUniqueIdentifier> targetIdentifiers = CombatTargetingHelpers.GetTargets(database, commandProcessor.CurrentState, actionExecutionContext, selectionGroupIndex, addRemoveNonStackableEffectInfo, hitRules, userIdentifier);
            foreach (CombatUnitUniqueIdentifier singleTargetIdentifier in targetIdentifiers)
            {
                switch (addRemoveType)
                {
                    case AddRemoveType.Add:
                        AddNonStackableEffectToTarget(commandProcessor, userIdentifier, singleTargetIdentifier, nonStackableEffectId, actionTimingContext);
                        break;
                    case AddRemoveType.Remove:
                        RemoveNonStackableEffectFromTarget(commandProcessor, userIdentifier, singleTargetIdentifier, nonStackableEffectId, actionTimingContext);
                        break;
                    default:
                        throw new NotImplementedException($"Unhandled {nameof(AddRemoveType)}: ({addRemoveType})");
                }
            }
        }

        private static void AddNonStackableEffectToTarget(CombatCommandProcessor commandProcessor, CombatUnitUniqueIdentifier sourceIdentifier, CombatUnitUniqueIdentifier targetIdentifier, string nonStackableEffectId, ActionSequenceTimingContext actionTimingContext)
        {
            UnitState target = commandProcessor.CurrentState.GetUnit(targetIdentifier);
            IReadOnlyList<string> activeNonStackableEffectIds = target.ActiveNonStackableEffectIds;

            if (activeNonStackableEffectIds.Contains(nonStackableEffectId))
                return;

            CombatCommand_AddNonStackableEffect effectAddedCommand = new(sourceIdentifier, actionTimingContext, sourceIdentifier, targetIdentifier, nonStackableEffectId);
            commandProcessor.Apply(effectAddedCommand);
        }

        private static void RemoveNonStackableEffectFromTarget(CombatCommandProcessor commandProcessor, CombatUnitUniqueIdentifier sourceIdentifier, CombatUnitUniqueIdentifier targetIdentifier, string nonStackableEffectId, ActionSequenceTimingContext actionTimingContext)
        {
            UnitState target = commandProcessor.CurrentState.GetUnit(targetIdentifier);
            IReadOnlyList<string> activeNonStackableEffectIds = target.ActiveNonStackableEffectIds;

            if (!activeNonStackableEffectIds.Contains(nonStackableEffectId))
                return;

            CombatCommand_RemoveNonStackableEffect effectRemovedCommand = new(sourceIdentifier, actionTimingContext, sourceIdentifier, targetIdentifier, nonStackableEffectId);
            commandProcessor.Apply(effectRemovedCommand);
        }
    }
}


