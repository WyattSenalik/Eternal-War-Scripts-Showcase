// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Helpers
{
    public class CircularBuffer<T>
    {
        public int Count { get; private set; }
        public int Capacity => _buffer.Length;

        private readonly T[] _buffer;
        private int _start;
        private int _end;

        public CircularBuffer(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
            _buffer = new T[capacity];
        }

        public T this[int index]
        {
            get
            {
                ValidateIndex(index);
                return _buffer[(_start + index) % Capacity];
            }
            set
            {
                ValidateIndex(index);
                _buffer[(_start + index) % Capacity] = value;
            }
        }

        public void Add(T item)
        {
            _buffer[_end] = item;
            _end = (_end + 1) % Capacity;

            if (Count < Capacity)
            {
                Count++;
                return;
            }
            _start = (_start + 1) % Capacity;
        }

        public void Clear()
        {
            Count = 0;
            _start = 0;
            _end = 0;
        }

        public int FindIndex(Predicate<T> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            for (int i = 0; i < Count; ++i)
            {
                T value = this[i];
                if (predicate.Invoke(value))
                    return i;
            }
            return -1;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < Count; ++i)
            {
                T value = this[i];
                if (value == null)
                {
                    if (item == null)
                        return true;
                    continue;
                }
                if (value.Equals(item))
                    return true;
            }
            return false;
        }

        private void ValidateIndex(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
        }
    }
}
