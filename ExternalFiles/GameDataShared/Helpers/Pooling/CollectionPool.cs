// Original Authors - Wyatt Senalik
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Helpers.Pooling
{
    public static class CollectionPool
    {
        private static readonly Dictionary<Type, Stack<object>> _poolDictionary = new();

        public static TCollection Get<TCollection>() where TCollection : class, new()
        {
            lock (_poolDictionary)
            {
                Type type = typeof(TCollection);
                if (_poolDictionary.TryGetValue(type, out Stack<object>? pool) && pool.Count > 0)
                {
                    object obj = pool.Pop();
                    if (obj is TCollection castedObj)
                        return castedObj;
                    throw new Exception($"Failed to cast object in pool to {type.Name}");
                }
            }
            return new TCollection();
        }

        public static void Release<TCollection>(TCollection collection, Action<TCollection> resetCallback) where TCollection : class, new()
        {
            resetCallback.Invoke(collection); // Like clear, determined by caller
            lock (_poolDictionary)
            {
                Type type = typeof(TCollection);
                if (_poolDictionary.TryGetValue(type, out Stack<object>? pool))
                {
                    pool.Push(collection);
                    return;
                }
                pool = new Stack<object>();
                pool.Push(collection);
                _poolDictionary.Add(type, pool);
            }
        }
    }
}