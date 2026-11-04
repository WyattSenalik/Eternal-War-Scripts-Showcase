using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
// Original Authors - Wyatt Senalik

namespace EternalWar.Core.Combat
{
    [Serializable]
    public sealed class GridPositionGroupListWrapper
    {
        [JsonIgnore]
        public IReadOnlyList<GridPositionGroup> SelectionGroups => _selectionGroups;
        [JsonIgnore]
        public int GroupIndex => _groupIndex;

        [JsonInclude]
        private readonly List<GridPositionGroup> _selectionGroups = new();
        [JsonInclude]
        private int _groupIndex = -1;

        [JsonConstructor]
        public GridPositionGroupListWrapper(List<GridPositionGroup> _selectionGroups, int _groupIndex)
        {
            this._selectionGroups = _selectionGroups;
            this._groupIndex = _groupIndex;
        }

        public GridPositionGroupListWrapper(IReadOnlyList<GridPositionGroup> selectionGroups)
        {
            _selectionGroups.AddRange(selectionGroups);
            _groupIndex = 0;
        }

        public void AdvanceToNextGroup() => ++_groupIndex;
        public void ResetGroupIndex() => _groupIndex = 0;

        public static GridPositionGroupListWrapper CreateNew() => new(new List<GridPositionGroup>(), -1);
        public static GridPositionGroupListWrapper CreateForSingleTarget(GridPosition singleTargetTile)
        {
            return new GridPositionGroupListWrapper(new GridPositionGroup[1]
            {
                new(new ValueArray<GridPosition>(singleTargetTile))
            });
        }

        public override string ToString()
        {
            return $"Index=({_groupIndex}); Groups=[{string.Join(", ", _selectionGroups)}]";
        }
    }

    public static class GridPositionGroupListWrapperExtensions
    {
        public static GridPositionGroup GetCurrentGroup(this GridPositionGroupListWrapper positionGroupListWrapper)
        {
            return positionGroupListWrapper.SelectionGroups[positionGroupListWrapper.GroupIndex];
        }
    }
}