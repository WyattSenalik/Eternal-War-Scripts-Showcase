// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation
{
    public sealed class TileLocationHandler_Self : ITileLocationHandler
    {
        public IReadOnlyList<GridPosition> DetermineGridPositions(TileLocationHandlerContext context)
        {
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            CombatState combatState = context.CombatState;

            UnitState user = combatState.GetUnit(userIdentifier);
            GridPosition userPosition = user.GridPosition;

            return new GridPosition[1] { userPosition };
        }
    }
}
