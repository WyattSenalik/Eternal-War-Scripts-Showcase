// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using EternalWar.Core.Model.Run.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.UnitDecisionController
{
    internal sealed record GoalOrientedActionEvaluation(int GoalPriority, EvaluationResult Result) : IComparable<GoalOrientedActionEvaluation>
    {
        public static readonly GoalOrientedActionEvaluation Failure = new(int.MaxValue, EvaluationResult.Failure);

        public int CompareTo(GoalOrientedActionEvaluation? other)
        {
            if (other == null)
                return 1;

            // Lower priority is desired
            int priorityComparison = other.GoalPriority.CompareTo(GoalPriority);

            return priorityComparison != 0 ? priorityComparison : Result.CompareTo(other.Result);
        }
    }

    internal sealed class UnitDecisionControllerHandler_GoalOriented : UnitDecisionControllerHandler<UnitDecisionControllerInfo_GoalOriented>
    {
        protected override AITurnSimulationResult? SimulateTurn(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, UnitDecisionControllerInfo_GoalOriented decisionControllerInfo)
        {
            AITurnSimulationResult? accumulatedSimulationResult = null;
            CombatState currentCombatState = combatState;
            int round = 0;
            while (true)
            {
                //Debug.Print($"[{userIdentifier}] Round {round}");
                if (!TryFindBestAction(database, randomState, combatInstance, currentCombatState, userIdentifier, decisionControllerInfo, out AITurnSimulationResult? actionSimulationResult))
                {
                    //Debug.Print($"Exited at Round {round}. Failed to find best action");
                    break;
                }

                CombatSimulationResult actionCombatSimulationResult = actionSimulationResult.CombatSimulationResult;
                CombatState stateAfterStep = actionCombatSimulationResult.FinalState;
                // An action that accomplishes a goal without mutating state would loop forever.
                if (ReferenceEquals(stateAfterStep, currentCombatState))
                {
                    //Debug.Print($"Exited at Round {round}. Avoided infinite loop\nCurrent State:\n{GameDataSerialization.Serialize(currentCombatState)}\n\nPost-Step State:\n{GameDataSerialization.Serialize(stateAfterStep)}");
                    break;
                }

                accumulatedSimulationResult = accumulatedSimulationResult == null ? actionSimulationResult : accumulatedSimulationResult.Append(actionSimulationResult);
                currentCombatState = stateAfterStep;

                randomState = accumulatedSimulationResult.CombatSimulationResult.FinalRandomState;

                ++round;
            }

            //Debug.Print($"[{userIdentifier}] Best compounded result ({accumulatedSimulationResult?.ExecutionContexts})");
            return accumulatedSimulationResult;
        }

        private static bool TryFindBestAction(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, UnitDecisionControllerInfo_GoalOriented decisionControllerInfo, [NotNullWhen(true)] out AITurnSimulationResult? bestSimulationResult)
        {
            ValueArray<AIGoalInfo> goals = decisionControllerInfo.Goals;

            UnitBuild userBuild = combatInstance.GetUnitBuild(userIdentifier);
            UnitState user = combatState.GetUnit(userIdentifier);

            int remainingActionPoints = user.CurrentActionPoints;
            ValueArray<string> allActionIds = userBuild.GetAllActionSequenceIds(database);

            AITurnSimulationResult? currentBestSimulationResult = null;
            GoalOrientedActionEvaluation currentBestEvaluation = GoalOrientedActionEvaluation.Failure;
            foreach (string actionSequenceId in allActionIds)
            {
                CombatActionSequenceDefinition actionDefinition = database.GetActionSequenceDefinition(actionSequenceId);
                int actionPointCost = actionDefinition.ActionPointCost;
                if (remainingActionPoints < actionPointCost)
                {
                    //Debug.Print($"[{userIdentifier}] Too expensive: using {actionSequenceId}({actionPointCost}). Only have {remainingActionPoints})");
                    continue;
                }

                List<(GridPositionGroup Group, int Amount)> selections = CombatSelectionHelpers.GetSelectionsForAction(database, combatInstance, combatState, userIdentifier, actionDefinition);
                //Debug.Print($"[{userIdentifier}] Action {actionSequenceId} has these selections available ({string.Join(", ", selections)})");

                CombatSelectionHelpers.ForEachSelectionCombination(database, combatInstance, combatState, userIdentifier, actionDefinition, concreteSelections =>
                {
                    ActionSelectionContext selectionContext = new(concreteSelections);
                    ActionExecutionContext executionContext = new(actionSequenceId, userIdentifier, selectionContext);
                    CombatSimulationResult actionSimulationResult = CombatSimulator.Simulate(database, randomState, combatInstance, combatState, executionContext);

                    int currentBestGoalPriority = currentBestEvaluation.GoalPriority;
                    GoalOrientedActionEvaluation candidateEvaluation = EvaluateGoals(database, combatInstance, combatState, userIdentifier, goals, actionSimulationResult, currentBestGoalPriority);
                    if (candidateEvaluation.CompareTo(currentBestEvaluation) <= 0)
                    {
                        //Debug.Print($"[{userIdentifier}] Worse, ignoring: using {actionSequenceId}, with selections ({concreteSelections}) (eval: {candidateEvaluation}). Current best ({currentBestEvaluation})");
                        return;
                    }

                    //Debug.Print($"[{userIdentifier}] New best: using ({actionSequenceId}) with selections ({concreteSelections}). Previous best ({currentBestEvaluation})");
                    ValueArray<ActionExecutionContext> candidateExecutionContexts = new(executionContext);
                    currentBestEvaluation = candidateEvaluation;
                    currentBestSimulationResult = new AITurnSimulationResult(actionSimulationResult, candidateExecutionContexts);
                });
            }

            bestSimulationResult = currentBestSimulationResult;
            //Debug.Print($"[{userIdentifier}] Best result ({currentBestSimulationResult?.ExecutionContexts})");
            return bestSimulationResult != null;
        }

        private static GoalOrientedActionEvaluation EvaluateGoals(GameDatabase database, CombatInstance combatInstance, CombatState evaluationState, CombatUnitUniqueIdentifier userIdentifier, ValueArray<AIGoalInfo> goals, CombatSimulationResult candidateSimulationResult, int currentBestGoalPriority)
        {
            AIGoalTargetLocator targetLocator = new(database, combatInstance, evaluationState, userIdentifier);

            int goalPriority = 0;
            foreach (AIGoalInfo goalInfo in goals)
            {
                // Goals of lesser value priority (higher number) can never beat what was already found.
                if (goalPriority > currentBestGoalPriority)
                    break;

                ValueArray<CombatEvent> relevantEvents = GatherEventsWithEffectedUnitsThatSatisfyConditions(database, combatInstance, evaluationState, candidateSimulationResult, userIdentifier, goalInfo);
                IAIGoalHandler handler = CombatHandlersRegistration.GetHandler(goalInfo);

                EvaluationResult evaluationResult;
                if (goalInfo is AIGoalInfo_MoveCloserToTarget)
                {
                    evaluationResult = EvaluateMoveCloserGoalForEachSubject(handler, goalInfo, userIdentifier, targetLocator, relevantEvents, candidateSimulationResult.EventLog);
                }
                else
                {
                    AIGoalHandlerContext goalHandlerContext = new(userIdentifier, targetLocator, relevantEvents, candidateSimulationResult.EventLog, null);
                    evaluationResult = handler.Evaluate(goalHandlerContext, goalInfo);
                }

                // The first goal that succeeds is the most valuable one this candidate can accomplish.
                if (evaluationResult.DidSucceed)
                {
                    //Debug.Print($"[{userIdentifier}] Succeeded for goal ({goalPriority} {goalInfo.GetType()}).");
                    return new GoalOrientedActionEvaluation(goalPriority, evaluationResult);
                }
                //Debug.Print($"[{userIdentifier}] Failed for goal ({goalPriority} {goalInfo.GetType()}).");

                ++goalPriority;
            }

            return GoalOrientedActionEvaluation.Failure;
        }

        private static EvaluationResult EvaluateMoveCloserGoalForEachSubject(IAIGoalHandler handler, AIGoalInfo goalInfo, CombatUnitUniqueIdentifier userIdentifier, IAIGoalTargetLocator targetLocator, ValueArray<CombatEvent> relevantEvents, ValueArray<CombatEvent> candidateEvents)
        {
            EvaluationResult bestEvaluation = EvaluationResult.Failure;
            List<CombatUnitUniqueIdentifier> evaluatedSubjectIdentifiers = new();

            foreach (CombatEvent combatEvent in relevantEvents)
            {
                CombatUnitUniqueIdentifier? subjectIdentifier = combatEvent.GetEffectedUnit();
                if (subjectIdentifier == null || evaluatedSubjectIdentifiers.Contains(subjectIdentifier))
                    continue;

                evaluatedSubjectIdentifiers.Add(subjectIdentifier);
                ValueArray<CombatEvent> subjectEvents = GatherEventsForSubject(relevantEvents, subjectIdentifier);
                AIGoalHandlerContext goalHandlerContext = new(userIdentifier, targetLocator, subjectEvents, candidateEvents, subjectIdentifier);
                EvaluationResult evaluation = handler.Evaluate(goalHandlerContext, goalInfo);
                if (evaluation.CompareTo(bestEvaluation) > 0)
                    bestEvaluation = evaluation;
            }

            return bestEvaluation;
        }

        private static ValueArray<CombatEvent> GatherEventsForSubject(ValueArray<CombatEvent> events, CombatUnitUniqueIdentifier subjectIdentifier)
        {
            using ValueArray<CombatEvent>.Builder subjectEventBuilder = new(events.Count);
            foreach (CombatEvent combatEvent in events)
            {
                if (combatEvent.GetEffectedUnit() == subjectIdentifier)
                    subjectEventBuilder.Add(combatEvent);
            }

            return subjectEventBuilder.ToValueArray();
        }

        private static ValueArray<CombatEvent> GatherEventsWithEffectedUnitsThatSatisfyConditions(GameDatabase database, CombatInstance combatInstance, CombatState previousState, CombatSimulationResult candidateSimulationResult, CombatUnitUniqueIdentifier userIdentifier, AIGoalInfo goalInfo)
        {
            ValueArray<CombatEvent> eventLog = candidateSimulationResult.EventLog;
            int eventCount = eventLog.Count;

            using ValueArray<CombatEvent>.Builder relevantEventBuilder = new(eventCount);
            foreach (CombatEvent combatEvent in eventLog)
            {
                CombatUnitUniqueIdentifier? subjectIdentifier = combatEvent.GetEffectedUnit();
                if (subjectIdentifier == null)
                    continue;

                // Ignore the dead (TODO: Allow this sometimes if going to try and revive friendly units?)
                UnitState subject = previousState.GetUnit(subjectIdentifier);
                if (subject.IsDead())
                    continue;

                AIConditionHandlerContext conditionHandlerContext = new(database, combatInstance, previousState, candidateSimulationResult, userIdentifier, subjectIdentifier);
                if (!DoesSatisfyAllConditions(goalInfo, conditionHandlerContext))
                    continue;

                relevantEventBuilder.Add(combatEvent);
            }

            return relevantEventBuilder.ToValueArray();
        }

        private static bool DoesSatisfyAllConditions(AIGoalInfo goalInfo, AIConditionHandlerContext conditionHandlerContext)
        {
            ValueArray<AIConditionInfo> conditions = goalInfo.Conditions;
            foreach (AIConditionInfo conditionInfo in conditions)
            {
                IAIConditionHandler conditionHandler = CombatHandlersRegistration.GetHandler(conditionInfo);
                if (!conditionHandler.DoesSatisfy(conditionHandlerContext, conditionInfo))
                    return false;
            }

            return true;
        }
    }
}
