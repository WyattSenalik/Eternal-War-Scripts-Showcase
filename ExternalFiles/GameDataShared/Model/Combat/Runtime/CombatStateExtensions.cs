// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public static class CombatStateExtensions
    {
        public static UnitState? GetUnitAtPosition(this CombatState combatState, GridPosition position)
        {
            // TODO: find a more efficient way to look this up
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;
            IEnumerable<UnitState> units = unitMap.Values;
            return units.FirstOrDefault(unit => unit.GridPosition == position);
        }

        public static IEnumerable<UnitState> GetAllUnitsAtPosition(this CombatState combatState, GridPosition position)
        {
            // TODO: find a more efficient way to look this up
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;
            IEnumerable<UnitState> units = unitMap.Values;
            return units.Where(unit => unit.GridPosition == position);
        }

        public static UnitState GetUnit(this CombatState combatState, CombatUnitUniqueIdentifier unitUniqueIdentifier)
        {
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;
            return unitMap[unitUniqueIdentifier];
        }

        public static bool TryGetUnit(this CombatState combatState, CombatUnitUniqueIdentifier unitUniqueIdentifier, [NotNullWhen(true)] out UnitState? unit)
        {
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;
            return unitMap.TryGetValue(unitUniqueIdentifier, out unit);
        }

        public static GridPosition GetUnitGridPosition(this CombatState combatState, CombatUnitUniqueIdentifier unitIdentifier)
        {
            UnitState unit = combatState.GetUnit(unitIdentifier);

            return unit.GridPosition;
        }

        public static float DistanceTo(this CombatState combatState, CombatUnitUniqueIdentifier fromIdentifier, CombatUnitUniqueIdentifier toIdentifier, DistanceType distanceType)
        {
            GridPosition fromPosition = combatState.GetUnitGridPosition(fromIdentifier);
            GridPosition toPosition = combatState.GetUnitGridPosition(toIdentifier);

            return fromPosition.DistanceTo(toPosition, distanceType);
        }

        public static TeamRelation DetermineRelationBetween(this CombatState combatState, CombatUnitUniqueIdentifier fromIdentifier, CombatUnitUniqueIdentifier toIdentifier)
        {
            UnitState fromUnit = combatState.GetUnit(fromIdentifier);
            TeamAffiliation fromAffiliation = fromUnit.CurrentAffiliation;
            UnitState toUnit = combatState.GetUnit(toIdentifier);
            TeamAffiliation toAffiliation = toUnit.CurrentAffiliation;

            return fromAffiliation.DetermineRelation(toAffiliation);
        }

        public static CombatStatus DeriveStatus(this CombatState combatState)
        {
            // TODO: Generic win-condition? Like route all enemies vs. escort
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;
            IEnumerable<UnitState> units = unitMap.Values;

            bool isAnyAllyAlive = false;
            bool isAnyEnemyAlive = false;
            foreach (UnitState unit in units)
            {
                if (unit.IsDead())
                    continue;

                TeamAffiliation affiliation = unit.CurrentAffiliation;
                switch (affiliation)
                {
                    case TeamAffiliation.Ally:
                        isAnyAllyAlive = true;
                        break;
                    case TeamAffiliation.Enemy:
                        isAnyEnemyAlive = true;
                        break;
                }

                if (isAnyAllyAlive && isAnyEnemyAlive)
                    break;
            }

            CombatStatus status = (isAnyAllyAlive, isAnyEnemyAlive) switch
            {
                (true, true) => new CombatStatus_Active(),
                (true, false) => new CombatStatus_Victory(),
                (false, true) => new CombatStatus_Defeat(),
                _ => throw new NotImplementedException($"({nameof(isAnyAllyAlive)}, {nameof(isAnyEnemyAlive)}) not implemented. ({isAnyAllyAlive}, {isAnyEnemyAlive})")
            };
            return status;
        }
    }
}
