// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Model.Combat.Common.Stats
{
    public enum StatType
    {
        Health,
        Strength,
        Defense,
        Speed,
        Magic,
        Will,
        Movement,
        ActionPoints,
        Evasion,
        Accuracy
    }

    public enum AttributeStatType
    {
        Strength = 0,
        Defense = 1,
        Speed = 2,
        Magic = 3,
        Will = 4,
        Evasion = 5,
        Accuracy = 6
    }

    public enum VictoryStatType
    {
        Health,
        Strength,
        Defense,
        Speed,
        Magic,
        Will,
    }

    public static class StatTypeExtensions
    {
        public static float GetStatValue(this StatType statType, CombatStats stats)
        {
            AttributeStats attributeStats = stats.AttributeStats;
            HealthInfo healthInfo = stats.MaxHealthInfo;
            int movement = stats.Movement;
            int actionPoints = stats.MaxActionPoints;

            return statType switch
            {
                StatType.Health => healthInfo.Health,

                StatType.Movement => movement,
                StatType.ActionPoints => actionPoints,

                StatType.Strength => attributeStats.Strength,
                StatType.Defense => attributeStats.Defense,
                StatType.Speed => attributeStats.Speed,
                StatType.Magic => attributeStats.Magic,
                StatType.Will => attributeStats.Will,
                StatType.Evasion => attributeStats.Evasion,
                StatType.Accuracy => attributeStats.Accuracy,
                _ => throw new NotImplementedException()
            };
        }
    }

    public static class AttributeStatTypeExtensions
    {
        public static bool IsAttributeStatType(this StatType statType)
        {
            return statType switch
            {
                StatType.Strength or StatType.Defense or StatType.Speed or StatType.Magic or StatType.Will or StatType.Evasion or StatType.Accuracy => true,
                _ => false,
            };
        }

        public static AttributeStatType ToAttributeStatType(this StatType statType)
        {
            return statType switch
            {
                StatType.Strength => AttributeStatType.Strength,
                StatType.Defense => AttributeStatType.Defense,
                StatType.Speed => AttributeStatType.Speed,
                StatType.Magic => AttributeStatType.Magic,
                StatType.Will => AttributeStatType.Will,
                StatType.Evasion => AttributeStatType.Evasion,
                StatType.Accuracy => AttributeStatType.Accuracy,
                _ => throw new Exception($"Invalid {nameof(StatType)} to convert to {nameof(AttributeStatType)}: ({statType})")
            };
        }
    }

    public static class VictoryStatTypeExtensions
    {
        public static StatType ToStatType(this VictoryStatType victoryStatType)
        {
            return victoryStatType switch
            {
                VictoryStatType.Health => StatType.Health,
                VictoryStatType.Strength => StatType.Strength,
                VictoryStatType.Defense => StatType.Defense,
                VictoryStatType.Speed => StatType.Speed,
                VictoryStatType.Magic => StatType.Magic,
                VictoryStatType.Will => StatType.Will,
                _ => throw new NotImplementedException($"Unhandled {nameof(VictoryStatType)}: ({victoryStatType})"),
            };
        }
    }
}
