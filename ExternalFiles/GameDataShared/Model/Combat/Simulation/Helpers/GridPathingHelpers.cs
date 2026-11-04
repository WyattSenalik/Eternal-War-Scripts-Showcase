// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class GridPathingHelpers
    {
        private sealed record ReachablePosition(GridPosition GridPosition, int ReachCost, bool CanStopOn) : IHaveGridPosition;
        private sealed record PathingNode(GridPosition GridPosition, PathingNode? Parent, int G, int H) : IHaveGridPosition
        {
            public int F => G + H;

            public PathingNode(GridPosition position) : this(position, null, 0, 0) { }
            public PathingNode(GridPosition position, PathingNode parent, GridPosition pathStartingPosition) : this(position,parent, parent.G + 1, (position.Position - pathStartingPosition.Position).LengthSquared()) { }
        }

        public static List<GridPosition> DetermineReachablePositions(GameDatabase database, CombatState combatState, GridPosition startingPosition, int tileDistance, TeamAffiliation affiliation, bool includeStarting = false)
        {
            string combatId = combatState.DefinitionId;

            ReachablePosition startingNode = new(startingPosition, ReachCost: 0, CanStopOn: includeStarting);
            List<GridPosition> reachablePositions = new();
            List<ReachablePosition> inProgressNodes = new() { startingNode };
            List<GridPosition> alreadyTestedPositions = new();

            while (inProgressNodes.Count > 0)
            {
                ReachablePosition currentNode = FindNodeWithLowestReachCost(inProgressNodes, out int foundIndex);
                GridPosition currentGridPosition = currentNode.GridPosition;
                if (currentNode.CanStopOn)
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
                if (!ShouldTestPosition(database, combatState, testPosition, currentNode, startingPosition, alreadyTestedPositions, inProgressNodes, affiliation))
                    return;

                // TODO: Make step cost dynamic and can change as things do in game.
                int stepCost = GetStepCost(database, combatId, currentNode.GridPosition);
                int totalReachCost = currentNode.ReachCost + stepCost;
                if (totalReachCost > tileDistance)
                    return;
                bool canStopOn = !CombatGridHelpers.IsTileBlocked(database, combatState, testPosition);
                ReachablePosition newNode = new(testPosition, totalReachCost, canStopOn);
                inProgressNodes.Add(newNode);
            }
        }

        public static bool TryToCalculatePath(GameDatabase database, CombatState combatState, GridPosition startingPosition, GridPosition endPosition, TeamAffiliation affiliation, [NotNullWhen(true)] out Path? path)
        {
            if (IsPathingImpossible(database, combatState, startingPosition, endPosition))
            {
                path = null;
                return false;
            }

            // We calculate from the end position backwards to the starting position
            List<PathingNode> inProgressNodes = new() { new PathingNode(endPosition) };
            List<GridPosition> alreadyTestedPositions = new();

            while (inProgressNodes.Count > 0)
            {
                PathingNode currentNode = FindNodeWithLowestF(inProgressNodes, out int currentIndex);
                GridPosition currentPosition = currentNode.GridPosition;

                inProgressNodes.RemoveAt(currentIndex);
                alreadyTestedPositions.Add(currentPosition);

                // Found the starting position, that means we've finished the pathing.
                if (currentPosition.Equals(startingPosition))
                {
                    path = BuildPath(currentNode);
                    return true;
                }

                // Haven't yet made it to the starting position, so check the children (up, down, left right)
                GridPosition leftPosition = currentPosition.Add(-1, 0);
                _TryToQueuePositionForTesting(leftPosition, currentNode);

                GridPosition rightPosition = currentPosition.Add(1, 0);
                _TryToQueuePositionForTesting(rightPosition, currentNode);

                GridPosition downPosition = currentPosition.Add(0, -1);
                _TryToQueuePositionForTesting(downPosition, currentNode);

                GridPosition upPosition = currentPosition.Add(0, 1);
                _TryToQueuePositionForTesting(upPosition, currentNode);
            }
            path = null;
            return false;


            void _TryToQueuePositionForTesting(GridPosition testPosition, PathingNode currentNode)
            {
                if (!ShouldTestPosition(database, combatState, testPosition, currentNode, startingPosition, alreadyTestedPositions, inProgressNodes, affiliation))
                    return;

                PathingNode newNode = new(testPosition, currentNode, startingPosition);
                inProgressNodes.Add(newNode);
            }
        }

        public static bool TryToCalculatePath(GameDatabase database, CombatState combatState, GridPosition startingPosition, GridPosition endPosition, TeamAffiliation affiliation, [NotNullWhen(true)] out Path? path, out int distance, out int totalStepCost)
        {
            distance = -1;
            totalStepCost = -1;
            if (IsPathingImpossible(database, combatState, startingPosition, endPosition))
            {
                path = null;
                return false;
            }

            string combatId = combatState.DefinitionId;

            // We calculate from the end position backwards to the starting position
            List<PathingNode> inProgressNodes = new() { new PathingNode(endPosition) };
            List<GridPosition> alreadyTestedPositions = new();

            while (inProgressNodes.Count > 0)
            {
                PathingNode currentNode = FindNodeWithLowestF(inProgressNodes, out int currentIndex);

                inProgressNodes.RemoveAt(currentIndex);
                alreadyTestedPositions.Add(currentNode.GridPosition);

                // Found the starting position, that means we've finished the pathing.
                if (currentNode.GridPosition.Equals(startingPosition))
                {
                    path = BuildPath(currentNode);
                    distance = path.Positions.Count - 1;
                    totalStepCost = 0;
                    foreach (GridPosition positionInPath in path.Positions)
                    {
                        totalStepCost += GetStepCost(database, combatId, positionInPath);
                    }
                    return true;
                }

                // Haven't yet made it to the starting position, so check the children (up, down, left right)
                GridPosition leftPosition = currentNode.GridPosition.Add(-1, 0);
                _TryToQueuePositionForTesting(leftPosition, currentNode);

                GridPosition rightPosition = currentNode.GridPosition.Add(1, 0);
                _TryToQueuePositionForTesting(rightPosition, currentNode);

                GridPosition downPosition = currentNode.GridPosition.Add(0, -1);
                _TryToQueuePositionForTesting(downPosition, currentNode);

                GridPosition upPosition = currentNode.GridPosition.Add(0, 1);
                _TryToQueuePositionForTesting(upPosition, currentNode);
            }
            path = null;
            return false;


            void _TryToQueuePositionForTesting(GridPosition testPosition, PathingNode currentNode)
            {
                if (!ShouldTestPosition(database, combatState, testPosition, currentNode, startingPosition, alreadyTestedPositions, inProgressNodes, affiliation, skipEndPositionUnit: true, endPosition))
                    return;

                PathingNode newNode = new(testPosition, currentNode, startingPosition);
                inProgressNodes.Add(newNode);
            }
        }

        public static int GetStepCost(GameDatabase database, string combatId, GridPosition position)
        {
            // TODO: Make step cost based on more than just definition data (like some runtime if it changes what block is there or if some effect gets applied to it like tar)
            CombatDefinition combatDefinition = database.GetCombatDefinition(combatId);
            GridBlocksInfo gridBlocks = combatDefinition.GridBlocks;
            return gridBlocks.GetStepCostAtPosition(database, position);
        }

        private static bool IsPathingImpossible(GameDatabase database, CombatState combatState, GridPosition startingPosition, GridPosition endPosition, bool ignoreUnitBlocking = false)
        {
            if (startingPosition.Equals(endPosition))
                return true;

            string combatId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;

            if (!tileGrid.IsPositionOnGrid(startingPosition))
                return true;
            if (!tileGrid.IsPositionOnGrid(endPosition))
                return true;
            if (ignoreUnitBlocking)
                return false;

            IEnumerable<UnitState> unitsAtPosition = combatState.GetAllUnitsAtPosition(endPosition);
            return unitsAtPosition.Any(unit => database.GetUnitDefinition(unit.UniqueIdentifier.DefinitionId).ShouldBlockOccupiedTile);
        }

        private static bool ShouldTestPosition(GameDatabase database, CombatState combatState, GridPosition testPosition, IHaveGridPosition currentNode, GridPosition startPosition, IReadOnlyList<GridPosition> alreadyTestedPositions, IReadOnlyList<IHaveGridPosition> inProgressNodes, TeamAffiliation affiliation, bool skipEndPositionUnit = false, GridPosition endPosition = new())
        {
            string combatId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;
            GridBlocksInfo gridBlocks = combatDefinition.GridBlocks;

            // Not on the grid.
            if (!tileGrid.IsPositionOnGrid(testPosition))
                return false;
            // Too drastic a height change to walk this way.
            if (gridBlocks.IsHeightChangeDrastic(database, testPosition, currentNode.GridPosition))
                return false;
            // Already tested.
            if (alreadyTestedPositions.Contains(testPosition))
                return false;
            // Already queued up to test.
            if (inProgressNodes.Any(node => node.GridPosition.Equals(testPosition)))
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
            foreach (UnitState unit in unitsAtPosition)
            {
                // Ignore if they aren't even blocking the tile.
                CombatUnitUniqueIdentifier uniqueIdentifier = unit.UniqueIdentifier;
                string unitId = uniqueIdentifier.DefinitionId;
                UnitDefinition unitDefinition = database.GetUnitDefinition(unitId);
                if (!unitDefinition.ShouldBlockOccupiedTile)
                    continue;

                // If they are friendly, we can walk through them, but not stop on them.
                TeamAffiliation unitAffiliation = unit.CurrentAffiliation;
                TeamRelation relation = unitAffiliation.DetermineRelation(affiliation);
                if (relation != TeamRelation.Friendly)
                    return false;
            }
            return true;
        }

        private static ReachablePosition FindNodeWithLowestReachCost(IReadOnlyList<ReachablePosition> reachablePositions, out int index)
        {
            ReachablePosition lowestNode = reachablePositions[0];
            float lowestCost = lowestNode.ReachCost;
            index = 0;
            for (int i = 1; i < reachablePositions.Count; ++i)
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

        private static PathingNode FindNodeWithLowestF(IReadOnlyList<PathingNode> nodes, out int index)
        {
            PathingNode lowestNode = nodes[0];
            float lowestF = lowestNode.F;
            index = 0;
            for (int i = 1; i < nodes.Count; ++i)
            {
                PathingNode node = nodes[i];
                if (node.F >= lowestF)
                    continue;
                lowestNode = node;
                lowestF = node.F;
                index = i;
            }
            return lowestNode;
        }

        private static Path BuildPath(PathingNode endNode)
        {
            using ValueArray<GridPosition>.Builder pathPositions = new();
            PathingNode? currentNode = endNode;
            while (currentNode != null)
            {
                pathPositions.Add(currentNode.GridPosition);
                currentNode = currentNode.Parent;
            }
            return new Path(pathPositions.ToValueArray());
        }
    }
}
