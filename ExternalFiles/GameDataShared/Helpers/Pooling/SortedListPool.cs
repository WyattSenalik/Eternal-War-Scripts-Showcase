// Original Authors - Wyatt Senalik
using System.Collections.Generic;

namespace EternalWar.Core.Helpers.Pooling
{
    public static class SortedListPool
    {
        public static SortedList<TKey, TValue> Get<TKey, TValue>() where TKey : notnull
        {
            return CollectionPool.Get<SortedList<TKey, TValue>>();
        }

        public static void Release<TKey, TValue>(SortedList<TKey, TValue> list) where TKey : notnull
        {
            CollectionPool.Release(list, Reset);
        }

        private static void Reset<TKey, TValue>(SortedList<TKey, TValue> list) where TKey : notnull
        {
            list.Clear();
        }
    }
}