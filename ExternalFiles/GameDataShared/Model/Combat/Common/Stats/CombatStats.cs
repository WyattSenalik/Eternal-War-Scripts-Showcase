// Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Model.Combat.Common.Stats
{
    public sealed record CombatStats(HealthInfo MaxHealthInfo, AttributeStats AttributeStats, int Movement, int MaxActionPoints)
    {
        public static readonly CombatStats Empty = new(HealthInfo.Empty, AttributeStats.Empty, Movement: 0, MaxActionPoints: 0);
    }

    public static class CombatStatsExtensions
    {
        public static CombatStats Merge(this CombatStats a, CombatStats b)
        {
            HealthInfo healthInfo = a.MaxHealthInfo.Merge(b.MaxHealthInfo);
            AttributeStats attributeStats = a.AttributeStats.Merge(b.AttributeStats);
            int movement = a.Movement + b.Movement;
            int actionPoints = a.MaxActionPoints + b.MaxActionPoints;

            return new CombatStats(healthInfo, attributeStats, movement, actionPoints);
        }

        public static int Get(this CombatStats stats, StatType type)
        {
            return type switch
            {
                StatType.Health => stats.MaxHealthInfo.Health,
                StatType.Strength => stats.AttributeStats.Strength,
                StatType.Defense => stats.AttributeStats.Defense,
                StatType.Speed => stats.AttributeStats.Speed,
                StatType.Magic => stats.AttributeStats.Magic,
                StatType.Will => stats.AttributeStats.Will,
                StatType.Movement => stats.Movement,
                StatType.ActionPoints => stats.MaxActionPoints,
                StatType.Evasion => stats.AttributeStats.Evasion,
                StatType.Accuracy => stats.AttributeStats.Accuracy,
                _ => throw new NotImplementedException($"Unhandled {nameof(StatType)}: ({type})")
            };
        }

        public static CombatStats Replace(this CombatStats stats, StatType type, int newValue)
        {
            return type switch
            {
                StatType.Health => Replace_Health(stats, newValue),

                StatType.Movement => new CombatStats(stats.MaxHealthInfo, stats.AttributeStats, newValue, stats.MaxActionPoints),
                StatType.ActionPoints => new CombatStats(stats.MaxHealthInfo, stats.AttributeStats, stats.Movement, newValue),

                StatType.Strength or
                StatType.Defense or 
                StatType.Speed or 
                StatType.Magic or 
                StatType.Will or 
                StatType.Evasion or 
                StatType.Accuracy => Replace_Attributes(stats, type, newValue),

                _ => throw new NotImplementedException($"Unhandled {nameof(StatType)}: ({type})")
            };
        }

        private static CombatStats Replace_Health(CombatStats stats, int newHealth)
        {
            HealthInfo originalHealthInfo = stats.MaxHealthInfo;
            HealthInfo newHealthInfo = new(originalHealthInfo.HasInfiniteHealth, newHealth);
            return new CombatStats(newHealthInfo, stats.AttributeStats, stats.Movement, stats.MaxActionPoints);
        }

        private static CombatStats Replace_Attributes(CombatStats stats, StatType type, int newAttributeValue)
        {
            AttributeStats originalAttributeStats = stats.AttributeStats;

            AttributeStatType attributeStatType = type.ToAttributeStatType();
            AttributeStats newAttributeStats = originalAttributeStats.Replace(attributeStatType, newAttributeValue);

            return new CombatStats(stats.MaxHealthInfo, newAttributeStats, stats.Movement, stats.MaxActionPoints);
        }
    }
}