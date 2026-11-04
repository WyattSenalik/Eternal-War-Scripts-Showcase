// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers.Pooling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace EternalWar.Core.Helpers
{
    public readonly struct ValueDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEquatable<ValueDictionary<TKey, TValue>> where TKey : notnull
    {
        public static readonly ValueDictionary<TKey, TValue> Empty = new();

        public int Count => _dictionary.Count;
        public IEnumerable<TKey> Keys => _dictionary.Keys;
        public IEnumerable<TValue> Values => _dictionary.Values;

        private readonly ImmutableDictionary<TKey, TValue> _dictionary;

        public ValueDictionary()
        {
            _dictionary = ImmutableDictionary<TKey, TValue>.Empty;
        }

        public ValueDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            _dictionary = items.ToImmutableDictionary();
        }

        public ValueDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary.ToImmutableDictionary();
        }

        private ValueDictionary(ImmutableDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public TValue this[TKey key] => _dictionary[key];

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value!);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(ValueDictionary<TKey, TValue> other)
        {
            if (Count != other.Count)
                return false;

            EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
            ImmutableDictionary<TKey, TValue> otherDictionary = other._dictionary;

            foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
            {
                if (!otherDictionary.TryGetValue(pair.Key, out TValue? value))
                    return false;

                if (!comparer.Equals(pair.Value, value))
                    return false;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is not null && obj is ValueDictionary<TKey, TValue> castedOther && Equals(castedOther);
        }

        public override int GetHashCode()
        {
            HashCode hash = new();

            // Order-independent hash
            int combined = 0;
            foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
            {
                HashCode pairHash = new();
                pairHash.Add(pair.Key);
                pairHash.Add(pair.Value);

                combined ^= pairHash.ToHashCode();
            }

            hash.Add(combined);
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"ValueDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}> {{ Count = {Count}, Items = [{string.Join(", ", _dictionary.Select(p => $"{_Format(p.Key)} = {_Format(p.Value)}"))}] }}";

            static string _Format(object? value)
            {
                return value switch
                {
                    null => "null",
                    string s => $"\"{s}\"",
                    _ => value.ToString() ?? "<null>"
                };
            }
        }

        public static bool operator ==(ValueDictionary<TKey, TValue> left, ValueDictionary<TKey, TValue> right) => left.Equals(right);

        public static bool operator !=(ValueDictionary<TKey, TValue> left, ValueDictionary<TKey, TValue> right) => !left.Equals(right);

        public static implicit operator ValueDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary) => new(dictionary);

        public static implicit operator ValueDictionary<TKey, TValue>(ImmutableDictionary<TKey, TValue> dictionary) => new(dictionary);

#pragma warning disable CA1000 // Do not declare static members on generic types
        public static Builder CreateBuilder() => Builder.Create();
        public static Builder CreateBuilder(int capacity) => Builder.Create(capacity);
        public static Builder CreateBuilder(IEnumerable<KeyValuePair<TKey, TValue>> items) => Builder.Create(items);
#pragma warning restore CA1000 // Do not declare static members on generic types

#pragma warning disable CA1710 // Identifiers should have correct suffix
        public sealed class Builder : IDictionary<TKey, TValue>, IDisposable
#pragma warning restore CA1710 // Identifiers should have correct suffix
        {
            public int Count => Dictionary.Count;

            public bool IsReadOnly => false;

            public ICollection<TKey> Keys => Dictionary.Keys;

            public ICollection<TValue> Values => Dictionary.Values;

            private Dictionary<TKey, TValue>? _dictionary;

            private Builder()
            {
                _dictionary = DictionaryPool.Get<TKey, TValue>();
            }

            private Dictionary<TKey, TValue> Dictionary => _dictionary ?? throw new ObjectDisposedException(nameof(Builder));

            public ValueDictionary<TKey, TValue> ToValueDictionary()
            {
                return new ValueDictionary<TKey, TValue>(Dictionary);
            }

            public void Dispose()
            {
                if (_dictionary == null)
                    return;

                DictionaryPool.Release(_dictionary);
                _dictionary = null;
            }

            public TValue this[TKey key]
            {
                get => Dictionary[key];
                set => Dictionary[key] = value;
            }

            public void Add(TKey key, TValue value) => Dictionary.Add(key, value);

            public void Add(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Dictionary).Add(item);

            public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
            {
                foreach (KeyValuePair<TKey, TValue> pair in items)
                {
                    Dictionary.Add(pair.Key, pair.Value);
                }
            }

            public bool Remove(TKey key) => Dictionary.Remove(key);

            public bool Remove(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Dictionary).Remove(item);

            public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

            public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value!);

            public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Dictionary).Contains(item);

            public void Clear() => Dictionary.Clear();

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)Dictionary).CopyTo(array, arrayIndex);

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#pragma warning disable CA1000 // Do not declare static members on generic types
            public static Builder Create() => new();

            public static Builder Create(int capacity)
            {
                Builder builder = new();
                builder.Dictionary.EnsureCapacity(capacity);
                return builder;
            }

            public static Builder Create(IEnumerable<KeyValuePair<TKey, TValue>> items)
            {
                Builder builder = new();
                builder.AddRange(items);
                return builder;
            }
#pragma warning restore CA1000 // Do not declare static members on generic types
        }
    }

    public static class ValueDictionaryExtensions
    {
        public static ValueDictionary<TKey, TValue> ToValueDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
        {
            return new ValueDictionary<TKey, TValue>(items);
        }
    }
}
