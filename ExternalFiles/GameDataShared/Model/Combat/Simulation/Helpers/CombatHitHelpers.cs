// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatHitHelpers
    {
        public static int CalculateHitChance(GameDatabase database, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, CombatUnitUniqueIdentifier targetIdentifier, int moveAccuracy)
        {
            CombatStats userStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, userIdentifier);
            CombatStats targetStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, targetIdentifier);
            float accuracy = userStats.AttributeStats.Accuracy;
            float evasion = targetStats.AttributeStats.Evasion;
            return CalculateHitChance(moveAccuracy, accuracy, evasion);
        }

        public static int CalculateHitChance(int moveAccuracy, float accuracy, float evasion)
        {
            float unitEvasionMod = 1 - (evasion / 100.0f);
            return (int)Math.Floor((moveAccuracy + accuracy) * unitEvasionMod);
        }

        public static int CalculateDamageToDeal(GameDatabase database, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, CombatUnitUniqueIdentifier targetIdentifier, DamageInfo damageInfo, bool didCrit, int? passiveStacks = null)
        {
            CombatStats userStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, userIdentifier);
            CombatStats targetStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, targetIdentifier);
            AttributeStats targetAttributeStats = targetStats.AttributeStats;
            HitInfo hitInfo = damageInfo.HitInfo;
            DamageType damageType = damageInfo.DamageType;
            bool hasMinimumValue = hitInfo.HasMinimumValue;
            int minimumValue = hitInfo.MinimumValue;

            ValueFunctionContext functionContext = new(userStats, passiveStacks);
            float rawHitValue = hitInfo.GetValue(functionContext);
            int defensiveStatValue = damageType.GetDefensiveStat(targetAttributeStats);
            int damage = (int)MathF.Floor(rawHitValue / (1 + (defensiveStatValue * CombatConstants.DEFENSIVE_REDUCTION_FACTOR)));
            if (hasMinimumValue)
                damage = Math.Max(damage, minimumValue);
            return !didCrit ? damage : (int)MathF.Floor(damage * CombatConstants.BASE_CRIT_MOD);
        }

    }
}
