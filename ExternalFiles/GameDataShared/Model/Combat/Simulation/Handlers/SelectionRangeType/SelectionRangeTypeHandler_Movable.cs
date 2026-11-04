// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionRangeType
{
    public sealed class SelectionRangeTypeHandler_Movable : ISelectionRangeTypeHandler
    {
        public GridPositionGroup GetTilesInRange(SelectionRangeTypeHandlerContext context)
        {
            GameDatabase database = context.Database;
            CombatState combatState = context.CombatState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            SelectionSpecificationsInfo selectionSpecifications = context.SelectionSpecifications;
            bool shouldCheckForValidOccupation = context.ShouldCheckForValidOccupation;
            Func<GridPosition, bool> isTileInRangeValidFunction = context.IsTileInRangeValidFunction;

            selectionSpecifications.Deconstruct(out int tileRange, out _, out _, out _, out _, out HitRulesInfo occupiedHitRules);

            UnitState user = combatState.GetUnit(userIdentifier);
            GridPosition userPosition = user.GridPosition;
            TeamAffiliation userAffiliation = user.CurrentAffiliation;

            List<GridPosition> reachablePositions = CombatPathingHelpers.DetermineReachablePositions(database, combatState, userPosition, tileRange, userAffiliation);
            int index = 0;
            while (index < reachablePositions.Count)
            {
                if (!isTileInRangeValidFunction.Invoke(reachablePositions[index]))
                {
                    reachablePositions.RemoveAt(index);
                    continue;
                }
                ++index;
            }
            return new GridPositionGroup(reachablePositions.ToValueArray());
        }
    }
}
