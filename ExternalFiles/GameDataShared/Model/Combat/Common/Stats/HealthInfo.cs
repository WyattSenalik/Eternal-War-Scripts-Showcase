// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Model.Combat.Common.Stats
{
    public sealed record HealthInfo(bool HasInfiniteHealth, int Health)
    {
        public static readonly HealthInfo Empty = new(HasInfiniteHealth: false, Health: 0);
    }

    public static class HealthInfoExtensions
    {
        public static HealthInfo Merge(this HealthInfo a, HealthInfo b)
        {
            bool hasInfiniteHealth = a.HasInfiniteHealth || b.HasInfiniteHealth;
            int health = a.Health + b.Health;
            return new HealthInfo(hasInfiniteHealth, health);
        }
    }
}
