// Original Authors - Wyatt Senalik
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public enum ExpirationTime
    {
        Never = 0,
        TurnEnd = 1,
        TurnStart = 2,
        AfterActivate_PassiveSpecific = 3,
        SingleTileMoved = 4,
        OnHit = 5,
        OnPushed = 6,
    }

    public static class ExpirationTimeExtensions
    {
        public static bool TryConvertToPassiveActivateTime(this ExpirationTime expirationTime, out PassiveActivateTime activateTime)
        {
            switch (expirationTime)
            {
                case ExpirationTime.TurnStart:
                    activateTime = PassiveActivateTime.TurnStart;
                    return true;

                case ExpirationTime.TurnEnd:
                    activateTime = PassiveActivateTime.TurnEnd;
                    return true;

                case ExpirationTime.OnHit:
                    activateTime = PassiveActivateTime.OnHit;
                    return true;

                default:
                    activateTime = 0;
                    return false;
            }
        }

        public static bool TryCreateAdditionalStringKeyArguments(this ExpirationTime expirationTime, object args, [NotNullWhen(true)] out (string, object)[]? additionalArgs)
        {
            additionalArgs = expirationTime switch
            {
                ExpirationTime.OnHit => new (string, object)[1] { (CombatStringArgumentNames.DAMAGE_CONTEXT, args) },
                _ => null
            };
            return additionalArgs != null;
        }
    }
}
