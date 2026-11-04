// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionRangeType
{
    public sealed class SelectionRangeTypeHandler_Steps : ISelectionRangeTypeHandler
    {
        private sealed record SteppedToGridPosition(GridPosition Position, int StepDistance);

        public GridPositionGroup GetTilesInRange(SelectionRangeTypeHandlerContext context)
        {
            GameDatabase database = context.Database;
            CombatState combatState = context.CombatState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            SelectionSpecificationsInfo selectionSpecifications = context.SelectionSpecifications;
            bool shouldCheckForValidOccupation = context.ShouldCheckForValidOccupation;
            Func<GridPosition, bool> isTileInRangeValidFunction = context.IsTileInRangeValidFunction;

            int tileRange = selectionSpecifications.Range;
            HitRulesInfo occupiedHitRules = selectionSpecifications.OccupiedHitRules;

            UnitState user = combatState.GetUnit(userIdentifier);
            GridPosition userPosition = user.GridPosition;
            Vector2Int userPositionVector = userPosition.Position;

            SteppedToGridPosition steppedToUserPosition = new(userPosition, StepDistance: 0);

            using PooledList<SteppedToGridPosition> positionsToCheck = new();
            AddAdjacentPositions(database, combatState, steppedToUserPosition, positionsToCheck);
            using PooledList<SteppedToGridPosition> newValidPositionsThisIteration = new();
            using PooledList<GridPosition> validPositions = new();
            using PooledHashSet<GridPosition> previouslyCheckedPositions = new() { userPosition };
            for (int i = 0; i < tileRange; ++i)
            {
                // Check all positions in the list for this iteration
                while (positionsToCheck.Count > 0)
                {
                    int lastIndex = positionsToCheck.Count - 1;
                    SteppedToGridPosition steppedToPosition = positionsToCheck[lastIndex];
                    GridPosition position = steppedToPosition.Position;
                    if (previouslyCheckedPositions.Contains(steppedToPosition.Position))
                    {
                        positionsToCheck.RemoveAt(lastIndex);
                        continue;
                    }

                    if (CombatSelectionRulesHelpers.IsPositionValid(database, combatState, userIdentifier, position, occupiedHitRules, shouldCheckForValidOccupation, isTileInRangeValidFunction))
                        newValidPositionsThisIteration.Add(steppedToPosition);

                    previouslyCheckedPositions.Add(position);
                    positionsToCheck.RemoveAt(lastIndex);
                }

                foreach (SteppedToGridPosition newValidPosition in newValidPositionsThisIteration)
                {
                    // Too expensive to get to this tile
                    if (newValidPosition.StepDistance > tileRange)
                        continue;

                    // Add valid positions found this iteration
                    if (!validPositions.Contains(newValidPosition.Position))
                        validPositions.Add(newValidPosition.Position);

                    // Add new positions to check for next iteration
                    AddAdjacentPositions(database, combatState, newValidPosition, positionsToCheck);
                }
                newValidPositionsThisIteration.Clear();
            }
            ValueArray<GridPosition> immutableValidPosition = validPositions.ToValueArray();

            return new GridPositionGroup(immutableValidPosition);
        }

        private static void AddAdjacentPositions(GameDatabase database, CombatState combatState, SteppedToGridPosition stepppedToCenterPosition, ICollection<SteppedToGridPosition> outCollection)
        {
            stepppedToCenterPosition.Deconstruct(out GridPosition centerPosition, out int stepDistance);

            GridPosition newPositionToCheck = centerPosition.Add(-1, 0);
            int stepCostOfNewCheckTile = DeterminePositionStepCost(database, combatState, newPositionToCheck);
            outCollection.Add(new SteppedToGridPosition(newPositionToCheck, stepDistance + stepCostOfNewCheckTile));

            newPositionToCheck = centerPosition.Add(1, 0);
            stepCostOfNewCheckTile = DeterminePositionStepCost(database, combatState, newPositionToCheck);
            outCollection.Add(new SteppedToGridPosition(newPositionToCheck, stepDistance + stepCostOfNewCheckTile));

            newPositionToCheck = centerPosition.Add(0, -1);
            stepCostOfNewCheckTile = DeterminePositionStepCost(database, combatState, newPositionToCheck);
            outCollection.Add(new SteppedToGridPosition(newPositionToCheck, stepDistance + stepCostOfNewCheckTile));

            newPositionToCheck = centerPosition.Add(0, 1);
            stepCostOfNewCheckTile = DeterminePositionStepCost(database, combatState, newPositionToCheck);
            outCollection.Add(new SteppedToGridPosition(newPositionToCheck, stepDistance + stepCostOfNewCheckTile));
        }

        private static int DeterminePositionStepCost(GameDatabase database, CombatState combatState, GridPosition position)
        {
            string combatDefinitionId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            GridBlocksInfo gridBlocks = combatDefinition.GridBlocks;

            return gridBlocks.GetStepCostAtPosition(database, position);
        }
    }
}
