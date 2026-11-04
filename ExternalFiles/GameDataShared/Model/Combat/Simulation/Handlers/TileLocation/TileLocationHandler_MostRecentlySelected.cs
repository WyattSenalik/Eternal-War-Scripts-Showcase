// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation
{
    public sealed class TileLocationHandler_MostRecentlySelected : ITileLocationHandler
    {
        public IReadOnlyList<GridPosition> DetermineGridPositions(TileLocationHandlerContext context)
        {
            GridPositionGroup selectionGroup = context.SelectionGroup;
            return selectionGroup.Positions;
        }
    }
}
