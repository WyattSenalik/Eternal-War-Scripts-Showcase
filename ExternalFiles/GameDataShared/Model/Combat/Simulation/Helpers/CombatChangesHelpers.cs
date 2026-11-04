// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatChangesHelpers
    {
        public static ValueArray<(UnitState? PreviousUnit, UnitState? NewUnit)> FindUnitDifferences(CombatState previousState, CombatState newState)
        {
            using ValueArray<(UnitState?, UnitState?)>.Builder unitChanges = new();

            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> previousUnitMap = previousState.UnitMap;
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> newUnitMap = newState.UnitMap;

            // Go through both previous and new in case a unit was added or removed from combat
            // Previous keys
            foreach (CombatUnitUniqueIdentifier unitIdentifier in previousUnitMap.Keys)
            {
                UnitState previousUnit = previousUnitMap[unitIdentifier];
                if (!newUnitMap.TryGetValue(unitIdentifier, out UnitState? newUnit) || previousUnit != newUnit)
                    unitChanges.Add((previousUnit, newUnit));
            }
            // New keys
            foreach (CombatUnitUniqueIdentifier unitIdentifier in newUnitMap.Keys)
            {
                if (previousUnitMap.TryGetValue(unitIdentifier, out UnitState? previousUnit))
                    continue;
                // Have to be different, this is a new unit that didn't exist previously
                UnitState newUnit = newUnitMap[unitIdentifier];
                unitChanges.Add((previousUnit, newUnit));
            }

            return unitChanges.ToValueArray();
        }

        public static bool DidUnitDie(UnitState? previousState, UnitState? newState)
        {
            if (previousState == null && newState == null)
                throw new Exception("Tried checking if unit died by comparing 2 null unit states");

            // Previous State was null means was just spawned
            if (previousState == null)
                return false;
            // New state is null means unit died
            if (newState == null)
                return true;

            // Was already dead
            int previousHealth = previousState.CurrentHealth;
            if (previousHealth <= 0)
                return false;

            int newHealth = newState.CurrentHealth;
            return newHealth <= 0;
        }
    }
}
