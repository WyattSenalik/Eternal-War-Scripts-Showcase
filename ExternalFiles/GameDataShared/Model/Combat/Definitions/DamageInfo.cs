// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common.Stats;
using System;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record DamageInfo(DamageType DamageType, HitInfo HitInfo)
    {
        public static readonly DamageInfo Empty = new(0, HitInfo.Empty);
    }

    public enum DamageType
    {
        Physical = 0,
        Magical = 1,
        True = 2,
    }

    public static class DamageTypeExtensions
    {
        public static int GetDefensiveStat(this DamageType damageType, AttributeStats attributeStats)
        {
            int defense = attributeStats.Defense;
            int will = attributeStats.Will;
            return damageType switch
            {
                DamageType.Physical => defense,
                DamageType.Magical => will,
                DamageType.True => 0,
                _ => throw new NotImplementedException($"Unhandled {nameof(DamageType)}: ({damageType})")
            };
        }
    }
}
