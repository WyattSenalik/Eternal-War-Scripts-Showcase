using Networking.Client;
using Networking.Core;
using NUnit.Framework;
using System;

namespace Networking.Tests
{
    [TestFixture]
    internal sealed class NetworkClientTests
    {
        [Test]
        public void ConstructorRejectsNullBackend()
        {
            Assert.Throws<ArgumentNullException>(() => new NetworkClient(null!));
        }

        [Test]
        public void ClientForwardsCommandsStateEventsAndPayloads()
        {
            FakeClientBackend backend = new() { IsConnectingValue = true };
            using NetworkClient client = new(backend);
            int connectedCount = 0;
            string? disconnectReason = null;
            ReadOnlyMemory<byte> received = default;
            NetworkDeliveryMethod receivedMethod = default;
            client.Connected += () => connectedCount++;
            client.Disconnected += reason => disconnectReason = reason;
            client.MessageReceived += (message, method) => (received, receivedMethod) = (message, method);

            Assert.That(client.IsConnected, Is.False);
            Assert.That(client.IsConnecting, Is.True);
            client.Connect();
            client.Poll();
            client.Disconnect();
            client.SendMessageToServer(new byte[] { 1, 2, 3 });
            Assert.That(backend.ConnectCount, Is.EqualTo(1));
            Assert.That(backend.PollCount, Is.EqualTo(1));
            Assert.That(backend.DisconnectCount, Is.EqualTo(1));
            Assert.That(backend.LastPayload, Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(backend.LastDeliveryMethod, Is.EqualTo(NetworkDeliveryMethod.Reliable));

            backend.RaiseConnected();
            backend.RaiseDisconnected("remote closed");
            backend.RaiseMessage(new byte[] { 7, 8 }, NetworkDeliveryMethod.Unreliable);
            Assert.That(connectedCount, Is.EqualTo(1));
            Assert.That(disconnectReason, Is.EqualTo("remote closed"));
            Assert.That(received.ToArray(), Is.EqualTo(new byte[] { 7, 8 }));
            Assert.That(receivedMethod, Is.EqualTo(NetworkDeliveryMethod.Unreliable));
        }

        [Test]
        public void ClientUsesExplicitDeliveryAndDisposeDetachesAndDisposesOnce()
        {
            FakeClientBackend backend = new();
            NetworkClient client = new(backend);
            client.SendMessageToServer(new byte[] { 4 }, NetworkDeliveryMethod.Unreliable);
            Assert.That(backend.LastDeliveryMethod, Is.EqualTo(NetworkDeliveryMethod.Unreliable));

            int connectedCount = 0;
            client.Connected += () => connectedCount++;
            client.Dispose();
            client.Dispose();
            backend.RaiseConnected();

            Assert.That(connectedCount, Is.Zero);
            Assert.That(backend.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void EveryClientCommandThrowsAfterDisposal()
        {
            NetworkClient client = new(new FakeClientBackend());
            client.Dispose();

            Assert.Throws<ObjectDisposedException>(() => client.Connect());
            Assert.Throws<ObjectDisposedException>(() => client.Poll());
            Assert.Throws<ObjectDisposedException>(() => client.Disconnect());
            Assert.Throws<ObjectDisposedException>(() => client.SendMessageToServer(ReadOnlySpan<byte>.Empty));
        }

        private sealed class FakeClientBackend : INetworkClientBackend
        {
            public event Action? Connected;
            public event Action<string?>? Disconnected;
            public event Action<ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;
            public bool IsConnected { get; set; }
            public bool IsConnectingValue { get; set; }
            public bool IsConnecting => IsConnectingValue;
            public int ConnectCount { get; private set; }
            public int PollCount { get; private set; }
            public int DisconnectCount { get; private set; }
            public int DisposeCount { get; private set; }
            public byte[] LastPayload { get; private set; } = Array.Empty<byte>();
            public NetworkDeliveryMethod LastDeliveryMethod { get; private set; }

            public void Connect() => ConnectCount++;
            public void Poll() => PollCount++;
            public void Disconnect() => DisconnectCount++;
            public void SendMessageToServer(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
            {
                LastPayload = message.ToArray();
                LastDeliveryMethod = deliveryMethod;
            }
            public void Dispose() => DisposeCount++;
            public void RaiseConnected() => Connected?.Invoke();
            public void RaiseDisconnected(string? reason) => Disconnected?.Invoke(reason);
            public void RaiseMessage(byte[] data, NetworkDeliveryMethod method) => MessageReceived?.Invoke(data, method);
        }
    }
}
