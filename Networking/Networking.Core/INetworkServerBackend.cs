// Original Authors - Wyatt Senalik
using System;
using System.Diagnostics.CodeAnalysis;

namespace Networking.Core
{
    public interface INetworkServerBackend : IDisposable
    {
        event Action<NetworkConnectionId>? ClientConnected;
        event Action<NetworkConnectionId, string?>? ClientDisconnected;
        event Action<NetworkConnectionId, ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;

        bool IsRunning { get; }

        void Start();
        void Poll();
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Stop is fine")]
        void Stop();
        void SendMessageToClient(NetworkConnectionId client, ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod);
        void BroadcastMessage(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod);
        void DisconnectClient(NetworkConnectionId client);
    }
}
