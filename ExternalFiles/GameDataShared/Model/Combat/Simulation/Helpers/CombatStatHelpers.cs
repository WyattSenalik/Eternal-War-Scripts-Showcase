// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatStatHelpers
    {
        public static CombatStats CalculateInitialStats(GameDatabase database, CombatInstance instance, CombatUnitUniqueIdentifier unitIdentifier)
        {
            string unitDefinitionId = unitIdentifier.DefinitionId;

            UnitBuild unitBuild = instance.GetUnitBuild(unitIdentifier);
            UnitDefinition unitDefinition = database.GetUnitDefinition(unitDefinitionId);

            CombatStats baseStats = unitDefinition.GetBaseStats();
            CombatStats startingStatChanges = unitBuild.StatChanges;

            return baseStats.Merge(startingStatChanges);
        }

        public static CombatStats CalculateStats(GameDatabase database, CombatRuntime combatRuntime, CombatUnitUniqueIdentifier unitIdentifier) => CalculateStats(database, combatRuntime.Instance, combatRuntime.State, unitIdentifier);
        public static CombatStats CalculateStats(GameDatabase database, CombatInstance instance, CombatState state, CombatUnitUniqueIdentifier unitIdentifier)
        {
            CombatStats currentStats = CalculateInitialStats(database, instance, unitIdentifier);

            UnitState unit = state.GetUnit(unitIdentifier);
            IReadOnlyList<TemporaryStatModifierState> temporaryStatModifiers = unit.TemporaryStatModifiers;
            foreach (TemporaryStatModifierState statModifier in temporaryStatModifiers)
            {
                currentStats = AddTemporaryModifier(currentStats, statModifier);
            }
            return currentStats;
        }

        public static CombatStats AddTemporaryModifier(CombatStats combatStats, TemporaryStatModifierState temporaryStateModifier)
        {
            StatType statType = temporaryStateModifier.Stat;
            int previousAmount = combatStats.Get(statType);
            int modifyAmount = temporaryStateModifier.Amount;
            int newAmount = previousAmount + modifyAmount;
            return combatStats.Replace(statType, newAmount);
        }
    }
}
