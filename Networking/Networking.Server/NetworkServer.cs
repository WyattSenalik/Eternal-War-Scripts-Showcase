// Original Authors - Wyatt Senalik
using Networking.Core;
using System;

namespace Networking.Server
{
    public sealed class NetworkServer : IDisposable
    {
        public event Action<NetworkConnectionId>? ClientConnected;
        public event Action<NetworkConnectionId, string?>? ClientDisconnected;
        public event Action<NetworkConnectionId, ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;

        public bool IsRunning => _backend.IsRunning;

        private readonly INetworkServerBackend _backend;
        private bool _disposed;

        public NetworkServer(INetworkServerBackend backend)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));

            _backend.ClientConnected += Backend_ClientConnected;
            _backend.ClientDisconnected += Backend_ClientDisconnected;
            _backend.MessageReceived += Backend_MessageReceived;
        }

        public void Start()
        {
            ThrowIfDisposed();

            _backend.Start();
        }

        public void Poll()
        {
            ThrowIfDisposed();

            _backend.Poll();
        }

        public void Stop()
        {
            ThrowIfDisposed();

            _backend.Stop();
        }

        public void SendMessageToClient(NetworkConnectionId client, ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod = NetworkDeliveryMethod.Reliable)
        {
            ThrowIfDisposed();

            _backend.SendMessageToClient(client, message, deliveryMethod);
        }

        public void BroadcastMessage(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod = NetworkDeliveryMethod.Reliable)
        {
            ThrowIfDisposed();

            _backend.BroadcastMessage(message, deliveryMethod);
        }

        public void DisconnectClient(NetworkConnectionId client)
        {
            ThrowIfDisposed();

            _backend.DisconnectClient(client);
        }

        private void Backend_ClientConnected(NetworkConnectionId client)
        {
            ClientConnected?.Invoke(client);
        }

        private void Backend_ClientDisconnected(NetworkConnectionId client, string? reason)
        {
            ClientDisconnected?.Invoke(client, reason);
        }

        private void Backend_MessageReceived(NetworkConnectionId client, ReadOnlyMemory<byte> message, NetworkDeliveryMethod deliveryMethod)
        {
            MessageReceived?.Invoke(client, message, deliveryMethod);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _backend.ClientConnected -= Backend_ClientConnected;
            _backend.ClientDisconnected -= Backend_ClientDisconnected;
            _backend.MessageReceived -= Backend_MessageReceived;

            _backend.Dispose();
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }
}
