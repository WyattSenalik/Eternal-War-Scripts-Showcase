// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record GridBlocksInfo(ValueArray<BlockInfo> Blocks);

    public sealed record BlockInfo(string DefinitionId, GridPosition Position, Rotation_Restricted_Four Rotation, int Height);

    public sealed record BlockDefinition(string Id, int ExtraStepCost, bool CanStandOn) : IHaveStringId;

    public static class GridBlocksInfoExtensions
    {
        public static int GetHeightAtPosition(this GridBlocksInfo gridBlocks, GameDatabase database, GridPosition gridPosition)
        {
            BlockInfo? highestBlock = gridBlocks.GetHighestBlockAtPosition(database, gridPosition, onlyStandable: true);
            return highestBlock != null ? highestBlock.Height : -1;
        }

        public static bool IsHeightChangeDrastic(this GridBlocksInfo gridBlocks, GameDatabase database, GridPosition fromPosition, GridPosition toPosition)
        {
            const int DRASTIC_HEIGHT_BAR = 1;

            int fromHeight = gridBlocks.GetHeightAtPosition(database, fromPosition);
            int toHeight = gridBlocks.GetHeightAtPosition(database, toPosition);
            int heightDifference = Math.Abs(toHeight - fromHeight);
            return heightDifference > DRASTIC_HEIGHT_BAR;
        }

        public static int GetStepCostAtPosition(this GridBlocksInfo gridBlocks, GameDatabase database, GridPosition gridPosition)
        {
            BlockInfo? blockInfo = gridBlocks.GetHighestBlockAtPosition(database, gridPosition, onlyStandable: true);
            if (blockInfo == null)
                return int.MaxValue;
            
            string blockDefinitionId = blockInfo.DefinitionId;
            BlockDefinition blockDefinition = database.GetBlockDefinition(blockDefinitionId);
            return 1 + blockDefinition.ExtraStepCost;
        }

        public static BlockInfo? GetHighestBlockAtPosition(this GridBlocksInfo gridBlocks, GameDatabase database, GridPosition gridPosition, bool onlyStandable)
        {
            ValueArray<BlockInfo> blocksAtPosition = GetBlocksAtGridPosition(gridBlocks, gridPosition);
            if (blocksAtPosition.Count <= 0)
                return null;
            // TODO: Need to create a cached pre-sorted list somewhere.
            using PooledList<BlockInfo> blocks = new(blocksAtPosition);
            blocks.Sort((BlockInfo a, BlockInfo b) => b.Height.CompareTo(a.Height));
            if (!onlyStandable)
                return blocks[0];
            return blocks.FirstOrDefault(block => database.GetBlockDefinition(block.DefinitionId).CanStandOn);
        }

        private static ValueArray<BlockInfo> GetBlocksAtGridPosition(GridBlocksInfo gridBlocks, GridPosition gridPosition)
        {
            // TODO: Need to create a cached dictionary somewhere.
            ValueArray<BlockInfo> blocks = gridBlocks.Blocks;
            return blocks.Where(singleBlock => singleBlock.Position == gridPosition).ToValueArray();
        }
    }
}
