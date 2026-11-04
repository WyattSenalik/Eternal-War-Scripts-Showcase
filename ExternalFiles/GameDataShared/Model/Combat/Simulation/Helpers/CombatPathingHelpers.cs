// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatPathingHelpers
    {
        private sealed record ReachablePosition(GridPosition GridPosition, int ReachCost, bool CanStopOn);

        public static List<GridPosition> DetermineReachablePositions(GameDatabase database, CombatState combatState, GridPosition startPosition, int tileDistance, TeamAffiliation affiliation, bool includeStarting = false)
        {
            string combatDefinitionId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            GridBlocksInfo gridBlocks = combatDefinition.GridBlocks;

            ReachablePosition startingNode = new(startPosition, ReachCost: 0, CanStopOn: includeStarting);
            List<GridPosition> reachablePositions = new();
            using PooledList<ReachablePosition> inProgressNodes = new() { startingNode };
            using PooledHashSet<GridPosition> alreadyTestedPositions = new();

            while (inProgressNodes.Count > 0)
            {
                ReachablePosition currentNode = FindNodeWithLowestReachCost(inProgressNodes, out int foundIndex);
                GridPosition currentGridPosition = currentNode.GridPosition;
                bool canStopOn = currentNode.CanStopOn;
                if (canStopOn)
                    reachablePositions.Add(currentGridPosition);
                inProgressNodes.RemoveAt(foundIndex);
                alreadyTestedPositions.Add(currentGridPosition);

                GridPosition leftPosition = currentGridPosition + new Vector2Int(-1, 0);
                _TryToQueuePositionForTesting(leftPosition, currentNode);

                GridPosition rightPosition = currentGridPosition + new Vector2Int(1, 0);
                _TryToQueuePositionForTesting(rightPosition, currentNode);

                GridPosition downPosition = currentGridPosition + new Vector2Int(0, -1);
                _TryToQueuePositionForTesting(downPosition, currentNode);

                GridPosition upPosition = currentGridPosition + new Vector2Int(0, 1);
                _TryToQueuePositionForTesting(upPosition, currentNode);
            }
            return reachablePositions;


            void _TryToQueuePositionForTesting(GridPosition testPosition, ReachablePosition currentNode)
            {
                if (!ShouldTestPosition(database, combatState, testPosition, currentNode, startPosition, alreadyTestedPositions, inProgressNodes, affiliation))
                    return;

                // TODO: Have some step cost tracking in CombatState?
                int stepCost = gridBlocks.GetStepCostAtPosition(database, testPosition);
                int totalReachCost = currentNode.ReachCost + stepCost;
                if (totalReachCost > tileDistance)
                    return;
                bool canStopOn = !CombatGridHelpers.IsTileBlocked(database, combatState, testPosition);
                ReachablePosition newNode = new(testPosition, totalReachCost, canStopOn);
                inProgressNodes.Add(newNode);
            }
        }

        private static bool ShouldTestPosition(GameDatabase database, CombatState combatState, GridPosition testPosition, ReachablePosition currentNode, GridPosition startPosition, PooledHashSet<GridPosition> alreadyTestedPositions, IReadOnlyList<ReachablePosition> inProgressNodes, TeamAffiliation affiliation, bool skipEndPositionUnit = false, GridPosition endPosition = new())
        {
            string combatDefinitionId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;
            GridBlocksInfo gridBlocks = combatDefinition.GridBlocks;

            // Not on the grid.
            if (!tileGrid.IsPositionOnGrid(testPosition)) 
                return false;
            // Too drastic a height change to walk this way.
            GridPosition currentPosition = currentNode.GridPosition;
            if (gridBlocks.IsHeightChangeDrastic(database, testPosition, currentPosition))
                return false;
            // Already tested.
            if (alreadyTestedPositions.Contains(testPosition))
                return false;
            // Already queued up to test.
            if (inProgressNodes.Any(node => node.GridPosition == testPosition))
                return false;
            // If its the start position, then of course someone is occupying it, so skip that step.
            if (testPosition == startPosition)
                return true;
            if (skipEndPositionUnit && testPosition == endPosition)
            {
                Debug.PrintError("Skipping end position unit.");
                return true;
            }
            // Check for non-friendlies occupying that tile.
            IEnumerable<UnitState> unitsAtPosition = combatState.GetAllUnitsAtPosition(testPosition);
            foreach (UnitState blockingUnit in unitsAtPosition)
            {
                CombatUnitUniqueIdentifier unitIdentifier = blockingUnit.UniqueIdentifier;
                string unitDefinitionId = unitIdentifier.DefinitionId;

                UnitDefinition unitDefinition = database.GetUnitDefinition(unitDefinitionId);
                if (!unitDefinition.ShouldBlockOccupiedTile)
                    continue;

                TeamAffiliation unitAffiliation = blockingUnit.CurrentAffiliation;
                TeamRelation relation = unitAffiliation.DetermineRelation(affiliation);
                if (relation == TeamRelation.Friendly)
                    continue;
                return false;
            }
            return true;
        }

        private static ReachablePosition FindNodeWithLowestReachCost(IReadOnlyList<ReachablePosition> reachablePositions, out int index)
        {
            ReachablePosition lowestNode = reachablePositions[0];
            float lowestCost = float.PositiveInfinity;
            index = -1;
            for (int i = 0; i < reachablePositions.Count; ++i)
            {
                ReachablePosition node = reachablePositions[i];
                if (node.ReachCost >= lowestCost)
                    continue;
                lowestNode = node;
                lowestCost = node.ReachCost;
                index = i;
            }
            return lowestNode;
        }
    }
}
