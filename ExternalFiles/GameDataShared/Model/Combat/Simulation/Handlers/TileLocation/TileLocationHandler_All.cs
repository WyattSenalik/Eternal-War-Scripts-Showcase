// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation
{
    public sealed class TileLocationHandler_All : ITileLocationHandler
    {
        public IReadOnlyList<GridPosition> DetermineGridPositions(TileLocationHandlerContext context)
        {
            GameDatabase database = context.Database;
            CombatState combatState = context.CombatState;
            string combatDefinitionId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;
            Vector2Int gridSize = tileGrid.GridSize;

            List<GridPosition> gridPositions = new(gridSize.X * gridSize.Y);
            for (int y = 0; y < gridSize.Y; ++y)
            {
                for (int x = 0; x < gridSize.X; ++x)
                {
                    GridPosition position = new(x, y);
                    gridPositions.Add(position);
                }
            }
            return gridPositions;
        }
    }
}
