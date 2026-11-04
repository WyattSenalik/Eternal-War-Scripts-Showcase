// Original Authors - Wyatt Senalik
using Networking.Core;
using System;

namespace Networking.Client
{
    public sealed class NetworkClient : IDisposable
    {
        public event Action? Connected;
        public event Action<string?>? Disconnected;
        public event Action<ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;

        public bool IsConnected => _backend.IsConnected;
        public bool IsConnecting => _backend.IsConnecting;

        private readonly INetworkClientBackend _backend;
        private bool _disposed;

        public NetworkClient(INetworkClientBackend backend)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));

            _backend.Connected += Backend_Connected;
            _backend.Disconnected += Backend_Disconnected;
            _backend.MessageReceived += Backend_MessageReceived;
        }

        public void Connect()
        {
            ThrowIfDisposed();

            _backend.Connect();
        }

        public void Poll()
        {
            ThrowIfDisposed();

            _backend.Poll();
        }

        public void Disconnect()
        {
            ThrowIfDisposed();

            _backend.Disconnect();
        }

        public void SendMessageToServer(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod = NetworkDeliveryMethod.Reliable)
        {
            ThrowIfDisposed();

            _backend.SendMessageToServer(message, deliveryMethod);
        }

        private void Backend_Connected()
        {
            Connected?.Invoke();
        }

        private void Backend_Disconnected(string? reason)
        {
            Disconnected?.Invoke(reason);
        }

        private void Backend_MessageReceived(ReadOnlyMemory<byte> message, NetworkDeliveryMethod deliveryMethod)
        {
            MessageReceived?.Invoke(message, deliveryMethod);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _backend.Connected -= Backend_Connected;
            _backend.Disconnected -= Backend_Disconnected;
            _backend.MessageReceived -= Backend_MessageReceived;

            _backend.Dispose();
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }
}
