// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionRangeType;
using System;
using RangeType = EternalWar.Core.Model.Combat.Definitions.SelectionRangeType;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionType
{
    public static class SelectTypeHandlerHelpers
    {
        public static GridPositionGroup GetTilesInRange(SelectionTypeHandlerContext context, bool shouldCheckForValidOccupation, Func<GridPosition, bool> isTileInRangeValidFunction)
        {
            GameDatabase database = context.Database;
            CombatInstance combatInstance = context.CombatInstance;
            CombatState combatState = context.CombatState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            SelectionSpecificationsInfo selectionSpecifications = context.SelectionSpecifications;

            RangeType rangeType = selectionSpecifications.RangeType;
            ISelectionRangeTypeHandler rangeTypeHandler = CombatHandlersRegistration.GetHandler(rangeType);
            SelectionRangeTypeHandlerContext rangeTypeHandlerContext = new(database, combatInstance, combatState, userIdentifier, selectionSpecifications, shouldCheckForValidOccupation, isTileInRangeValidFunction);
            return rangeTypeHandler.GetTilesInRange(rangeTypeHandlerContext);
        }
    }
}
