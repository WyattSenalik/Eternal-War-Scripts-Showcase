// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System.Numerics;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    internal sealed class CombatView_World_Blocks(GameDatabase database, ICombatViewActions combatViewActions)
    {
        private readonly GameDatabase _database = database;
        private readonly ICombatViewActions _combatViewActions = combatViewActions;

        public void Activate(CombatRuntime combatRuntime)
        {
            CombatInstance combatInstance = combatRuntime.Instance;
            TileGridInstance tileGridInstance = combatInstance.TileGrid;
            string combatId = combatInstance.DefinitionId;

            CombatDefinition combatDefinition = _database.GetCombatDefinition(combatId);
            GridBlocksInfo gridBlocks = combatDefinition.GridBlocks;
            ValueArray<BlockInfo> blocks = gridBlocks.Blocks;

            foreach (BlockInfo blockInfo in blocks)
            {
                GridPosition gridPosition = blockInfo.Position;
                int height = blockInfo.Height;
                Vector3 worldPosition = tileGridInstance.GetWorldPosition(_database, combatId, gridPosition, height);
                _combatViewActions.CreateBlock(blockInfo, worldPosition);
            }
        }

        public void Deactivate()
        {
            _combatViewActions.DeleteAllBlocks();
        }
    }
}
