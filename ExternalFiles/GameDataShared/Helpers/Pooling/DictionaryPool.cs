// Original Authors - Wyatt Senalik
using System.Collections.Generic;

namespace EternalWar.Core.Helpers.Pooling
{
    public static class DictionaryPool
    {
        public static Dictionary<TKey, TValue> Get<TKey, TValue>() where TKey : notnull
        {
            return CollectionPool.Get<Dictionary<TKey, TValue>>();
        }

        public static void Release<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            CollectionPool.Release(dictionary, Reset);
        }

        private static void Reset<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            dictionary.Clear();
        }
    }
}