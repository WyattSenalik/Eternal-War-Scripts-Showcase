// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionType
{
    public sealed class SelectionTypeHandler_HasUnit : ISelectionTypeHandler
    {
        public void GetTilesInRange(SelectionTypeHandlerContext context, out GridPositionGroup selectableTiles, out GridPositionGroup unselectableTiles)
        {
            GameDatabase database = context.Database;
            CombatState combatState = context.CombatState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            SelectionSpecificationsInfo selectionSpecifications = context.SelectionSpecifications;
            HitRulesInfo occupiedHitRules = selectionSpecifications.OccupiedHitRules;

            UnitState user = combatState.GetUnit(userIdentifier);

            selectableTiles = SelectTypeHandlerHelpers.GetTilesInRange(context, shouldCheckForValidOccupation: true, isTileInRangeValidFunction: gridPosition => CombatGridHelpers.IsTileOccupiedByValidOccupant(database, combatState, occupiedHitRules, user, gridPosition));

            unselectableTiles = SelectTypeHandlerHelpers.GetTilesInRange(context, shouldCheckForValidOccupation: false, isTileInRangeValidFunction: gridPosition => !CombatGridHelpers.IsTileOccupiedByValidOccupant(database, combatState, occupiedHitRules, user, gridPosition));
        }
    }
}
