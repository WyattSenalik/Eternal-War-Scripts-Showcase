// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using System.Collections.Generic;
using System.Numerics;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record CombatDefinition(string Id, TileGridInfo TileGrid, GridBlocksInfo GridBlocks, ValueArray<UnitPlacementOptionInfo> PlayerUnitPlacementOptions, ValueArray<UnitPlacementInfo> NonPlayerUnitPlacements, CombatRewardsInfo RewardsInfo) : IHaveStringId;

    public sealed record TileGridInfo(Vector2Int GridSize, Vector2 TileSize, Vector2 TileSpacing);

    public sealed record UnitPlacementInfo(string UnitId, UnitPlacementOptionInfo Placement);

    public sealed record UnitPlacementOptionInfo(GridPosition Position, EightDirection FacingDirection);

    // TODO: Maybe make artifact pool into a definition and reference a string id here instead so that we can reuse the pools (basically separating them into tiers). OR may want to have info for chance to get common, uncommen, rare, etc. and then mark the artifacts as such. (probably do the 2nd one).
    public sealed record CombatRewardsInfo(bool HasArtifactRewards, ValueArray<string> AvailableArtifactsIdPool);

    public static class TileGridInfoExtensions
    {
        public static Vector2 GetWorldSize(this TileGridInfo tileGrid)
        {
            Vector2Int gridSize = tileGrid.GridSize;
            Vector2 tileSize = tileGrid.TileSize;
            Vector2 tileSpacing = tileGrid.TileSpacing;

            float x = gridSize.X * (tileSize.X + tileSpacing.X);
            float y = gridSize.Y * (tileSize.Y + tileSpacing.Y);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns true if position is on the grid and not null
        /// </summary>
        public static bool IsPositionOnGrid(this TileGridInfo tileGrid, GridPosition gridPosition)
        {
            Vector2Int gridSize = tileGrid.GridSize;
            Vector2Int gridPositionVector = gridPosition.Position;

            return gridPositionVector.X >= 0 && gridPositionVector.X < gridSize.X
                && gridPositionVector.Y >= 0 && gridPositionVector.Y < gridSize.Y;
        }

        public static List<GridPosition> GetAllGridPositions(this TileGridInfo tileGrid)
        {
            Vector2Int gridSize = tileGrid.GridSize;

            List<GridPosition> gridPositions = new();
            for (int y = 0; y < gridSize.Y; ++y)
            {
                for (int x = 0; x < gridSize.X; ++x)
                {
                    GridPosition currentGridPosition = new(x, y);
                    gridPositions.Add(currentGridPosition);
                }
            }
            return gridPositions;
        }
    }
}
