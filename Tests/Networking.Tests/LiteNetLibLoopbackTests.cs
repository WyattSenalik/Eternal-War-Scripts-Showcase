using Networking.Client;
using Networking.Core;
using Networking.LiteNetLib;
using Networking.Server;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Networking.Tests
{
    [TestFixture]
    internal sealed class LiteNetLibLoopbackTests
    {
        [Test]
        public void BackendsValidateEndpointsAndGuardInvalidOperations()
        {
            Assert.Throws<ArgumentNullException>(() => new LiteNetLibClientBackend(null!, 1234, ""));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LiteNetLibClientBackend("localhost", 0, ""));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LiteNetLibClientBackend("localhost", 65536, ""));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LiteNetLibServerBackend(0, ""));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LiteNetLibServerBackend(65536, ""));

            using LiteNetLibClientBackend client = new("127.0.0.1", 1234, "");
            using LiteNetLibServerBackend server = new(1234, "");
            Assert.Throws<InvalidOperationException>(() => client.SendMessageToServer(new byte[] { 1 }, NetworkDeliveryMethod.Reliable));
            Assert.Throws<InvalidOperationException>(() => server.SendMessageToClient(new NetworkConnectionId(1), new byte[] { 1 }, NetworkDeliveryMethod.Reliable));
            Assert.DoesNotThrow(() => server.DisconnectClient(new NetworkConnectionId(1)));
            Assert.DoesNotThrow(() => server.BroadcastMessage(ReadOnlySpan<byte>.Empty, NetworkDeliveryMethod.Reliable));
            Assert.DoesNotThrow(() => server.Stop());
        }

        [Test]
        public void LoopbackConnectsSendsBothDeliveryMethodsAndDisconnects()
        {
            int port = GetAvailableUdpPort();
            using NetworkServer server = new(new LiteNetLibServerBackend(port, "loopback-key"));
            using NetworkClient client = new(new LiteNetLibClientBackend("127.0.0.1", port, "loopback-key"));
            List<NetworkConnectionId> connectedClients = new();
            List<string?> disconnectReasons = new();
            byte[]? serverReceived = null;
            byte[]? clientReceived = null;
            NetworkDeliveryMethod serverReceivedMethod = default;
            NetworkDeliveryMethod clientReceivedMethod = default;

            server.ClientConnected += id => connectedClients.Add(id);
            server.ClientDisconnected += (_, reason) => disconnectReasons.Add(reason);
            server.MessageReceived += (_, message, method) => (serverReceived, serverReceivedMethod) = (message.ToArray(), method);
            client.MessageReceived += (message, method) => (clientReceived, clientReceivedMethod) = (message.ToArray(), method);

            server.Start();
            client.Connect();
            PumpUntil(() => client.IsConnected && connectedClients.Count == 1, server, client);
            Assert.That(connectedClients[0].Value, Is.GreaterThan(0));
            Assert.That(client.IsConnecting, Is.False);
            Assert.That(server.IsRunning, Is.True);

            client.SendMessageToServer(new byte[] { 10, 11 }, NetworkDeliveryMethod.Reliable);
            PumpUntil(() => serverReceived is not null, server, client);
            Assert.That(serverReceived, Is.EqualTo(new byte[] { 10, 11 }));
            Assert.That(serverReceivedMethod, Is.EqualTo(NetworkDeliveryMethod.Reliable));

            NetworkConnectionId connectedClient = connectedClients[0];
            server.SendMessageToClient(connectedClient, new byte[] { 20, 21 }, NetworkDeliveryMethod.Unreliable);
            PumpUntil(() => clientReceived is not null, server, client);
            Assert.That(clientReceived, Is.EqualTo(new byte[] { 20, 21 }));
            Assert.That(clientReceivedMethod, Is.EqualTo(NetworkDeliveryMethod.Unreliable));

            server.BroadcastMessage(new byte[] { 30 }, NetworkDeliveryMethod.Reliable);
            clientReceived = null;
            PumpUntil(() => clientReceived is not null, server, client);
            Assert.That(clientReceived, Is.EqualTo(new byte[] { 30 }));

            client.Disconnect();
            PumpUntil(() => disconnectReasons.Count == 1, server, client);
            Assert.That(client.IsConnected, Is.False);
            Assert.That(server.IsRunning, Is.True);
            server.Stop();
            Assert.That(server.IsRunning, Is.False);
            Assert.DoesNotThrow(() => server.Stop());
        }

        [Test]
        public void ServerRejectsWrongConnectionKey()
        {
            int port = GetAvailableUdpPort();
            using LiteNetLibServerBackend server = new(port, "expected-key");
            using LiteNetLibClientBackend client = new("127.0.0.1", port, "wrong-key");
            int connectedCount = 0;
            server.ClientConnected += _ => connectedCount++;

            server.Start();
            client.Connect();
            DateTime timeout = DateTime.UtcNow.AddMilliseconds(800);
            while (DateTime.UtcNow < timeout)
            {
                server.Poll();
                client.Poll();
                Thread.Sleep(1);
            }

            Assert.That(connectedCount, Is.Zero);
            Assert.That(client.IsConnected, Is.False);
        }

        private static void PumpUntil(Func<bool> condition, NetworkServer server, NetworkClient client)
        {
            DateTime timeout = DateTime.UtcNow.AddSeconds(4);
            while (!condition() && DateTime.UtcNow < timeout)
            {
                server.Poll();
                client.Poll();
                Thread.Sleep(1);
            }
            server.Poll();
            client.Poll();
            Assert.That(condition(), Is.True, "The loopback networking event did not arrive before the deadline.");
        }

        private static int GetAvailableUdpPort()
        {
            using UdpClient udpClient = new(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
        }
    }
}
