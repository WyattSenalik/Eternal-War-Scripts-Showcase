// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionType
{
    public sealed class SelectionTypeHandler_Any : ISelectionTypeHandler
    {
        public void GetTilesInRange(SelectionTypeHandlerContext context, out GridPositionGroup selectableTiles, out GridPositionGroup unselectableTiles)
        {
            selectableTiles = SelectTypeHandlerHelpers.GetTilesInRange(context, shouldCheckForValidOccupation: true, isTileInRangeValidFunction: gridPosition => true);
            unselectableTiles = SelectTypeHandlerHelpers.GetTilesInRange(context, shouldCheckForValidOccupation: false, isTileInRangeValidFunction: gridPosition => false);
        }
    }
}
