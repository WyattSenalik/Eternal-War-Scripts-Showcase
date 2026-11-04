// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Model.Combat.Common
{
    public static class CombatConstants
    {
        // For each 10 of a defensive stat, each point of health you have can take an extra damage of the type being reduced (stole this from league of legends).
        // Examples: - 0 DEF and gets dealt 12 raw physical damage. Actually take 12 damage.
        //           - 10 DEF and gets dealt 12 raw physical damage. Actually take 6 damage.
        //           - 20 DEF and gets dealt 12 raw physical damage. Actually take 4 damage.
        //           - 20 DEF and gets dealt 12 raw physical damage. Actually take 3 damage.
        public const float DEFENSIVE_REDUCTION_FACTOR = 1.0f / 10.0f;
        public const float PUSH_DIAGONAL_CUTOFF = 0.35f;
        public const float BASE_CRIT_MOD = 1.5f;
        public const float BLOCK_HEIGHT = 0.5f;
    }
}
