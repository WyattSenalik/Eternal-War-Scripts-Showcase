// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Model.Combat.Common.Stats
{
    public sealed record AttributeStats(int Strength, int Defense, int Speed, int Magic, int Will, int Evasion, int Accuracy)
    {
        public static readonly AttributeStats Empty = new(0, 0, 0, 0, 0, 0, 0);
    }

    public static class AttributeStatsExtensions
    {
        public static int Get(this AttributeStats attributeStats, AttributeStatType statType)
        {
            return statType switch
            {
                AttributeStatType.Strength => attributeStats.Strength,
                AttributeStatType.Defense => attributeStats.Defense,
                AttributeStatType.Speed => attributeStats.Speed,
                AttributeStatType.Magic => attributeStats.Magic,
                AttributeStatType.Will => attributeStats.Will,
                AttributeStatType.Evasion => attributeStats.Evasion,
                AttributeStatType.Accuracy => attributeStats.Accuracy,
                _ => throw new NotImplementedException($"Unhandled {nameof(AttributeStatType)}: ({statType})")
            };
        }

        public static AttributeStats Replace(this AttributeStats attributeStats, AttributeStatType statType, int newValue)
        {
            return statType switch
            {
                AttributeStatType.Strength => attributeStats with { Strength = newValue },
                AttributeStatType.Defense => attributeStats with { Defense = newValue },
                AttributeStatType.Speed => attributeStats with { Speed = newValue },
                AttributeStatType.Magic => attributeStats with { Magic = newValue },
                AttributeStatType.Will => attributeStats with { Will = newValue },
                AttributeStatType.Evasion => attributeStats with { Evasion = newValue },
                AttributeStatType.Accuracy => attributeStats with { Accuracy = newValue },
                _ => throw new NotImplementedException($"Unhandled {nameof(AttributeStatType)}: ({statType})")
            };
        }

        public static AttributeStats Merge(this AttributeStats a, AttributeStats b)
        {
            int strength = a.Strength + b.Strength;
            int defense = a.Defense + b.Defense;
            int speed = a.Speed + b.Speed;
            int magic = a.Magic + b.Magic;
            int will = a.Will + b.Will;
            int evasion = a.Evasion + b.Evasion;
            int accuracy = a.Accuracy + b.Accuracy;
            return new AttributeStats(strength, defense, speed, magic, will, evasion, accuracy);
        }
    }
}
