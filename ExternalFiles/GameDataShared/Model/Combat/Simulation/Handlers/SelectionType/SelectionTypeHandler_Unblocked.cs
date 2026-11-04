// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionType
{
    public sealed class SelectionTypeHandler_Unblocked : ISelectionTypeHandler
    {
        public void GetTilesInRange(SelectionTypeHandlerContext context, out GridPositionGroup selectableTiles, out GridPositionGroup unselectableTiles)
        {
            GameDatabase database = context.Database;
            CombatState combatState = context.CombatState;

            selectableTiles = SelectTypeHandlerHelpers.GetTilesInRange(context, shouldCheckForValidOccupation: true, isTileInRangeValidFunction: gridPosition => !CombatGridHelpers.IsTileBlocked(database, combatState, gridPosition));

            unselectableTiles = SelectTypeHandlerHelpers.GetTilesInRange(context, shouldCheckForValidOccupation: false, isTileInRangeValidFunction: gridPosition => CombatGridHelpers.IsTileBlocked(database, combatState, gridPosition));
        }
    }
}
