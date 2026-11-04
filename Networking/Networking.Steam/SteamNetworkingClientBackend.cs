// Original Authors - Wyatt Senalik
using Networking.Core;
using Steamworks;
using Steamworks.Data;
using System;
using System.Runtime.InteropServices;

namespace Networking.Steam
{
    public sealed class SteamNetworkingClientBackend : INetworkClientBackend
    {
        public event Action? Connected;
        public event Action<string?>? Disconnected;
        public event Action<ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;

        public bool IsConnected { get; private set; }
        public bool IsConnecting { get; private set; }

        private readonly SteamId _serverSteamId;
        private readonly int _virtualPort;

        private SteamClientConnectionManager? _connectionManager;

        public SteamNetworkingClientBackend(SteamId serverSteamId, int virtualPort = 0)
        {
            if (serverSteamId == 0)
                throw new ArgumentException("Server Steam ID cannot be zero.", nameof(serverSteamId));

            ArgumentOutOfRangeException.ThrowIfNegative(virtualPort, nameof(virtualPort));

            _serverSteamId = serverSteamId;
            _virtualPort = virtualPort;
        }

        public void Connect()
        {
            if (_connectionManager != null)
                throw new InvalidOperationException("The client is already connected or connecting.");

            SteamNetworkingUtils.InitRelayNetworkAccess();

            IsConnecting = true;

            _connectionManager = SteamNetworkingSockets.ConnectRelay<SteamClientConnectionManager>(_serverSteamId, _virtualPort);
            _connectionManager.Backend = this;
        }

        public void Poll()
        {
            _connectionManager?.Receive();
        }

        public void Disconnect()
        {
            if (_connectionManager == null)
                return;

            _connectionManager.Close();
            _connectionManager = null;

            IsConnected = false;
            IsConnecting = false;
        }

        public void SendMessageToServer(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
        {
            if (_connectionManager == null || !IsConnected)
                throw new InvalidOperationException("The client is not connected.");

            _connectionManager.Connection.SendMessage(message.ToArray(), ToSendType(deliveryMethod));
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void HandleConnected(ConnectionInfo info)
        {
            IsConnecting = false;
            IsConnected = true;

            Connected?.Invoke();
        }

        private void HandleDisconnected(ConnectionInfo info)
        {
            IsConnecting = false;
            IsConnected = false;

            Disconnected?.Invoke(info.EndReason.ToString());

            _connectionManager = null;
        }

        private void HandleMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] message = new byte[size];

            Marshal.Copy(data, message, 0, size);

            MessageReceived?.Invoke(message, NetworkDeliveryMethod.Reliable);
        }

        private static SendType ToSendType(
            NetworkDeliveryMethod deliveryMethod)
        {
            return deliveryMethod switch
            {
                NetworkDeliveryMethod.Reliable => SendType.Reliable,
                NetworkDeliveryMethod.Unreliable => SendType.Unreliable,
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod))
            };
        }

        private sealed class SteamClientConnectionManager : ConnectionManager
        {
            public SteamNetworkingClientBackend? Backend { get; set; }

            public override void OnConnected(ConnectionInfo info)
            {
                Backend?.HandleConnected(info);
            }

            public override void OnDisconnected(ConnectionInfo info)
            {
                Backend?.HandleDisconnected(info);
            }

            public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
            {
                Backend?.HandleMessage(data, size, messageNum, recvTime, channel);
            }
        }
    }
}
