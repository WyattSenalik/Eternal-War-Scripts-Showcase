// Original Authors - Wyatt Senalik
using System;
using System.Collections;
using System.Collections.Generic;

namespace EternalWar.Core.Helpers.Pooling
{
    /// <summary>
    /// Scoped wrapper around <see cref="HashSetPool"/>. Create it in a using statement and the
    /// underlying set is returned to the pool on dispose.
    /// </summary>
    public sealed class PooledHashSet<T> : IReadOnlyCollection<T>, IDisposable
    {
        public int Count => HashSet.Count;

        private HashSet<T> HashSet => _hashSet ?? throw new ObjectDisposedException(nameof(PooledHashSet<T>));

        private HashSet<T>? _hashSet;

        public PooledHashSet()
        {
            _hashSet = HashSetPool.Get<T>();
        }
        public PooledHashSet(params T[] items)
        {
            _hashSet = HashSetPool.Get<T>();
            foreach (T singleItem in items)
            {
                _hashSet.Add(singleItem);
            }
        }
        public PooledHashSet(IEnumerable<T> items)
        {
            _hashSet = HashSetPool.Get<T>();
            foreach (T singleItem in items)
            {
                _hashSet.Add(singleItem);
            }
        }

        public bool Add(T item) => HashSet.Add(item);
        public bool Remove(T item) => HashSet.Remove(item);
        public bool Contains(T item) => HashSet.Contains(item);
        public void Clear() => HashSet.Clear();
        public IEnumerator<T> GetEnumerator() => HashSet.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            if (_hashSet == null)
                return;

            HashSetPool.Release<T>(_hashSet);
            _hashSet = null;
        }
    }
}