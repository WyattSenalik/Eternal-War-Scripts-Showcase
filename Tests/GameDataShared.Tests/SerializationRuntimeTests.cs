// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Serialization;
using NUnit.Framework;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class SerializationRuntimeTests
    {
        [Test]
        public void RandomState()
        {
            DeterministicRandomState zero = new(0);
            DeterministicRandomState zeroAdvanced = new(0, 123);
            DeterministicRandomState something = new(4567);
            DeterministicRandomState somethingAdvanced = new(4567, 123);

            string zeroSerialized = GameDataSerialization.Serialize(zero);
            DeterministicRandomState? zeroDeserialized = GameDataSerialization.Deserialize<DeterministicRandomState>(zeroSerialized);

            string zeroAdvancedSerialized = GameDataSerialization.Serialize(zeroAdvanced);
            DeterministicRandomState? zeroAdvancedDeserialized = GameDataSerialization.Deserialize<DeterministicRandomState>(zeroAdvancedSerialized);

            string somethingSerialized = GameDataSerialization.Serialize(something);
            DeterministicRandomState? somethingDeserialized = GameDataSerialization.Deserialize<DeterministicRandomState>(somethingSerialized);

            string somethingAdvancedSerialized = GameDataSerialization.Serialize(somethingAdvanced);
            DeterministicRandomState? somethingAdvancedDeserialized = GameDataSerialization.Deserialize<DeterministicRandomState>(somethingAdvancedSerialized);

            Assert.That(zeroDeserialized, Is.EqualTo(zero));
            Assert.That(zeroAdvancedDeserialized, Is.EqualTo(zeroAdvanced));
            Assert.That(somethingDeserialized, Is.EqualTo(something));
            Assert.That(somethingAdvancedDeserialized, Is.EqualTo(somethingAdvanced));
        }
    }
}
