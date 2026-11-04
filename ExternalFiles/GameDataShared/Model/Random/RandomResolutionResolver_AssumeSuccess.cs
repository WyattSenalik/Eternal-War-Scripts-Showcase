// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Model.Random
{
    public sealed class RandomResolutionResolver_AssumeSuccess : IRandomResolutionResolver
    {
        public HitRollResult RollHitChance(int chance)
        {
            return new HitRollResult(DidHit: true, DidCrit: false);
        }

        public int RollRange(int exclusiveMax)
        {
            return exclusiveMax - 1;
        }
    }
}
