// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation
{
    public sealed class TileLocationHandler_Square : ITileLocationHandler
    {
        public IReadOnlyList<GridPosition> DetermineGridPositions(TileLocationHandlerContext context)
        {
            GameDatabase database = context.Database;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            CombatState combatState = context.CombatState;
            string combatDefinitionId = combatState.DefinitionId;
            int locationValue = context.LocationValue;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;

            UnitState user = combatState.GetUnit(userIdentifier);
            GridPosition userPosition = user.GridPosition;

            if (locationValue <= 0)
                return new GridPosition[1] { userPosition };
            int squareHalf = locationValue / 2;
            GridPosition squareBottomLeft = userPosition - new Vector2Int(squareHalf, squareHalf);

            List<GridPosition> gridPositions = new();
            for (int x = 0; x < locationValue; x++)
            {
                for (int y = 0; y < locationValue; y++)
                {
                    GridPosition currentPosition = squareBottomLeft + new Vector2Int(x, y);
                    if (!tileGrid.IsPositionOnGrid(currentPosition))
                        continue;
                    gridPositions.Add(currentPosition);
                }
            }

            return gridPositions;
        }
    }
}
