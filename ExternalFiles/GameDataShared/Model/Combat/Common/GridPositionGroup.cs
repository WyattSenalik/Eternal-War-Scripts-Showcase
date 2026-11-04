// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Common
{
    [Serializable]
    public sealed record GridPositionGroup(ValueArray<GridPosition> Positions)
    {
        public static readonly GridPositionGroup EMPTY = new(ValueArray<GridPosition>.Empty);

        public static GridPositionGroup CreateForSinglePosition(GridPosition position)
        {
            ValueArray<GridPosition> positions = new(position);
            return new GridPositionGroup(positions);
        }

        public override string ToString()
        {
            return '[' + string.Join(", ", Positions) + ']';
        }
    }

    public static class GridPositionGroupExtensions
    {
        public static ValueArray<ValueArray<GridPositionGroup>> GetAllCombinationSets(this IReadOnlyList<(GridPositionGroup Group, int Amount)> selections)
        {
            using ValueArray<ValueArray<GridPositionGroup>>.Builder builder = new();
            selections.ForEachCombinationSet(builder.Add);
            return builder.ToValueArray();
        }

        public static void ForEachCombinationSet(this IReadOnlyList<(GridPositionGroup Group, int Amount)> selections, Action<ValueArray<GridPositionGroup>> callback)
        {
            using PooledList<GridPositionGroup> current = new();
            _Recurse(0);

            void _Recurse(int index)
            {
                if (index == selections.Count)
                {
                    callback.Invoke(current.ToValueArray());
                    return;
                }

                (GridPositionGroup group, int amount) = selections[index];
                group.ForEachCombination(amount, combination =>
                {
                    current.Add(combination);

                    _Recurse(index + 1);

                    current.RemoveAt(current.Count - 1);
                });
            }
        }

        public static void ForEachCombination(this GridPositionGroup group, int numberToSelect, Action<GridPositionGroup> callback)
        {
            ValueArray<GridPosition> groupPositions = group.Positions;
            using PooledList<GridPosition> current = new();
            _Recurse(0);

            void _Recurse(int index)
            {
                if (current.Count == numberToSelect)
                {
                    callback.Invoke(new GridPositionGroup(current.ToValueArray()));
                    return;
                }

                for (int i = index;
                    i <= groupPositions.Count - (numberToSelect - current.Count); // Doing this prunes branches that can't reach number to select.
                    ++i)
                {
                    current.Add(groupPositions[i]);

                    _Recurse(i + 1);

                    current.RemoveAt(current.Count - 1);
                }
            }
        }
    }
}