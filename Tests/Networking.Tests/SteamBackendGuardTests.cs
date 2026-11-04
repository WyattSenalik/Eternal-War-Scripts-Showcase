using Networking.Core;
using Networking.Steam;
using NUnit.Framework;
using Steamworks;
using System;

namespace Networking.Tests
{
    [TestFixture]
    internal sealed class SteamBackendGuardTests
    {
        [Test]
        public void ConstructorsValidateServerIdAndVirtualPortsWithoutInitializingSteam()
        {
            Assert.Throws<ArgumentException>(() => new SteamNetworkingClientBackend(default, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SteamNetworkingClientBackend((SteamId)1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SteamNetworkingServerBackend(-1));

            Assert.DoesNotThrow(() => new SteamNetworkingClientBackend((SteamId)1));
            Assert.DoesNotThrow(() => new SteamNetworkingServerBackend());
        }

        [Test]
        public void UnstartedSteamBackendsPollStopAndDisposeSafelyButGuardSends()
        {
            using SteamNetworkingClientBackend client = new((SteamId)1);
            using SteamNetworkingServerBackend server = new();

            Assert.That(client.IsConnected, Is.False);
            Assert.That(client.IsConnecting, Is.False);
            Assert.That(server.IsRunning, Is.False);
            Assert.DoesNotThrow(() => client.Poll());
            Assert.DoesNotThrow(() => client.Disconnect());
            Assert.DoesNotThrow(() => server.Poll());
            Assert.DoesNotThrow(() => server.Stop());
            Assert.DoesNotThrow(() => server.DisconnectClient(new NetworkConnectionId(999)));
            Assert.DoesNotThrow(() => server.BroadcastMessage(ReadOnlySpan<byte>.Empty, NetworkDeliveryMethod.Reliable));
            Assert.Throws<InvalidOperationException>(() => client.SendMessageToServer(new byte[] { 1 }, NetworkDeliveryMethod.Reliable));
            Assert.Throws<InvalidOperationException>(() => server.SendMessageToClient(new NetworkConnectionId(1), ReadOnlySpan<byte>.Empty, NetworkDeliveryMethod.Reliable));
        }
    }
}
