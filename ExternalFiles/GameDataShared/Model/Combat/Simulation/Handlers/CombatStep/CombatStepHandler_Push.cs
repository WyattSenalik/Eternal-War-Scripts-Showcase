// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Handlers.PushDirection;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System.Collections.Generic;
using Direction = EternalWar.Core.Model.Combat.Definitions.PushDirection;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep
{
    public sealed class CombatStepHandler_Push : CombatStepHandler<CombatStepInfo_Push>
    {
        protected override void Simulate(CombatStepSimulationContext context, CombatStepInfo_Push pushInfo)
        {
            CombatCommandProcessor commandProcessor = context.CommandProcessor;
            GameDatabase database = commandProcessor.Database;
            CombatInstance combatInstance = commandProcessor.CombatInstance;
            HitRulesInfo hitRules = pushInfo.HitRules;
            Direction direction = pushInfo.Direction;
            int tilesToPush = pushInfo.TilesToPush;
            ActionExecutionContext actionExecutionContext = context.ActionContext;
            CombatUnitUniqueIdentifier userIdentifier = actionExecutionContext.UserIdentifier;
            string actionSequenceId = actionExecutionContext.ActionSequenceId;
            int selectionGroupIndex = context.SelectionGroupIndex;
            int step = context.Step;

            IPushDirectionHandler pushDirectionHandler = CombatHandlersRegistration.GetHandler(direction);

            ActionSequenceTimingContext actionTimingContext = new(actionSequenceId, step);
            IEnumerable<CombatUnitUniqueIdentifier> targetIdentifiers = CombatTargetingHelpers.GetTargets(database, commandProcessor.CurrentState, actionExecutionContext, selectionGroupIndex, pushInfo, hitRules, userIdentifier);

            CombatCommand_PerformShove shoveCommand = new(userIdentifier, actionTimingContext);
            commandProcessor.Apply(shoveCommand);

            foreach (CombatUnitUniqueIdentifier singleTargetIdentifier in targetIdentifiers)
            {
                string targetDefinitionId = singleTargetIdentifier.DefinitionId;

                // TODO: Eventually move this check into a helper that also checks if the unit is rooted, immune to pushing, etc. Maybe CombatSimulationHelpers.CanBePushed
                UnitDefinition targetDefinition = database.GetUnitDefinition(targetDefinitionId);
                if (!targetDefinition.CanBeMoved)
                    continue;

                UnitState target = commandProcessor.CurrentState.GetUnit(singleTargetIdentifier);
                GridPosition targetOriginalPosition = target.GridPosition;

                PushDirectionContext pushDirectionContext = new(database, combatInstance, commandProcessor.CurrentState, userIdentifier, singleTargetIdentifier, tilesToPush);
                GridPosition finalPushPosition = pushDirectionHandler.DeterminePosition(pushDirectionContext);

                EightDirection newTargetFacingDirection = (finalPushPosition.Position - targetOriginalPosition.Position).GetClosestEightDirection();

                CombatCommand_ChangeFacing changeFacingCommand = new(userIdentifier, actionTimingContext, singleTargetIdentifier, target.FacingDirection, newTargetFacingDirection);
                commandProcessor.Apply(changeFacingCommand);

                // Push one tile at a time. TODO: Make it so all pushes are done at once?
                for (int i = 0; i < tilesToPush; ++i)
                {
                    float t0 = ((float)i) / tilesToPush;
                    GridPosition fromPosition = targetOriginalPosition.Interpolate(finalPushPosition, t0);
                    float t1 = ((float)(i + 1)) / tilesToPush;
                    GridPosition toPosition = targetOriginalPosition.Interpolate(finalPushPosition, t1);

                    CombatCommand_MoveUnit pushedCommand = new(userIdentifier, actionTimingContext, singleTargetIdentifier, fromPosition, toPosition, MovementType.Push, IsFirstMove: i == 0, IsLastMove: i == tilesToPush - 1);
                    commandProcessor.Apply(pushedCommand);
                }
            }
        }
    }
}


