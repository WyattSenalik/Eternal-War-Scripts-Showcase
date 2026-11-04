// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Core.Model.Random
{
    public sealed class RandomResolutionResolver_Deterministic(ulong seed, ulong position = 0) : IRandomResolutionResolver, IEquatable<RandomResolutionResolver_Deterministic>
    {
        /// <summary>
        /// Number of raw 64-bit random values already consumed.
        /// The next raw value will be at this position.
        /// </summary>
        public ulong Position { get; private set; } = position;
        public ulong Seed { get; } = seed;

        public HitRollResult RollHitChance(int chance)
        {
            int randomPercent = NextInt(1, 101);
            if (chance <= 0)
                return new HitRollResult(DidHit: false, DidCrit: false);

            bool canCrit = chance > 100;
            if (!canCrit)
            {
                bool didHit = randomPercent <= chance;
                return new HitRollResult(didHit, DidCrit: false);
            }
            int critChance = chance - 100;
            bool didCrit = randomPercent <= critChance;
            return new HitRollResult(DidHit: true, didCrit);
        }

        public int RollRange(int exclusiveMax)
        {
            return NextInt(0, exclusiveMax);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not RandomResolutionResolver_Deterministic other)
                return false;
            return Equals(other);
        }
        public static bool operator ==(RandomResolutionResolver_Deterministic a, RandomResolutionResolver_Deterministic b) => a.Equals(b);
        public static bool operator !=(RandomResolutionResolver_Deterministic a, RandomResolutionResolver_Deterministic b) => !(a == b);
        public bool Equals(RandomResolutionResolver_Deterministic? other)
        {
            if (other is null)
                return this is null;
            if (this is null)
                return false;

            return Seed == other.Seed && Position == other.Position;
        }
        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Seed);
            hash.Add(Position);
            return hash.ToHashCode();
        }


        /// <summary>
        /// Gets the next deterministic 64-bit random value.
        /// </summary>
        private ulong NextUInt64()
        {
            ulong value = GetUInt64AtPosition(Seed, Position);
            Position = unchecked(Position + 1);
            return value;
        }

        /// <summary>
        /// Gets a random integer in the range
        /// [minInclusive, maxExclusive).
        /// </summary>
        private int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Must be greater than minInclusive.");

            ulong range = (ulong)((long)maxExclusive - minInclusive);

            // Rejection sampling avoids modulo bias.
            ulong threshold = unchecked(0UL - range) % range;
            while (true)
            {
                ulong value = NextUInt64();

                if (value >= threshold)
                {
                    long result = minInclusive + (long)(value % range);
                    return (int)result;
                }
            }
        }

        /// <summary>
        /// Gets a random double in the range [0, 1).
        /// </summary>
        private double NextDouble()
        {
            return (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);
        }

        /// <summary>
        /// Gets the random value at an arbitrary position without
        /// changing any generator state.
        /// </summary>
        private static ulong GetUInt64AtPosition(ulong seed, ulong position)
        {
            unchecked
            {
                ulong value = seed + position * 0x9E3779B97F4A7C15UL;
                value = (value ^ value >> 30) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ value >> 27) * 0x94D049BB133111EBUL;
                return value ^ value >> 31;
            }
        }
    }

    public static class RandomResolutionResolver_DeterministicExtensions
    {
        public static DeterministicRandomState ToState(this RandomResolutionResolver_Deterministic resolver) => new(resolver.Seed, resolver.Position);
    }
}
