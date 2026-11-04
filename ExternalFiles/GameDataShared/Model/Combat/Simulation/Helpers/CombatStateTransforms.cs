// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatStateTransforms
    {
        public static CombatState TransformUnit(this CombatState combatState, CombatUnitUniqueIdentifier unitUniqueIdentifier, Func<UnitState, UnitState> modifyFunc)
        {
            UnitState oldUnitState = combatState.GetUnit(unitUniqueIdentifier);
            UnitState newUnitState = modifyFunc.Invoke(oldUnitState);
            return ReplaceUnit(combatState, newUnitState);
        }

        public static CombatState ReplaceUnit(this CombatState combatState, UnitState modifiedUnit)
        {
            CombatUnitUniqueIdentifier unitUniqueIdentifier = modifiedUnit.UniqueIdentifier;
            IReadOnlyDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;

            using ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder mapBuilder = ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.CreateBuilder(unitMap);
            mapBuilder[unitUniqueIdentifier] = modifiedUnit;
            CombatState modifiedCombatState = combatState with
            {
                UnitMap = mapBuilder.ToValueDictionary()
            };
            return modifiedCombatState;
        }
    }
}
