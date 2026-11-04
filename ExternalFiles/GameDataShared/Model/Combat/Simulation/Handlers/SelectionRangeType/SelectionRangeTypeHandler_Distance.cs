// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionRangeType
{
    public sealed class SelectionRangeTypeHandler_Distance : ISelectionRangeTypeHandler
    {
        public GridPositionGroup GetTilesInRange(SelectionRangeTypeHandlerContext context)
        {
            GameDatabase database = context.Database;
            CombatState combatState = context.CombatState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            SelectionSpecificationsInfo selectionSpecifications = context.SelectionSpecifications;
            bool shouldCheckForValidOccupation = context.ShouldCheckForValidOccupation;
            Func<GridPosition, bool> isTileInRangeValidFunction = context.IsTileInRangeValidFunction;

            int tileRange = selectionSpecifications.Range;
            HitRulesInfo occupiedHitRules = selectionSpecifications.OccupiedHitRules;

            UnitState user = combatState.GetUnit(userIdentifier);
            GridPosition userPosition = user.GridPosition;
            Vector2Int userPositionVector = userPosition.Position;

            int tileRangeCeiling = (int)MathF.Ceiling(tileRange);
            Vector2Int tileRangeCeiling2D = new(tileRangeCeiling, tileRangeCeiling);
            Vector2Int max = userPositionVector + tileRangeCeiling2D;
            Vector2Int min = userPositionVector - tileRangeCeiling2D;
            using ValueArray<GridPosition>.Builder positionsInRangeBuilder = new();
            for (int y = min.Y; y <= max.Y; ++y)
            {
                for (int x = min.X; x <= max.X; ++x)
                {
                    Vector2Int current = new(x, y);
                    Vector2Int difference = current - userPositionVector;
                    int distanceAway = Math.Abs(difference.X) + Math.Abs(difference.Y);
                    if (distanceAway > tileRange)
                        continue;

                    GridPosition currentPosition = new(current);
                    if (!CombatSelectionRulesHelpers.IsPositionValid(database, combatState, userIdentifier, currentPosition, occupiedHitRules, shouldCheckForValidOccupation, isTileInRangeValidFunction))
                        continue;

                    positionsInRangeBuilder.Add(currentPosition);
                }
            }
            return new GridPositionGroup(positionsInRangeBuilder.ToValueArray());
        }
    }
}
