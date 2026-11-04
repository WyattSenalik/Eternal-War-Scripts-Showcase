// Original Authors - Wyatt Senalik
using System;

namespace Networking.Core
{
    public interface INetworkClientBackend : IDisposable
    {
        event Action? Connected;
        event Action<string?>? Disconnected;
        event Action<ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;

        bool IsConnected { get; }
        bool IsConnecting { get; }

        void Connect();
        void Poll();
        void Disconnect();
        void SendMessageToServer(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod);
    }
}
