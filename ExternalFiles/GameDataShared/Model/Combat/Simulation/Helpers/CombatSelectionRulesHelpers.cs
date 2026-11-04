// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    /// <summary>Helpers for determining if tiles are selectable.</summary>
    public static class CombatSelectionRulesHelpers
    {
        public static bool IsPositionValid(GameDatabase database, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, GridPosition position, HitRulesInfo hitRules, bool shouldCheckForValidOccupation, Func<GridPosition, bool> isTileInRangeValid)
        {
            string combatId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;

            if (!tileGrid.IsPositionOnGrid(position))
                return false;
            if (shouldCheckForValidOccupation && !IsOccupationValid(database, combatState, userIdentifier, position, hitRules))
                return false;
            if (!isTileInRangeValid.Invoke(position))
                return false;
            return true;
        }

        public static bool IsOccupationValid(GameDatabase database, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, GridPosition position, HitRulesInfo hitRules)
        {
            UnitState user = combatState.GetUnit(userIdentifier);

            IEnumerable<UnitState> unitsAtPosition = combatState.GetAllUnitsAtPosition(position);
            foreach (UnitState unit in unitsAtPosition)
            {
                if (CombatTargetingRulesHelpers.AreHitRulesSatisfied(hitRules, user, unit, database))
                    continue;

                CombatUnitUniqueIdentifier unitIdentifier = unit.UniqueIdentifier;
                string unitDefinitionId = unitIdentifier.DefinitionId;
                UnitDefinition unitDefinition = database.GetUnitDefinition(unitDefinitionId);
                if (!unitDefinition.ShouldBlockOccupiedTile)
                    continue;

                return false;
            }
            return true;
        }
    }
}
