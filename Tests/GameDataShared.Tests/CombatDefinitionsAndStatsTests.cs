using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Numerics;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class CombatDefinitionsAndStatsTests
    {
        private const string DATA_FOLDER_PATH = "../../../../../Data";

        [Test]
        public void AttributeStatsGetReplaceAndMergeCoverEveryAttributeIncludingWill()
        {
            AttributeStats stats = new(1, 2, 3, 4, 5, 6, 7);
            int[] expected = { 1, 2, 3, 4, 5, 6, 7 };
            AttributeStatType[] types = Enum.GetValues<AttributeStatType>();

            for (int index = 0; index < types.Length; index++)
            {
                AttributeStatType type = types[index];
                Assert.That(stats.Get(type), Is.EqualTo(expected[index]));
                Assert.That(stats.Replace(type, 20 + index).Get(type), Is.EqualTo(20 + index));
            }

            Assert.That(stats.Merge(stats), Is.EqualTo(new AttributeStats(2, 4, 6, 8, 10, 12, 14)));
            Assert.Throws<NotImplementedException>(() => stats.Get((AttributeStatType)99));
            Assert.Throws<NotImplementedException>(() => stats.Replace((AttributeStatType)99, 1));
        }

        [Test]
        public void CombatStatsGetAndReplaceCoverEveryStatAndPreserveInfiniteHealth()
        {
            CombatStats stats = new(new HealthInfo(true, 10), new AttributeStats(1, 2, 3, 4, 5, 6, 7), 8, 9);
            StatType[] types = Enum.GetValues<StatType>();
            int[] expected = { 10, 1, 2, 3, 4, 5, 8, 9, 6, 7 };

            for (int index = 0; index < types.Length; index++)
            {
                StatType type = types[index];
                Assert.That(stats.Get(type), Is.EqualTo(expected[index]));
                CombatStats replaced = stats.Replace(type, 30 + index);
                Assert.That(replaced.Get(type), Is.EqualTo(30 + index));
                Assert.That(replaced.MaxHealthInfo.HasInfiniteHealth, Is.True);
            }

            Assert.That(stats.Merge(stats), Is.EqualTo(new CombatStats(new HealthInfo(true, 20), new AttributeStats(2, 4, 6, 8, 10, 12, 14), 16, 18)));
            Assert.Throws<NotImplementedException>(() => stats.Get((StatType)99));
            Assert.Throws<NotImplementedException>(() => stats.Replace((StatType)99, 1));
        }

        [Test]
        public void StatConversionsMapAllAttributeAndVictoryTypes()
        {
            foreach (StatType statType in Enum.GetValues<StatType>())
            {
                bool isAttribute = statType is StatType.Strength or StatType.Defense or StatType.Speed or StatType.Magic or StatType.Will or StatType.Evasion or StatType.Accuracy;
                Assert.That(statType.IsAttributeStatType(), Is.EqualTo(isAttribute));
                if (isAttribute)
                    Assert.That(statType.ToAttributeStatType().ToString(), Is.EqualTo(statType.ToString()));
                else
                    Assert.Throws<Exception>(() => statType.ToAttributeStatType());
            }

            Assert.That(Enum.GetValues<VictoryStatType>().Select(type => type.ToStatType()), Is.EqualTo(new[] { StatType.Health, StatType.Strength, StatType.Defense, StatType.Speed, StatType.Magic, StatType.Will }));
            Assert.Throws<NotImplementedException>(() => ((VictoryStatType)99).ToStatType());
            Assert.That(StatType.Will.GetStatValue(new CombatStats(HealthInfo.Empty, new AttributeStats(1, 2, 3, 4, 5, 6, 7), 8, 9)), Is.EqualTo(5));
        }

        [Test]
        public void HealthAndDamageHelpersHandleAllDamageTypesAndInvalidEnums()
        {
            Assert.That(new HealthInfo(false, 3).Merge(new HealthInfo(true, 4)), Is.EqualTo(new HealthInfo(true, 7)));
            AttributeStats attributes = new(1, 2, 3, 4, 5, 6, 7);
            Assert.That(DamageType.Physical.GetDefensiveStat(attributes), Is.EqualTo(2));
            Assert.That(DamageType.Magical.GetDefensiveStat(attributes), Is.EqualTo(5));
            Assert.That(DamageType.True.GetDefensiveStat(attributes), Is.Zero);
            Assert.Throws<NotImplementedException>(() => ((DamageType)99).GetDefensiveStat(attributes));
        }

        [Test]
        public void GridAndBlockQueriesHandleBoundsHeightsAndMissingStandableBlocks()
        {
            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
            TileGridInfo grid = new(new Vector2Int(2, 3), new Vector2(1, 2), new Vector2(0.5f, 0.25f));
            Assert.That(grid.GetWorldSize(), Is.EqualTo(new Vector2(3, 6.75f)));
            Assert.That(grid.IsPositionOnGrid(new GridPosition(0, 0)), Is.True);
            Assert.That(grid.IsPositionOnGrid(new GridPosition(1, 2)), Is.True);
            Assert.That(grid.IsPositionOnGrid(new GridPosition(-1, 0)), Is.False);
            Assert.That(grid.IsPositionOnGrid(new GridPosition(2, 0)), Is.False);
            Assert.That(grid.IsPositionOnGrid(new GridPosition(0, 3)), Is.False);
            Assert.That(grid.GetAllGridPositions(), Has.Count.EqualTo(6));
            Assert.That(new TileGridInfo(Vector2Int.Zero, Vector2.One, Vector2.Zero).GetAllGridPositions(), Is.Empty);

            GridPosition position = new(0, 0);
            GridPosition emptyPosition = new(1, 1);
            GridBlocksInfo blocks = new(new ValueArray<BlockInfo>(
                new BlockInfo("Grass_Flat", position, Rotation_Restricted_Four._0, 2),
                new BlockInfo("Water_Flat", position, Rotation_Restricted_Four._0, 4),
                new BlockInfo("Grass_Flat", position, Rotation_Restricted_Four._0, 3),
                new BlockInfo("Water_Flat", emptyPosition, Rotation_Restricted_Four._0, 6)));

            Assert.That(blocks.GetHighestBlockAtPosition(database, position, onlyStandable: false)!.Height, Is.EqualTo(4));
            Assert.That(blocks.GetHighestBlockAtPosition(database, position, onlyStandable: true)!.Height, Is.EqualTo(3));
            Assert.That(blocks.GetHeightAtPosition(database, position), Is.EqualTo(3));
            Assert.That(blocks.GetStepCostAtPosition(database, position), Is.EqualTo(1));
            Assert.That(blocks.GetHighestBlockAtPosition(database, emptyPosition, onlyStandable: true), Is.Null);
            Assert.That(blocks.GetHeightAtPosition(database, emptyPosition), Is.EqualTo(-1));
            Assert.That(blocks.GetStepCostAtPosition(database, emptyPosition), Is.EqualTo(int.MaxValue));
            Assert.That(blocks.GetHighestBlockAtPosition(database, new GridPosition(9, 9), onlyStandable: false), Is.Null);
            Assert.That(blocks.IsHeightChangeDrastic(database, position, emptyPosition), Is.True);
            Assert.That(blocks.IsHeightChangeDrastic(database, position, position), Is.False);
        }
    }
}
