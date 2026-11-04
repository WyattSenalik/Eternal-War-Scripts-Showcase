// Original Authors - Wyatt Senalik
using System.Collections.Generic;

namespace EternalWar.Core.Helpers.Pooling
{
    public static class ListPool
    {
        public static List<T> Get<T>()
        {
            return CollectionPool.Get<List<T>>();
        }

        public static void Release<T>(List<T> list)
        {
            CollectionPool.Release(list, Reset);
        }

        private static void Reset<T>(List<T> list)
        {
            list.Clear();
        }
    }
}