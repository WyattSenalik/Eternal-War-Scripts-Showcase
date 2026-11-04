// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public sealed record CombatInstance(string DefinitionId, TileGridInstance TileGrid, ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild> UnitMap);

    public sealed record TileGridInstance(Vector3 WorldOffset);

    // TODO: This was put here to help w/ when classes are prestiged, but I'm unsure how we're going to handle that exactly, so this is current not being used and might be replaced with something else
    public sealed record UnitBuild(CombatUnitUniqueIdentifier UniqueIdentifier, GridPosition StartingPosition, EightDirection StartingFacingDirection, CombatStats StatChanges, ValueArray<string> CombatActionIds, ValueArray<string> ArtifactIds);

    public static class CombatInstanceExtensions
    {
        public static UnitBuild GetUnitBuild(this CombatInstance instance, CombatUnitUniqueIdentifier uniqueIdentifier)
        {
            ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild> unitMap = instance.UnitMap;
            if (!unitMap.TryGetValue(uniqueIdentifier, out UnitBuild? unitBuild))
                throw new Exception($"No unit build for unit with id ({uniqueIdentifier})");
            return unitBuild;
        }
    }

    public static class TileGridInstanceExtensions
    {
        public static Vector3 GetWorldPosition(this TileGridInstance tileGridInstance, GameDatabase database, string combatId, GridPosition gridPosition, int? heightOverride = null)
        {
            CombatDefinition combatDefinition = database.GetCombatDefinition(combatId);

            TileGridInfo tileGridInfo = combatDefinition.TileGrid;
            GridBlocksInfo gridBlocks = combatDefinition.GridBlocks;

            Vector3 worldOffset = tileGridInstance.WorldOffset;
            Vector2 tileSize = tileGridInfo.TileSize;
            Vector2 tileSpacing = tileGridInfo.TileSpacing;
            Vector2Int gridPositionVector = gridPosition.Position;

            int height = heightOverride ?? gridBlocks.GetHeightAtPosition(database, gridPosition);

            float x = gridPositionVector.X * (tileSize.X + tileSpacing.X);
            float y = (height * CombatConstants.BLOCK_HEIGHT) + worldOffset.Y;
            float z = gridPositionVector.Y * (tileSize.Y + tileSpacing.Y);
            return new Vector3(x, y, z);
        }

        public static GridPosition GetGridPosition(this TileGridInstance tileGridInstance, CombatDefinition combatDefinition, Vector3 worldPosition)
        {
            TileGridInfo tileGridInfo = combatDefinition.TileGrid;

            Vector3 worldOffset = tileGridInstance.WorldOffset;
            Vector2 tileSize = tileGridInfo.TileSize;
            Vector2 tileSpacing = tileGridInfo.TileSpacing;

            worldPosition -= worldOffset;
            int x = (int)Math.Round(worldPosition.X / (tileSize.X + tileSpacing.X));
            int y = (int)Math.Round(worldPosition.Z / (tileSize.Y + tileSpacing.Y));
            Vector2Int gridPositionVector = new(x, y);
            return new GridPosition(gridPositionVector);
        }
    }

    public static class UnitBuildExtensions
    {
        public static ValueArray<string> GetAllActionSequenceIds(this UnitBuild unitBuild, GameDatabase database)
        {
            CombatUnitUniqueIdentifier uniqueIdentifier = unitBuild.UniqueIdentifier;
            UnitDefinition unitDefinition = database.GetUnitDefinition(uniqueIdentifier);

            ValueArray<string> actionIds = unitBuild.CombatActionIds;
            return actionIds.Add(unitDefinition.MoveCombatActionId);
        }

        public static bool HasActionSequenceId(this UnitBuild unitBuild, GameDatabase database, string actionSequenceId)
        {
            ValueArray<string> allActionIds = unitBuild.GetAllActionSequenceIds(database);
            return allActionIds.Contains(actionSequenceId);
        }
    }
}
