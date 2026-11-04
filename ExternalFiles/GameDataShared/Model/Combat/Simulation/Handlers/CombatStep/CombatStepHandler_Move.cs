// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Helpers;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep
{
    public sealed class CombatStepHandler_Move : CombatStepHandler<CombatStepInfo_Move>
    {
        protected override void Simulate(CombatStepSimulationContext context, CombatStepInfo_Move moveInfo)
        {
            CombatCommandProcessor commandProcessor = context.CommandProcessor;
            GameDatabase database = commandProcessor.Database;
            ActionExecutionContext actionContext = context.ActionContext;
            CombatUnitUniqueIdentifier userIdentifier = actionContext.UserIdentifier;
            string actionSequenceId = actionContext.ActionSequenceId;
            int selectionGroupIndex = context.SelectionGroupIndex;
            int step = context.Step;

            UnitState user = commandProcessor.CurrentState.GetUnit(userIdentifier);
            GridPosition userStartingPosition = user.GridPosition;
            TeamAffiliation userAffiliation = user.CurrentAffiliation;

            GridPosition finalMoveToPosition = CombatTargetingHelpers.GetSingleTargetPosition(database, commandProcessor.CurrentState, actionContext, selectionGroupIndex, moveInfo);
            ActionSequenceTimingContext actionTimingContext = new(actionSequenceId, step);
            // TODO: Pass in the already calculated path from the selection?
            if (!GridPathingHelpers.TryToCalculatePath(database, commandProcessor.CurrentState, userStartingPosition, finalMoveToPosition, userAffiliation, out Path? path))
            {
                // Failed to calculate path, so just teleport there.
                Debug.Print($"Failed to calculate path from {userStartingPosition} to {finalMoveToPosition} for {userIdentifier}");
                CombatCommand_MoveUnit instantlyMovedCommand = new(userIdentifier, actionTimingContext, userIdentifier, userStartingPosition, finalMoveToPosition, MovementType.Teleport, IsFirstMove: true, IsLastMove: true);
                commandProcessor.Apply(instantlyMovedCommand);
                return;
            }

            // Step through each of the positions one-by-one
            ValueArray<GridPosition> positionsOnPath = path.Positions;
            EightDirection previousFacingDirection = user.FacingDirection;
            for (int i = 1; i < positionsOnPath.Count; ++i)
            {
                GridPosition previousPosition = positionsOnPath[i - 1];
                GridPosition nextPosition = positionsOnPath[i];

                // Potentially update facing direction before beginning to move
                Vector2Int positionDifference = previousPosition.Position - nextPosition.Position;
                EightDirection nextFacingDirection = positionDifference.GetClosestEightDirection();
                if (previousFacingDirection != nextFacingDirection)
                {
                    CombatCommand_ChangeFacing facingChangedCommand = new(userIdentifier, actionTimingContext, userIdentifier, previousFacingDirection, nextFacingDirection);
                    commandProcessor.Apply(facingChangedCommand);

                    previousFacingDirection = nextFacingDirection;
                }

                CombatCommand_MoveUnit movedCommand = new(userIdentifier, actionTimingContext, userIdentifier, previousPosition, nextPosition, MovementType.Walk, IsFirstMove: i == 1, IsLastMove: i == positionsOnPath.Count - 1);
                commandProcessor.Apply(movedCommand);

                // TODO: Need to handle things that could happen when stepping onto a tile (like stepping on a fire tile or something).
            }
        }
    }
}


