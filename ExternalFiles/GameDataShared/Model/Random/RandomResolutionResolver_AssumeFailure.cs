// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Model.Random
{
    public sealed class RandomResolutionResolver_AssumeFailure : IRandomResolutionResolver
    {
        public HitRollResult RollHitChance(int chance)
        {
            return new HitRollResult(DidHit: false, DidCrit: false);
        }

        public int RollRange(int exclusiveMax)
        {
            return 0;
        }
    }
}
