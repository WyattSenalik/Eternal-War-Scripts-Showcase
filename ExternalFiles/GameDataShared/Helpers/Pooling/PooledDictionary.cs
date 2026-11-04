// Original Authors - Wyatt Senalik
using System;
using System.Collections;
using System.Collections.Generic;

namespace EternalWar.Core.Helpers.Pooling
{
    /// <summary>
    /// Scoped wrapper around <see cref="DictionaryPool"/>. Create it in a using statement and the
    /// underlying dictionary is returned to the pool on dispose.
    /// </summary>
    public sealed class PooledDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable where TKey : notnull
    {
        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set => Dictionary[key] = value;
        }

        public ICollection<TKey> Keys => Dictionary.Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Dictionary.Keys;

        public ICollection<TValue> Values => Dictionary.Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Dictionary.Values;

        public int Count => Dictionary.Count;
        public bool IsReadOnly => false;

        private Dictionary<TKey, TValue> Dictionary => _dictionary ?? throw new ObjectDisposedException(nameof(PooledDictionary<TKey, TValue>));

        private Dictionary<TKey, TValue>? _dictionary;

        public PooledDictionary()
        {
            _dictionary = DictionaryPool.Get<TKey, TValue>();
        }

        public PooledDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items) : this()
        {
            foreach (KeyValuePair<TKey, TValue> item in items)
            {
                Dictionary.Add(item.Key, item.Value);
            }
        }

        public PooledDictionary(int capacity) : this()
        {
            Dictionary.EnsureCapacity(capacity);
        }

        public void Add(TKey key, TValue value) => Dictionary.Add(key, value);
        public void Add(KeyValuePair<TKey, TValue> item) => Dictionary.Add(item.Key, item.Value);
        public void Clear() => Dictionary.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);
        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool Remove(TKey key) => Dictionary.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Remove(item);
        public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value!);

        public void Dispose()
        {
            if (_dictionary == null)
                return;

            DictionaryPool.Release<TKey, TValue>(_dictionary);
            _dictionary = null;
        }
    }
}
