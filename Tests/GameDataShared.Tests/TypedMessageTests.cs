using EternalWar.Core.Network.Messages;
using EternalWar.Core.Model;
using EternalWar.Core.Network.Messages.HighLevelGame;
using EternalWar.Core.Network;
using EternalWar.Core.Serialization;
using NUnit.Framework;
using System;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class TypedMessageTests
    {
        [Test]
        public void TypedMessageRoundTripsTypedPayloadAndExtractsData()
        {
            TypedMessage original = new(NetworkMessageTypeNames.LOG, "hello");
            TypedMessage? restored = original.Serialize().DeserializeIntoTypedMessage();
            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.Type, Is.EqualTo(NetworkMessageTypeNames.LOG));
            Assert.That(restored.TryExtractDataAsString(out string? text, out string stringError), Is.True, stringError);
            Assert.That(text, Is.EqualTo("hello"));
            Assert.That(restored.TryExtractData(out string? extracted, out string typedError), Is.True, typedError);
            Assert.That(extracted, Is.EqualTo("hello"));
            Assert.That(restored.ExtractData<string>(), Is.EqualTo("hello"));
        }

        [Test]
        public void TypedMessageExtractsRecordPayloadAndReportsWrongTypesWithoutThrowing()
        {
            ChangeRequestId changeId = new(Guid.Parse("30000000-0000-0000-0000-000000000001"), Guid.Parse("30000000-0000-0000-0000-000000000002"));
            TypedMessage original = new(NetworkMessageTypeNames.CHANGE_REQUEST_DENIED, new TypedMessageData_ChangeRequestDenied(changeId));
            TypedMessage restored = original.Serialize().DeserializeIntoTypedMessage()!;
            TypedMessageData_ChangeRequestDenied extracted = restored.ExtractData<TypedMessageData_ChangeRequestDenied>();
            Assert.That(extracted.ChangeId, Is.EqualTo(changeId));

            Assert.That(restored.TryExtractData<string>(out _, out string error), Is.False);
            Assert.That(error, Is.Not.Empty);
            Assert.That(restored.TryExtractDataAsString(out _, out string stringError), Is.False);
            Assert.That(stringError, Does.Contain("Expected string"));
        }

        [Test]
        public void TypedMessageRoundTripsNetworkGameCommandBatchPayload()
        {
            ClientInfo client = new(Guid.Parse("30000000-0000-0000-0000-000000000003"), 1);
            DatedNetworkGameCommandBatch commandBatch = new(7, new NetworkGameCommand_ClientDisconnected(client));
            TypedMessage original = new(
                NetworkMessageTypeNames.NETWORK_GAME_COMMANDS_APPLIED,
                new TypedMessageData_NetworkGameCommandsApplied(GameDataHash.Empty, commandBatch, null));

            TypedMessage restored = original.Serialize().DeserializeIntoTypedMessage()!;
            TypedMessageData_NetworkGameCommandsApplied payload = restored.ExtractData<TypedMessageData_NetworkGameCommandsApplied>();

            Assert.That(payload.DatedCommandBatch, Is.EqualTo(commandBatch));
        }

        [Test]
        public void TypedMessageHandlesNullAndNonJsonPayloadsAsFailedTryOperations()
        {
            TypedMessage nullData = new("kind", null);
            Assert.That(nullData.TryExtractData<int>(out int value, out string nullError), Is.False);
            Assert.That(value, Is.Zero);
            Assert.That(nullError, Does.Contain("null"));
            Assert.Throws<Exception>(() => nullData.ExtractData<int>());

            TypedMessage directString = new("kind", "not a JsonElement");
            Assert.That(directString.TryExtractData<int>(out _, out string castError), Is.False);
            Assert.That(castError, Is.Not.Empty);

            Assert.That("null".DeserializeIntoTypedMessage(), Is.Null);
            Assert.Throws<System.Text.Json.JsonException>(() => "{".DeserializeIntoTypedMessage());
        }
    }
}
