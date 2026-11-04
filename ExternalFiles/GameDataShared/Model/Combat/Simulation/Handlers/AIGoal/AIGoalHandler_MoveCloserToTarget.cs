// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Events;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal sealed class AIGoalHandler_MoveCloserToTarget : AIGoalHandler<AIGoalInfo_MoveCloserToTarget>
    {
        protected override EvaluationResult Evaluate(AIGoalHandlerContext context, AIGoalInfo_MoveCloserToTarget goalInfo)
        {
            IAIGoalTargetLocator targetLocator = context.TargetLocator;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            ValueArray<CombatEvent> candidateEvents = context.CandidateEvents;

            AITargetSelectorInfo moverSelectorInfo = goalInfo.Mover;
            CombatUnitUniqueIdentifier originalSubjectIdentifier = context.CurrentSubjectIdentifier ?? userIdentifier;
            CombatUnitUniqueIdentifier? moverIdentifier = targetLocator.DetermineTarget(moverSelectorInfo, originalSubjectIdentifier);
            if (moverIdentifier == null)
                return EvaluationResult.Failure;

            AITargetSelectorInfo targetSelectorInfo = goalInfo.Target;
            CombatUnitUniqueIdentifier? targetIdentifier = targetLocator.DetermineTarget(targetSelectorInfo, moverIdentifier);
            if (targetIdentifier == null || targetIdentifier == moverIdentifier)
                return EvaluationResult.Failure;

            // The mover has to actually go somewhere, otherwise this goal was not accomplished.
            if (!TryFindEndingPosition(candidateEvents, moverIdentifier, out GridPosition moverEndingPosition))
                return EvaluationResult.Failure;

            GridPosition moverStartingPosition = targetLocator.DetermineStartingPosition(moverIdentifier);
            GridPosition targetStartingPosition = targetLocator.DetermineStartingPosition(targetIdentifier);
            // The target may have been pushed or teleported during the same action.
            GridPosition targetEndingPosition = TryFindEndingPosition(candidateEvents, targetIdentifier, out GridPosition movedTargetPosition) ? movedTargetPosition : targetStartingPosition;

            DistanceType distanceType = goalInfo.DistanceComparisonType;
            float startingDistance = moverStartingPosition.DistanceTo(targetStartingPosition, distanceType);
            float endingDistance = moverEndingPosition.DistanceTo(targetEndingPosition, distanceType);
            float distanceClosed = startingDistance - endingDistance;
            if (distanceClosed <= 0.0f)
                return EvaluationResult.Failure;

            // Prefer closing the most distance, then prefer ending up as close as possible.
            ValueArray<int> heuristics = new((int)(distanceClosed * 10), -(int)(endingDistance * 10));
            return new EvaluationResult(heuristics);
        }

        private static bool TryFindEndingPosition(ValueArray<CombatEvent> candidateEvents, CombatUnitUniqueIdentifier unitIdentifier, out GridPosition endingPosition)
        {
            bool didFindAnyMovement = false;
            endingPosition = default;
            foreach (CombatEvent combatEvent in candidateEvents)
            {
                if (combatEvent is not CombatEvent_Moved movedEvent)
                    continue;

                CombatUnitUniqueIdentifier movedUnitIdentifier = movedEvent.EffectedUnitIdentifier;
                if (movedUnitIdentifier != unitIdentifier)
                    continue;

                endingPosition = movedEvent.ToPosition;
                didFindAnyMovement = true;
            }

            return didFindAnyMovement;
        }
    }
}
