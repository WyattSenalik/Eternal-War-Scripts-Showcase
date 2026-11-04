using Networking.Core;
using Networking.Server;
using NUnit.Framework;
using System;

namespace Networking.Tests
{
    [TestFixture]
    internal sealed class NetworkServerTests
    {
        [Test]
        public void ConnectionIdentifierFormatsInvariantlyAndInvalidIsZero()
        {
            Assert.That(NetworkConnectionId.Invalid.Value, Is.Zero);
            Assert.That(new NetworkConnectionId(123).ToString(), Is.EqualTo("123"));
        }

        [Test]
        public void ConstructorRejectsNullBackend()
        {
            Assert.Throws<ArgumentNullException>(() => new NetworkServer(null!));
        }

        [Test]
        public void ServerForwardsCommandsStateEventsAndMessages()
        {
            FakeServerBackend backend = new();
            using NetworkServer server = new(backend);
            NetworkConnectionId connectedId = default;
            NetworkConnectionId disconnectedId = default;
            string? reason = null;
            byte[] received = Array.Empty<byte>();
            NetworkDeliveryMethod receivedDelivery = default;
            server.ClientConnected += id => connectedId = id;
            server.ClientDisconnected += (id, why) => (disconnectedId, reason) = (id, why);
            server.MessageReceived += (_, message, delivery) => (received, receivedDelivery) = (message.ToArray(), delivery);

            Assert.That(server.IsRunning, Is.False);
            server.Start();
            Assert.That(server.IsRunning, Is.True);
            server.Poll();
            NetworkConnectionId target = new(42);
            server.SendMessageToClient(target, new byte[] { 1 });
            server.BroadcastMessage(new byte[] { 2 }, NetworkDeliveryMethod.Unreliable);
            server.DisconnectClient(target);
            Assert.That(backend.StartCount, Is.EqualTo(1));
            Assert.That(backend.PollCount, Is.EqualTo(1));
            Assert.That(backend.LastTarget, Is.EqualTo(target));
            Assert.That(backend.LastPayload, Is.EqualTo(new byte[] { 2 }));
            Assert.That(backend.LastDeliveryMethod, Is.EqualTo(NetworkDeliveryMethod.Unreliable));
            Assert.That(backend.DisconnectCount, Is.EqualTo(1));

            backend.RaiseClientConnected(target);
            backend.RaiseClientDisconnected(target, "gone");
            backend.RaiseMessage(target, new byte[] { 7, 8 }, NetworkDeliveryMethod.Reliable);
            Assert.That(connectedId, Is.EqualTo(target));
            Assert.That(disconnectedId, Is.EqualTo(target));
            Assert.That(reason, Is.EqualTo("gone"));
            Assert.That(received, Is.EqualTo(new byte[] { 7, 8 }));
            Assert.That(receivedDelivery, Is.EqualTo(NetworkDeliveryMethod.Reliable));
        }

        [Test]
        public void DisposeIsIdempotentDetachesCallbacksAndAllCommandsGuardDisposedState()
        {
            FakeServerBackend backend = new();
            NetworkServer server = new(backend);
            int connectionCount = 0;
            server.ClientConnected += _ => connectionCount++;
            server.Dispose();
            server.Dispose();
            backend.RaiseClientConnected(new NetworkConnectionId(1));
            Assert.That(connectionCount, Is.Zero);
            Assert.That(backend.DisposeCount, Is.EqualTo(1));

            Assert.Throws<ObjectDisposedException>(() => server.Start());
            Assert.Throws<ObjectDisposedException>(() => server.Poll());
            Assert.Throws<ObjectDisposedException>(() => server.Stop());
            Assert.Throws<ObjectDisposedException>(() => server.SendMessageToClient(new NetworkConnectionId(1), ReadOnlySpan<byte>.Empty));
            Assert.Throws<ObjectDisposedException>(() => server.BroadcastMessage(ReadOnlySpan<byte>.Empty));
            Assert.Throws<ObjectDisposedException>(() => server.DisconnectClient(new NetworkConnectionId(1)));
        }

        private sealed class FakeServerBackend : INetworkServerBackend
        {
            public event Action<NetworkConnectionId>? ClientConnected;
            public event Action<NetworkConnectionId, string?>? ClientDisconnected;
            public event Action<NetworkConnectionId, ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;
            public bool IsRunning { get; private set; }
            public int StartCount { get; private set; }
            public int PollCount { get; private set; }
            public int DisconnectCount { get; private set; }
            public int DisposeCount { get; private set; }
            public NetworkConnectionId LastTarget { get; private set; }
            public byte[] LastPayload { get; private set; } = Array.Empty<byte>();
            public NetworkDeliveryMethod LastDeliveryMethod { get; private set; }

            public void Start() { StartCount++; IsRunning = true; }
            public void Poll() => PollCount++;
            public void Stop() => IsRunning = false;
            public void SendMessageToClient(NetworkConnectionId client, ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
            {
                LastTarget = client;
                LastPayload = message.ToArray();
                LastDeliveryMethod = deliveryMethod;
            }
            public void BroadcastMessage(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
            {
                LastPayload = message.ToArray();
                LastDeliveryMethod = deliveryMethod;
            }
            public void DisconnectClient(NetworkConnectionId client) { LastTarget = client; DisconnectCount++; }
            public void Dispose() => DisposeCount++;
            public void RaiseClientConnected(NetworkConnectionId client) => ClientConnected?.Invoke(client);
            public void RaiseClientDisconnected(NetworkConnectionId client, string? why) => ClientDisconnected?.Invoke(client, why);
            public void RaiseMessage(NetworkConnectionId client, byte[] bytes, NetworkDeliveryMethod delivery) => MessageReceived?.Invoke(client, bytes, delivery);
        }
    }
}
