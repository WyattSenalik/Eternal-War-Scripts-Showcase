// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Engine.Godot.Run.Combat.Resources;
using EternalWar.Engine.Godot.Run.Map.Resources;
using Godot;
using System;
using System.Collections.Generic;

namespace EternalWar.Engine.Godot
{
    [GlobalClass]
    public sealed partial class GodotGameVisualDatabaseResource : Resource
    {
        public GodotCombatVisualDatabaseResource CombatDatabase => _combatDatabase ?? throw new ArgumentNullException(nameof(_combatDatabase));
        public GodotMapVisualDatabaseResource MapDatabase => _mapDatabase ?? throw new ArgumentNullException(nameof(_mapDatabase));

        [Export] private GodotCombatVisualDatabaseResource? _combatDatabase;
        [Export] private GodotMapVisualDatabaseResource? _mapDatabase;
    }

    public sealed record GodotGameVisualDatabase(
        PackedScene CombatNodeCollectionPrefab,
        PackedScene UnitTurnCellUIPrefab,
        PackedScene CombatActionButtonPrefab,
        ValueDictionary<string, GodotBlockVisualResource> Blocks,
        ValueDictionary<string, GodotUnitVisualResource> Units,
        ValueDictionary<string, GodotArtifactVisualResource> Artifacts,
        PackedScene MapNodePrefab,
        PackedScene MapNodeRowPrefab);

    public static class GodotGameVisualDatabaseResourceExtensions
    {
        public static GodotGameVisualDatabase CreateOptimizedDatabase(this GodotGameVisualDatabaseResource resource)
        {
            GodotCombatVisualDatabaseResource combatVisualDatabaseResource = resource.CombatDatabase;
            PackedScene combatNodeCollectionPrefab = combatVisualDatabaseResource.CombatNodeCollectionPrefab;
            PackedScene unitTurnCellUIPrefab = combatVisualDatabaseResource.UnitTurnCellUIPrefab;
            PackedScene combatActionButtonPrefab = combatVisualDatabaseResource.CombatActionButtonPrefab;

            IReadOnlyList<GodotBlockVisualResource> blocks = combatVisualDatabaseResource.Blocks;
            IReadOnlyList<GodotUnitVisualResource> units = combatVisualDatabaseResource.Units;
            IReadOnlyList<GodotArtifactVisualResource> artifacts = combatVisualDatabaseResource.Artifacts;

            ValueDictionary<string, GodotBlockVisualResource> blocksDictionary = CreateBlocksDictionary(blocks);
            ValueDictionary<string, GodotUnitVisualResource> unitsDictionary = CreateUnitsDictionary(units);
            ValueDictionary<string, GodotArtifactVisualResource> artifactsDictionary = CreateArtifactsDictionary(artifacts);

            GodotMapVisualDatabaseResource mapVisualDatabaseResource = resource.MapDatabase;
            PackedScene mapNodePrefab = mapVisualDatabaseResource.NodePrefab;
            PackedScene mapNodeRowPrefab = mapVisualDatabaseResource.NodeRowPrefab;

            return new GodotGameVisualDatabase(combatNodeCollectionPrefab, unitTurnCellUIPrefab, combatActionButtonPrefab, blocksDictionary, unitsDictionary, artifactsDictionary, mapNodePrefab, mapNodeRowPrefab);
        }

        private static ValueDictionary<string, GodotBlockVisualResource> CreateBlocksDictionary(IReadOnlyList<GodotBlockVisualResource> blocks)
        {
            using ValueDictionary<string, GodotBlockVisualResource>.Builder blocksBuilder = ValueDictionary<string, GodotBlockVisualResource>.CreateBuilder();

            foreach (GodotBlockVisualResource blockResource in blocks)
            {
                string definitionId = blockResource.DefinitionId;
                blocksBuilder.Add(definitionId, blockResource);
            }

            return blocksBuilder.ToValueDictionary();
        }

        private static ValueDictionary<string, GodotUnitVisualResource> CreateUnitsDictionary(IReadOnlyList<GodotUnitVisualResource> units)
        {
            using ValueDictionary<string, GodotUnitVisualResource>.Builder unitsBuilder = ValueDictionary<string, GodotUnitVisualResource>.CreateBuilder();

            foreach (GodotUnitVisualResource unitResource in units)
            {
                string definitionId = unitResource.DefinitionId;
                unitsBuilder.Add(definitionId, unitResource);
            }

            return unitsBuilder.ToValueDictionary();
        }

        private static ValueDictionary<string, GodotArtifactVisualResource> CreateArtifactsDictionary(IReadOnlyList<GodotArtifactVisualResource> artifacts)
        {
            using ValueDictionary<string, GodotArtifactVisualResource>.Builder artifactsBuilder = ValueDictionary<string, GodotArtifactVisualResource>.CreateBuilder();

            foreach (GodotArtifactVisualResource artifactResource in artifacts)
            {
                string definitionId = artifactResource.DefinitionId;
                artifactsBuilder.Add(definitionId, artifactResource);
            }

            return artifactsBuilder.ToValueDictionary();
        }
    }

    public static class GodotGameVisualDatabaseExtensions
    {
        public static GodotBlockVisualResource GetBlock(this GodotGameVisualDatabase database, string id)
        {
            ValueDictionary<string, GodotBlockVisualResource> blocks = database.Blocks;
            return blocks[id];
        }

        public static GodotUnitVisualResource GetUnit(this GodotGameVisualDatabase database, string id)
        {
            ValueDictionary<string, GodotUnitVisualResource> units = database.Units;
            return units[id];
        }

        public static GodotArtifactVisualResource GetArtifact(this GodotGameVisualDatabase database, string id)
        {
            ValueDictionary<string, GodotArtifactVisualResource> artifacts = database.Artifacts;
            return artifacts[id];
        }
    }
}
