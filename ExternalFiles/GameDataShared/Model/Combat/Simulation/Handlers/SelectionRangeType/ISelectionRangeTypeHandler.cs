// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System;
using RangeType = EternalWar.Core.Model.Combat.Definitions.SelectionRangeType;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionRangeType
{
    public sealed record SelectionRangeTypeHandlerContext(GameDatabase Database, CombatInstance CombatInstance, CombatState CombatState, CombatUnitUniqueIdentifier UserIdentifier, SelectionSpecificationsInfo SelectionSpecifications, bool ShouldCheckForValidOccupation, Func<GridPosition, bool> IsTileInRangeValidFunction);

    public interface ISelectionRangeTypeHandler
    {
        GridPositionGroup GetTilesInRange(SelectionRangeTypeHandlerContext context);
    }

    public static class SelectionRangeTypeHandlerFactory
    {
        public static ISelectionRangeTypeHandler CreateHandler(RangeType rangeType)
        {
            return rangeType switch
            {
                RangeType.Distance => new SelectionRangeTypeHandler_Distance(),
                RangeType.Steps => new SelectionRangeTypeHandler_Steps(),
                RangeType.Movable => new SelectionRangeTypeHandler_Movable(),
                _ => throw new NotImplementedException($"Unhandled {nameof(RangeType)}: {rangeType}")
            };
        }
    }
}
