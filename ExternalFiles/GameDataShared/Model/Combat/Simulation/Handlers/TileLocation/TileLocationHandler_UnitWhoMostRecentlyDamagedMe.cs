// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation
{
    public sealed class TileLocationHandler_UnitWhoMostRecentlyDamagedMe : ITileLocationHandler
    {
        public IReadOnlyList<GridPosition> DetermineGridPositions(TileLocationHandlerContext context)
        {
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            CombatState combatState = context.CombatState;

            UnitState user = combatState.GetUnit(userIdentifier);
            CombatUnitUniqueIdentifier? unitIdentifierWhoDamagedUserMostRecently = user.UnitIdentifierWhoMostRecentlyDamagedMe;
            if (unitIdentifierWhoDamagedUserMostRecently == null)
            {
                GridPosition userPosition = user.GridPosition;
                return new GridPosition[1] { userPosition };
            }

            UnitState unitWhoDamagedUserMostRecently = combatState.GetUnit(unitIdentifierWhoDamagedUserMostRecently);
            GridPosition targetPosition = unitWhoDamagedUserMostRecently.GridPosition;
            return new GridPosition[1] { targetPosition };
        }
    }
}
