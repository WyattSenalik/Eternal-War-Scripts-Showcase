// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers.Pooling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EternalWar.Core.Helpers
{
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "It is meant to be very similar to ValueArray, so the naming should match")]
    public sealed class SortedValueArray<TKey, TValue> : IReadOnlyList<TValue>, IReadOnlyDictionary<TKey, TValue>, IEquatable<SortedValueArray<TKey, TValue>> where TKey : notnull
    {
        public static readonly SortedValueArray<TKey, TValue> Empty = new();

        public int Count => _list.Count;
        public IEnumerable<TKey> Keys => _list.Keys;
        public IEnumerable<TValue> Values => _list.Values;

        private readonly SortedList<TKey, TValue> _list;

        private SortedValueArray()
        {
            _list = new SortedList<TKey, TValue>();
        }

        public SortedValueArray(params (TKey, TValue)[] kvps) : this()
        {
            _list.Capacity = kvps.Length;
            foreach ((TKey key, TValue value) in kvps)
            {
                _list.Add(key, value);
            }
        }

        public SortedValueArray(params KeyValuePair<TKey, TValue>[] kvps) : this()
        {
            _list.Capacity = kvps.Length;
            foreach ((TKey key, TValue value) in kvps)
            {
                _list.Add(key, value);
            }
        }

        public SortedValueArray(int capacity)
        {
            _list = new SortedList<TKey, TValue>(capacity);
        }

        public SortedValueArray(IComparer<TKey>? comparer)
        {
            _list = new SortedList<TKey, TValue>(comparer);
        }

        public SortedValueArray(IDictionary<TKey, TValue> dictionary)
        {
            _list = new SortedList<TKey, TValue>(dictionary);
        }

        public SortedValueArray(int capacity, IComparer<TKey>? comparer)
        {
            _list = new SortedList<TKey, TValue>(capacity, comparer);
        }

        public SortedValueArray(IDictionary<TKey, TValue> dictionary, IComparer<TKey>? comparer)
        {
            _list = new SortedList<TKey, TValue>(dictionary, comparer);
        }

        public SortedValueArray(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            _list = new SortedList<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValue> item in items)
            {
                _list.Add(item.Key, item.Value);
            }
        }

        public TValue this[int index] => _list.Values[index];

        public TValue this[TKey key] => _list[key];

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _list.GetEnumerator();

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => _list.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool ContainsKey(TKey key) => _list.ContainsKey(key);

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _list.TryGetValue(key, out value);

        public bool Equals(SortedValueArray<TKey, TValue>? other)
        {
            if (other is null)
                return false;

            // TODO: If efficieny needs to be increased, consider restricting T to implement IEquatable<T>? and use AsSpan so that it uses MemoryExtensions.SequenceEqual instead of Enumerable.SequenceEqual.
            return _list.SequenceEqual(other._list);
        }

        public override bool Equals(object? obj)
        {
            return obj is SortedValueArray<TKey, TValue> castedObj && Equals(castedObj);
        }

        public static bool operator ==(SortedValueArray<TKey, TValue> left, SortedValueArray<TKey, TValue> right)
        {
            return ReferenceEquals(left, right) || left?.Equals(right) == true;
        }

        public static bool operator !=(SortedValueArray<TKey, TValue> left, SortedValueArray<TKey, TValue> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            foreach (KeyValuePair<TKey, TValue> kvp in _list)
            {
                hash.Add(kvp.Key);
                hash.Add(kvp.Value);
            }
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", _list.Select(_Format))}]";

            static string _Format(KeyValuePair<TKey, TValue> kvp)
            {
                string valueString = kvp.Value switch
                {
                    null => "null",
                    string s => $"\"{s}\"",
                    _ => kvp.Value.ToString() ?? "<null>"
                };

                return $"({kvp.Key})={valueString}";
            }
        }

        public static implicit operator SortedValueArray<TKey, TValue>(KeyValuePair<TKey, TValue>[] items) => new(items);

        public sealed class Builder : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable<TKey>, IDisposable
        {
            public int Count => List.Count;
            public bool IsReadOnly => false;

            private SortedList<TKey, TValue> List => _list ?? throw new ObjectDisposedException(nameof(Builder));

            private SortedList<TKey, TValue>? _list;

            public Builder()
            {
                _list = SortedListPool.Get<TKey, TValue>();
            }

            public Builder(int capacity) : this()
            {
                List.Capacity = capacity;
            }

            public Builder(IEnumerable<KeyValuePair<TKey, TValue>> items) : this()
            {
                foreach (KeyValuePair<TKey, TValue> item in items)
                {
                    List.Add(item.Key, item.Value);
                }
            }

            public SortedValueArray<TKey, TValue> ToSortedValueArray()
            {
                return new SortedValueArray<TKey, TValue>(List);
            }

            public void Dispose()
            {
                if (_list == null)
                    return;
                SortedListPool.Release(_list);
                _list = null;
            }

            public TValue this[TKey key]
            {
                get => List[key];
                set => List[key] = value;
            }

            public KeyValuePair<TKey, TValue> this[int index]
            {
                get => new(List.Keys[index], List.Values[index]);
            }

            public void Add(TKey key, TValue value) => List.Add(key, value);

            public void Add(KeyValuePair<TKey, TValue> kvp) => List.Add(kvp.Key, kvp.Value);

            public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in collection)
                {
                    List.Add(kvp.Key, kvp.Value);
                }
            }

            public void Clear() => List.Clear();

            public bool ContainsKey(TKey key) => List.ContainsKey(key);

            public bool ContainsValue(TValue value) => List.ContainsValue(value);

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => List.GetEnumerator();

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => List.Keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int IndexOfKey(TKey key) => List.IndexOfKey(key);

            public int IndexOfValue(TValue value) => List.IndexOfValue(value);

            public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue? value) => List.TryGetValue(key, out value);

            public bool Remove(TKey key) => List.Remove(key);

            public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue? value) => List.Remove(key, out value);

            public bool Remove(KeyValuePair<TKey, TValue> kvp) => List.Remove(kvp.Key);

            public void RemoveAt(int index) => List.RemoveAt(index);
        }
    }

    public static class SortedValueArrayExtensions
    {
        public static KeyValuePair<TKey, TValue>[] ToArray<TKey, TValue>(this SortedValueArray<TKey, TValue> sortedValueArray) where TKey : notnull
        {
            KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[sortedValueArray.Count];
            int index = 0;
            foreach (KeyValuePair<TKey, TValue> kvp in sortedValueArray)
            {
                array[index] = kvp;
                ++index;
            }
            return array;
        }
    }
}
