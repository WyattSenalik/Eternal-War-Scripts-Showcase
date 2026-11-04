// Original Authors - Wyatt Senalik
using System.Collections.Generic;

namespace EternalWar.Core.Helpers.Pooling
{
    public static class HashSetPool
    {
        public static HashSet<T> Get<T>()
        {
            return CollectionPool.Get<HashSet<T>>();
        }

        public static void Release<T>(HashSet<T> hashSet)
        {
            CollectionPool.Release(hashSet, Reset);
        }

        private static void Reset<T>(HashSet<T> hashSet)
        {
            hashSet.Clear();
        }
    }
}