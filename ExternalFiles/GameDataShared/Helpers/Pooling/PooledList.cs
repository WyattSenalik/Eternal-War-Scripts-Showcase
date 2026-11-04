// Original Authors - Wyatt Senalik
using System;
using System.Collections;
using System.Collections.Generic;

namespace EternalWar.Core.Helpers.Pooling
{
    public sealed class PooledList<T> : IDisposable, IList<T>, IReadOnlyList<T>
    {
        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }
        public int Count => _list.Count;
        public bool IsReadOnly => false;

        private readonly List<T> _list;

        public PooledList()
        {
            _list = ListPool.Get<T>();
        }
        public PooledList(IEnumerable<T> items) : this()
        {
            _list.AddRange(items);
        }
        public PooledList(int capacity) : this()
        {
            _list.Capacity = capacity;
        }

        public void Add(T item) => _list.Add(item);

        public void Clear() => _list.Clear();

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item) => _list.Insert(index, item);

        public bool Remove(T item) => _list.Remove(item);

        public void RemoveAt(int index) => _list.RemoveAt(index);

        public void Dispose() => ListPool.Release(_list);

        public void AddRange(IEnumerable<T> enumerable) => _list.AddRange(enumerable);

        public int FindIndex(Predicate<T> predicate) => _list.FindIndex(predicate);

        public void Sort() => _list.Sort();
        public void Sort(Comparison<T> comparison) => _list.Sort(comparison);
        public void Sort(IComparer<T>? comparer) => _list.Sort(comparer);
        public void Sort(int index, int count, IComparer<T>? comparer) => _list.Sort(index, count, comparer);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}