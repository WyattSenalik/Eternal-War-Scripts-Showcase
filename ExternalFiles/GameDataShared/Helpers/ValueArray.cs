// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers.Pooling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace EternalWar.Core.Helpers
{
    public readonly struct ValueArray<T> : IReadOnlyList<T>, IEquatable<ValueArray<T>>
    {
        public static readonly ValueArray<T> Empty = new(ImmutableArray<T>.Empty);

        public int Count => Items.Length;

        private ImmutableArray<T> Items => _items.IsDefault ? ImmutableArray<T>.Empty : _items;

        private readonly ImmutableArray<T> _items;

        public ValueArray(IEnumerable<T> items)
        {
            _items = items.ToImmutableArray();
        }

        public ValueArray(params T[] items)
        {
            _items = ImmutableArray.CreateRange(items);
        }

        private ValueArray(ImmutableArray<T> items)
        {
            _items = items;
        }

        public T this[int index] => Items[index];

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(ValueArray<T> other)
        {
            // TODO: If efficieny needs to be increased, consider restricting T to implement IEquatable<T>? and use AsSpan so that it uses MemoryExtensions.SequenceEqual instead of Enumerable.SequenceEqual.
            return Items.SequenceEqual(other.Items);
        }

        public override bool Equals(object? obj)
        {
            return obj is ValueArray<T> castedOther && Equals(castedOther);
        }

        public static bool operator ==(ValueArray<T> left, ValueArray<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ValueArray<T> left, ValueArray<T> right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            foreach (T item in Items)
            {
                hash.Add(item);
            }
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Items.Select(_Format))}]";

            static string _Format(T value)
            {
                return value switch
                {
                    null => "null",
                    string s => $"\"{s}\"",
                    _ => value.ToString() ?? "<null>"
                };
            }
        }

        public static implicit operator ValueArray<T>(T[] items) => new(items);

        public static implicit operator ValueArray<T>(ImmutableArray<T> items) => new(items);

        public sealed class Builder : IList<T>, IDisposable
        {
            public int Count => List.Count;
            public bool IsReadOnly => false;

            private List<T> List => _list ?? throw new ObjectDisposedException(nameof(Builder));

            private List<T>? _list;

            public Builder()
            {
                _list = ListPool.Get<T>();
            }

            public Builder(int capacity) : this()
            {
                List.Capacity = capacity;
            }

            public Builder(IEnumerable<T> items) : this()
            {
                List.AddRange(items);
            }

            public ValueArray<T> ToValueArray()
            {
                return new ValueArray<T>(List);
            }

            public void Dispose()
            {
                if (_list == null)
                    return;
                ListPool.Release(_list);
                _list = null;
            }

            public T this[int index]
            {
                get => List[index];
                set => List[index] = value;
            }

            public void Add(T item) => List.Add(item);

            public void AddRange(IEnumerable<T> collection) => List.AddRange(collection);

            public void Clear() => List.Clear();

            public bool Contains(T item) => List.Contains(item);

            public void CopyTo(T[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

            public IEnumerator<T> GetEnumerator() => List.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int IndexOf(T item) => List.IndexOf(item);

            public void Insert(int index, T item) => List.Insert(index, item);

            public bool Remove(T item) => List.Remove(item);

            public void RemoveAt(int index) => List.RemoveAt(index);
        }
    }

    public static class ValueArrayExtensions
    {
        public static ValueArray<T> ToValueArray<T>(this IEnumerable<T> items)
        {
            return new ValueArray<T>(items);
        }

        /// <summary>
        /// Don't use in succession. This should be the only edit you do. If you need to do more than one edit, use a Builder.
        /// </summary>
        public static ValueArray<T> Add<T>(this ValueArray<T> originalArray, params T[] items)
        {
            using ValueArray<T>.Builder builder = new(originalArray);
            builder.AddRange(items);
            return builder.ToValueArray();
        }

        /// <summary>
        /// Don't use in succession. This should be the only edit you do. If you need to do more than one edit, use a Builder.
        /// </summary>
        public static ValueArray<T> Add<T>(this ValueArray<T> originalArray, params IEnumerable<T>[] enumerables)
        {
            using ValueArray<T>.Builder builder = new(originalArray);
            foreach (IEnumerable<T> singleNumerable in enumerables)
            {
                builder.AddRange(singleNumerable);
            }
            return builder.ToValueArray();
        }

        /// <summary>
        /// Don't use in succession. This should be the only edit you do. If you need to do more than one edit, use a Builder.
        /// </summary>
        public static ValueArray<T> Remove<T>(this ValueArray<T> originalArray, T item)
        {
            using ValueArray<T>.Builder builder = new(originalArray);
            builder.Remove(item);
            return builder.ToValueArray();
        }

        /// <summary>
        /// Don't use in succession. This should be the only edit you do. If you need to do more than one edit, use a Builder.
        /// </summary>
        public static ValueArray<T> Remove<T>(this ValueArray<T> originalArray, IEnumerable<T> items)
        {
            using ValueArray<T>.Builder builder = new(originalArray);
            foreach (T item in items)
            {
                builder.Remove(item);
            }
            return builder.ToValueArray();
        }

        /// <summary>
        /// Don't use in succession. This should be the only edit you do. If you need to do more than one edit, use a Builder.
        /// </summary>
        public static ValueArray<T> RemoveAt<T>(this ValueArray<T> originalArray, int removeIndex)
        {
            using ValueArray<T>.Builder builder = new(originalArray);
            builder.RemoveAt(removeIndex);
            return builder.ToValueArray();
        }

        /// <summary>
        /// Don't use in succession. This should be the only edit you do. If you need to do more than one edit, use a Builder.
        /// </summary>
        public static ValueArray<T> ReplaceAt<T>(this ValueArray<T> originalArray, int replaceIndex, T newItem)
        {
            using ValueArray<T>.Builder builder = new(originalArray);
            builder[replaceIndex] = newItem;
            return builder.ToValueArray();
        }
    }
}
