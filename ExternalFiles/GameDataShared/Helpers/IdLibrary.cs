// Original Authors - Wyatt Senalik
using System.Collections.Generic;

namespace EternalWar.Core.Helpers
{
    public sealed class IdLibrary
    {
        private const int STARTING_VALUE = 0;

        private readonly List<int> _availableIds;
        private int _numbersUsed;

        public IdLibrary(int capacity = 32)
        {
            _availableIds = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
                _availableIds.Add(STARTING_VALUE + i);
            _numbersUsed = capacity;
        }

        public int CheckoutID()
        {
            if (_availableIds.Count <= 0)
                return STARTING_VALUE + _numbersUsed++;

            int lastIndex = _availableIds.Count - 1;
            int checkedOutId = _availableIds[lastIndex];
            _availableIds.RemoveAt(lastIndex);
            return checkedOutId;

        }

        public void ReturnID(int id)
        {
            if (id < STARTING_VALUE || id >= STARTING_VALUE + _numbersUsed)
                return;

            int t_foundIndex = _availableIds.BinarySearch(id);
            if (t_foundIndex >= 0)
                return;
            _availableIds.Insert(~t_foundIndex, id);
        }

        public bool AreAllIDsReturned()
        {
            return _availableIds.Count == _numbersUsed;
        }

        public void Reset()
        {
            int capacity = _availableIds.Capacity;
            _availableIds.Clear();
            for (int i = 0; i < capacity; i++)
                _availableIds.Add(STARTING_VALUE + i);
            _numbersUsed = capacity;
        }
    }
}