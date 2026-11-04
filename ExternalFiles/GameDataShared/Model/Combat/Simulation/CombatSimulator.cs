// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Handlers;
using EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using EternalWar.Core.Model.Run.Runtime;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation
{
    public static class CombatSimulator
    {
        public static CombatSimulationResult Simulate(GameDatabase database, DeterministicRandomState randomState, CombatRuntime combatRuntime, ActionExecutionContext actionContext, SimulationOptions? simulationOptionsOverride = null) => Simulate(database, randomState, combatRuntime.Instance, combatRuntime.State, actionContext, simulationOptionsOverride);

        public static CombatSimulationResult Simulate(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, ActionExecutionContext actionContext, SimulationOptions? simulationOptionsOverride = null)
        {
            string actionSequenceId = actionContext.ActionSequenceId;
            CombatActionSequenceDefinition actionSequenceDefinition = database.GetActionSequenceDefinition(actionSequenceId);

            using CombatCommandPipeline commandPipeline = new(database, randomState, combatInstance, combatState, simulationOptionsOverride);
            SimulateActionSequence(commandPipeline.Processor, actionContext, actionSequenceDefinition, spendActionPoints: true, passiveStacks: null);

            CombatState finalState = commandPipeline.CurrentState;
            DeterministicRandomState finalRandomState = commandPipeline.CurrentRandomState;
            ValueArray<CombatCommand> commandLog = commandPipeline.BuildCommandLog();
            ValueArray<CombatEvent> eventLog = commandPipeline.BuildEventLog();
            return new CombatSimulationResult(finalState, finalRandomState, commandLog, eventLog);
        }

        internal static void SimulatePassiveAction(CombatCommandProcessor commandProcessor, ActionExecutionContext actionContext, CombatActionSequenceDefinition actionSequenceDefinition, int? passiveStacks)
        {
            SimulateActionSequence(commandProcessor, actionContext, actionSequenceDefinition, spendActionPoints: false, passiveStacks: passiveStacks);
        }

        private static void SimulateActionSequence(CombatCommandProcessor commandProcessor, ActionExecutionContext actionContext, CombatActionSequenceDefinition actionSequenceDefinition, bool spendActionPoints, int? passiveStacks)
        {
            string actionSequenceId = actionContext.ActionSequenceId;
            CombatUnitUniqueIdentifier userUniqueIdentifier = actionContext.UserIdentifier;
            IReadOnlyList<CombatStepInfo> actionSteps = actionSequenceDefinition.Steps;

            CombatCommand_BeginAction beginActionCommand;
            CombatState combatState = commandProcessor.CurrentState;
            UnitState user = combatState.GetUnit(userUniqueIdentifier);
            int userCurrentActionPoints = user.CurrentActionPoints;
            if (spendActionPoints)
            {
                int actionPointCost = actionSequenceDefinition.ActionPointCost;
                int resultingActionPoints = userCurrentActionPoints - actionPointCost;
                beginActionCommand = new CombatCommand_BeginAction(userUniqueIdentifier, actionPointCost, resultingActionPoints);
            }
            else
            {
                beginActionCommand = new CombatCommand_BeginAction(userUniqueIdentifier, ActionPointsSpent: 0, userCurrentActionPoints);
            }
            commandProcessor.Apply(beginActionCommand);

            int stepIndex = 0;
            int selectionStepIndex = 0;
            foreach (CombatStepInfo stepInfo in actionSteps)
            {
                ActionSequenceTimingContext actionTimingContext = new(actionSequenceId, stepIndex);
                CombatCommand_BeginActionStep stepStartedCommand = new(userUniqueIdentifier, actionTimingContext);
                commandProcessor.Apply(stepStartedCommand);
                HandleFaceTarget(actionContext, commandProcessor, selectionStepIndex, stepInfo, userUniqueIdentifier, actionTimingContext);

                ICombatStepHandler stepHandler = CombatHandlersRegistration.GetHandler(stepInfo);
                CombatStepSimulationContext stepSimulationContext = new(commandProcessor, actionContext, stepIndex, selectionStepIndex, passiveStacks);
                stepHandler.Simulate(stepSimulationContext, stepInfo);

                CombatCommand_CompleteActionStep stepEndedCommand = new(userUniqueIdentifier, actionTimingContext);
                commandProcessor.Apply(stepEndedCommand);

                ++stepIndex;
                if (stepInfo.DoesUseSelection())
                    ++selectionStepIndex;
            }

            commandProcessor.Apply(CombatCommand_CompleteAction.Instance);
        }

        private static void HandleFaceTarget(ActionExecutionContext actionContext, CombatCommandProcessor commandProcessor, int selectionGroupIndex, CombatStepInfo stepInfo, CombatUnitUniqueIdentifier userIdentifier, ActionSequenceTimingContext actionTimingContext)
        {
            if (!stepInfo.ShouldUserFaceTarget)
                return;

            GameDatabase database = commandProcessor.Database;
            CombatState combatState = commandProcessor.CurrentState;
            UnitState user = combatState.GetUnit(userIdentifier);

            GridPosition centerGridPosition = CombatTargetingHelpers.GetCenterPositionOfTargets(database, combatState, actionContext, selectionGroupIndex, stepInfo);
            Vector2Int positionDifference = user.GridPosition.Position - centerGridPosition.Position;
            EightDirection nextFacingDirection = positionDifference.GetClosestEightDirection();

            CombatCommand_ChangeFacing facingChangedCommand = new(userIdentifier, actionTimingContext, userIdentifier, user.FacingDirection, nextFacingDirection);
            commandProcessor.Apply(facingChangedCommand);
        }
    }
}


