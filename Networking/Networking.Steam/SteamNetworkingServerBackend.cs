// Original Authors - Wyatt Senalik
using Networking.Core;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Networking.Steam
{
    public sealed class SteamNetworkingServerBackend : INetworkServerBackend
    {
        public event Action<NetworkConnectionId>? ClientConnected;
        public event Action<NetworkConnectionId, string?>? ClientDisconnected;
        public event Action<NetworkConnectionId, ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;

        public bool IsRunning => _socketManager != null;

        private readonly Dictionary<uint, NetworkConnectionId> _connectionIds = new();
        private readonly Dictionary<NetworkConnectionId, Connection> _connections = new();
        private readonly int _virtualPort;

        private SteamServerSocketManager? _socketManager;
        private ulong _nextConnectionId = 1;

        public SteamNetworkingServerBackend(int virtualPort = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(virtualPort, nameof(virtualPort));

            _virtualPort = virtualPort;
        }

        public void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("The server is already running.");

            SteamNetworkingUtils.InitRelayNetworkAccess();

            _socketManager = SteamNetworkingSockets.CreateRelaySocket<SteamServerSocketManager>(_virtualPort);
            _socketManager.Backend = this;
        }

        public void Poll()
        {
            _socketManager?.Receive();
        }

        public void Stop()
        {
            if (_socketManager == null)
                return;

            _socketManager.Close();
            _socketManager = null;

            _connectionIds.Clear();
            _connections.Clear();
        }

        public void SendMessageToClient(NetworkConnectionId client, ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
        {
            if (!_connections.TryGetValue(client, out Connection connection))
                throw new InvalidOperationException($"No client exists with connection ID {client}.");

            connection.SendMessage(message.ToArray(), ToSendType(deliveryMethod));
        }

        public void BroadcastMessage(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
        {
            SendType sendType = ToSendType(deliveryMethod);
            byte[] data = message.ToArray();

            foreach (Connection connection in _connections.Values)
            {
                connection.SendMessage(data, sendType);
            }
        }

        public void DisconnectClient(NetworkConnectionId client)
        {
            if (_connections.TryGetValue(client, out Connection connection))
            {
                connection.Close();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Called by another class (contained)")]
        private void HandleConnecting(Connection connection, ConnectionInfo info)
        {
            connection.Accept();
        }

        private void HandleConnected(Connection connection, ConnectionInfo info)
        {
            NetworkConnectionId connectionId = new(_nextConnectionId++);

            _connectionIds[connection.Id] = connectionId;
            _connections[connectionId] = connection;

            ClientConnected?.Invoke(connectionId);
        }

        private void HandleDisconnected(Connection connection, ConnectionInfo info)
        {
            if (!_connectionIds.Remove(connection.Id, out NetworkConnectionId connectionId))
                return;

            _connections.Remove(connectionId);

            ClientDisconnected?.Invoke(connectionId, info.EndReason.ToString());
        }

        private void HandleMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            if (!_connectionIds.TryGetValue(connection.Id, out NetworkConnectionId connectionId))
                return;

            byte[] message = new byte[size];
            Marshal.Copy(data, message, 0, size);

            MessageReceived?.Invoke(connectionId, message, NetworkDeliveryMethod.Reliable);
        }

        private static SendType ToSendType(NetworkDeliveryMethod deliveryMethod)
        {
            return deliveryMethod switch
            {
                NetworkDeliveryMethod.Reliable => SendType.Reliable,
                NetworkDeliveryMethod.Unreliable => SendType.Unreliable,
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod))
            };
        }

        private sealed class SteamServerSocketManager : SocketManager
        {
            public SteamNetworkingServerBackend? Backend { get; set; }

            public override void OnConnecting(Connection connection, ConnectionInfo info)
            {
                Backend?.HandleConnecting(connection, info);
            }

            public override void OnConnected(Connection connection, ConnectionInfo info)
            {
                Backend?.HandleConnected(connection, info);
            }

            public override void OnDisconnected(Connection connection, ConnectionInfo info)
            {
                Backend?.HandleDisconnected(connection, info);
            }

            public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
            {
                Backend?.HandleMessage(connection, identity, data, size, messageNum, recvTime, channel);
            }
        }
    }
}
