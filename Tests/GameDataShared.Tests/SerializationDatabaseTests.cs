using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Serialization;
using NUnit.Framework;
using System;
using System.IO;
using System.Numerics;
using System.Text.Json;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class SerializationDatabaseTests
    {
        private const string DATA_FOLDER_PATH = "../../../../../Data";

        [Test]
        public void GameDataSerializationRoundTripsValueTypesAndPolymorphicModels()
        {
            ValueArray<string> valueArray = new("one", "two");
            Assert.That(GameDataSerialization.Deserialize<ValueArray<string>>(GameDataSerialization.Serialize(valueArray)), Is.EqualTo(valueArray));
            Assert.That(GameDataSerialization.Deserialize<ValueArray<string>>("null"), Is.EqualTo(ValueArray<string>.Empty));

            ValueDictionary<string, int> dictionary = new(new System.Collections.Generic.Dictionary<string, int> { ["z"] = 1, ["a"] = 2 });
            Assert.That(GameDataSerialization.Deserialize<ValueDictionary<string, int>>(GameDataSerialization.Serialize(dictionary)), Is.EqualTo(dictionary));

            SortedValueArray<int, string> sorted = new((2, "b"), (1, "a"));
            Assert.That(GameDataSerialization.Deserialize<SortedValueArray<int, string>>(GameDataSerialization.Serialize(sorted)), Is.EqualTo(sorted));

            Vector2 vector = new(2.5f, -4);
            Assert.That(GameDataSerialization.Deserialize<Vector2>(GameDataSerialization.Serialize(vector)), Is.EqualTo(vector));

            ValueAmount amount = new ValueAmount_PercentagePlusFixed(0.5f, 3);
            Assert.That(GameDataSerialization.Deserialize<ValueAmount>(GameDataSerialization.Serialize(amount)), Is.EqualTo(amount));
            NumericPredicate predicate = new NumericPredicate_And(new(new NumericPredicate_GreaterThan(0), new NumericPredicate_LessThan(10)));
            Assert.That(GameDataSerialization.Deserialize<NumericPredicate>(GameDataSerialization.Serialize(predicate)), Is.EqualTo(predicate));
        }

        [Test]
        public void GameDataSerializationRejectsUnknownPropertiesAndBadVectorShape()
        {
            Assert.Throws<JsonException>(() => GameDataSerialization.Deserialize<SerializationProbe>("{\"name\":\"x\",\"unexpected\":1}"));
            Assert.Throws<JsonException>(() => GameDataSerialization.Deserialize<Vector2>("{\"x\":1}"));
            Assert.Throws<JsonException>(() => GameDataSerialization.Deserialize<ValueArray<int>>("{\"bad\":true}"));
        }

        [Test]
        public void GameDataHashingIsStableAndChangesWhenSerializedDataChanges()
        {
            GameDataHash first = GameDataHashing.Hash(new SerializationProbe("a", 1));
            GameDataHash same = GameDataHashing.Hash(new SerializationProbe("a", 1));
            GameDataHash different = GameDataHashing.Hash(new SerializationProbe("b", 1));

            Assert.That(first.RawBytes.Count, Is.EqualTo(32));
            Assert.That(first, Is.EqualTo(same));
            Assert.That(first, Is.Not.EqualTo(different));
            Assert.That(GameDataHash.Empty.RawBytes.Count, Is.Zero);
        }

        [Test]
        public void DatabaseLoaderLoadsRepositoryDataWithAndWithoutSanityChecks()
        {
            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
            Assert.That(database.Combats.Count, Is.GreaterThan(0));
            Assert.That(database.Units.Count, Is.GreaterThan(0));
            Assert.That(database.ActionSequences.Count, Is.GreaterThan(0));
            Assert.That(database.GetCombatDefinition("Forest_Test").Id, Is.EqualTo("Forest_Test"));
            Assert.Throws<Exception>(() => database.GetCombatDefinition("missing"));

            GameDatabase checkedDatabase = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH, sanityCheck: true);
            Assert.That(checkedDatabase.Combats.Count, Is.EqualTo(database.Combats.Count));
            Assert.That(checkedDatabase.Units.Count, Is.EqualTo(database.Units.Count));
        }

        [Test]
        public void DatabaseLoaderReturnsEmptyForMissingFoldersAndSkipsMalformedFiles()
        {
            string root = Path.Combine(Path.GetTempPath(), $"ew-data-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path.Combine(root, "Units"));
            File.WriteAllText(Path.Combine(root, "Units", "malformed.json"), "{");
            try
            {
                Assert.That(DatabaseLoader.LoadFolder<EternalWar.Core.Model.Combat.Definitions.UnitDefinition>(root, "missing").Count, Is.Zero);
                Assert.That(DatabaseLoader.LoadFolder<EternalWar.Core.Model.Combat.Definitions.UnitDefinition>(root, "Units").Count, Is.Zero);
                Assert.That(DatabaseLoader.LoadDatabase(root).Units.Count, Is.Zero);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Test]
        public void DatabaseWriterAndLoaderRoundTripARealDatabaseInTemporaryStorage()
        {
            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
            string root = Path.Combine(Path.GetTempPath(), $"ew-db-{Guid.NewGuid():N}");
            try
            {
                DatabaseWriter.WriteDatabase(root, database);
                GameDatabase restored = DatabaseLoader.LoadDatabase(root, sanityCheck: true);
                Assert.That(restored.Combats, Is.EqualTo(database.Combats));
                Assert.That(restored.Units, Is.EqualTo(database.Units));
                Assert.That(restored.ActionSequences, Is.EqualTo(database.ActionSequences));
                Assert.That(Directory.GetFiles(root, "*.json", SearchOption.AllDirectories).Length, Is.GreaterThan(0));

                string unitFolder = Path.Combine(root, DatabaseFolderNames.UNITS_FOLDER_NAME);
                DatabaseWriter.WriteDataToFolder(root, DatabaseFolderNames.UNITS_FOLDER_NAME, database.Units);
                Assert.That(Directory.GetFiles(unitFolder, "*.json").Length, Is.EqualTo(database.Units.Count));
            }
            finally
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
        }

        [Test]
        public void SerializeToUtf8BytesAndJsonElementDeserializationMatchStringRoundTrip()
        {
            SerializationProbe probe = new("probe", 7);
            byte[] bytes = GameDataSerialization.SerializeToUtf8Bytes(probe);
            using JsonDocument document = JsonDocument.Parse(bytes);
            Assert.That(GameDataSerialization.Deserialize<SerializationProbe>(document.RootElement), Is.EqualTo(probe));
            Assert.That(GameDataSerialization.Deserialize<SerializationProbe>(GameDataSerialization.Serialize(probe)), Is.EqualTo(probe));
            Assert.That(GameDataSerialization.Serialize(probe), Does.Contain("\n"));
        }

        private sealed record SerializationProbe(string Name, int Value);
    }
}
