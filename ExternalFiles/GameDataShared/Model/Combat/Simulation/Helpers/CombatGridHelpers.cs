// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    /// <summary>Helpers for determining what is on the board.</summary>
    public static class CombatGridHelpers
    {
        public static bool IsTileBlockedByValidOccupant(GameDatabase database, CombatState combatState, HitRulesInfo occupiedHitRules, UnitState user, GridPosition position)
        {
            string combatDefinitionId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;

            if (!tileGrid.IsPositionOnGrid(position))
                return false;

            IEnumerable<UnitState> unitsAtPosition = combatState.GetAllUnitsAtPosition(position);
            foreach (UnitState unit in unitsAtPosition)
            {
                CombatUnitUniqueIdentifier unitIdentifier = unit.UniqueIdentifier;
                string unitDefinitionId = unitIdentifier.DefinitionId;

                UnitDefinition unitDefinition = database.GetUnitDefinition(unitDefinitionId);
                bool shouldBlockOccupiedTile = unitDefinition.ShouldBlockOccupiedTile;
                if (!shouldBlockOccupiedTile)
                    continue;

                if (!CombatTargetingRulesHelpers.AreHitRulesSatisfied(occupiedHitRules, user, unit, database))
                    continue;

                return true;
            }
            return false;
        }

        public static bool IsTileBlocked(GameDatabase database, CombatState combatState, GridPosition position)
        {
            string combatDefinitionId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;

            if (!tileGrid.IsPositionOnGrid(position))
                return false;

            IEnumerable<UnitState> unitsAtPosition = combatState.GetAllUnitsAtPosition(position);
            foreach (UnitState unit in unitsAtPosition)
            {
                CombatUnitUniqueIdentifier unitIdentifier = unit.UniqueIdentifier;
                string unitDefinitionId = unitIdentifier.DefinitionId;

                UnitDefinition unitDefinition = database.GetUnitDefinition(unitDefinitionId);
                bool shouldBlockOccupiedTile = unitDefinition.ShouldBlockOccupiedTile;
                if (shouldBlockOccupiedTile)
                    return true;
            }
            return false;
        }

        public static bool IsTileOccupiedByValidOccupant(GameDatabase database, CombatState combatState, HitRulesInfo occupiedHitRules, UnitState user, GridPosition position)
        {
            string combatDefinitionId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;

            if (!tileGrid.IsPositionOnGrid(position))
                return false;

            IEnumerable<UnitState> unitsAtPosition = combatState.GetAllUnitsAtPosition(position);
            foreach (UnitState unit in unitsAtPosition)
            {
                if (CombatTargetingRulesHelpers.AreHitRulesSatisfied(occupiedHitRules, user, unit, database))
                    return true;
            }
            return false;
        }

        public static bool IsTileOccupied(GameDatabase database, CombatState combatState, GridPosition position)
        {
            string combatDefinitionId = combatState.DefinitionId;

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatDefinitionId);
            TileGridInfo tileGrid = combatDefinition.TileGrid;

            if (!tileGrid.IsPositionOnGrid(position))
                return false;

            IEnumerable<UnitState> unitsAtPosition = combatState.GetAllUnitsAtPosition(position);
            return unitsAtPosition.Any();
        }
    }
}
