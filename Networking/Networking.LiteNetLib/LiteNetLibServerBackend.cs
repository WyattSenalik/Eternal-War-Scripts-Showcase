// Original Authors - Wyatt Senalik
using LiteNetLib;
using Networking.Core;
using System;
using System.Collections.Generic;

namespace Networking.LiteNetLib
{
    public sealed class LiteNetLibServerBackend : INetworkServerBackend
    {
        public event Action<NetworkConnectionId>? ClientConnected;
        public event Action<NetworkConnectionId, string?>? ClientDisconnected;
        public event Action<NetworkConnectionId, ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;

        public bool IsRunning => _manager is not null;

        private readonly Dictionary<int, NetworkConnectionId> _connectionIdsByPeerId = new();
        private readonly Dictionary<NetworkConnectionId, NetPeer> _peersByConnectionId = new();
        private readonly EventBasedNetListener _listener;
        private readonly int _port;
        private readonly string _connectionKey;

        private NetManager? _manager;
        private ulong _nextConnectionId = 1;

        public LiteNetLibServerBackend(int port, string connectionKey)
        {
            if (port <= 0 || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));

            _port = port;
            _connectionKey = connectionKey ?? string.Empty;

            _listener = new EventBasedNetListener();
            _listener.ConnectionRequestEvent += OnConnectionRequest;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;
            _listener.NetworkReceiveEvent += OnNetworkReceive;
        }

        public void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("The server is already running.");

            _manager = new NetManager(_listener);
            _manager.Start(_port);
        }

        public void Poll()
        {
            _manager?.PollEvents();
        }

        public void Stop()
        {
            if (_manager is null)
                return;

            _manager.Stop();
            _manager = null;

            _connectionIdsByPeerId.Clear();
            _peersByConnectionId.Clear();
        }

        public void SendMessageToClient(NetworkConnectionId client, ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
        {
            if (!_peersByConnectionId.TryGetValue(client, out NetPeer? peer))
                throw new InvalidOperationException($"No client exists with connection ID {client}.");

            peer.Send(message, ToDeliveryMethod(deliveryMethod));
        }

        public void BroadcastMessage(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
        {
            DeliveryMethod liteNetDeliveryMethod = ToDeliveryMethod(deliveryMethod);

            foreach (NetPeer peer in _peersByConnectionId.Values)
            {
                peer.Send(message, liteNetDeliveryMethod);
            }
        }

        public void DisconnectClient(NetworkConnectionId client)
        {
            if (!_peersByConnectionId.TryGetValue(client, out NetPeer? peer))
                return;

            peer.Disconnect();
        }

        public void Dispose()
        {
            Stop();
        }

        private void OnConnectionRequest(ConnectionRequest request)
        {
            if (_connectionKey.Length == 0)
            {
                request.Accept();
                return;
            }

            request.AcceptIfKey(_connectionKey);
        }

        private void OnPeerConnected(NetPeer peer)
        {
            NetworkConnectionId connectionId = new(_nextConnectionId++);

            _connectionIdsByPeerId.Add(peer.Id, connectionId);
            _peersByConnectionId.Add(connectionId, peer);

            ClientConnected?.Invoke(connectionId);
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (!_connectionIdsByPeerId.Remove(peer.Id, out NetworkConnectionId connectionId))
                return;

            _peersByConnectionId.Remove(connectionId);

            ClientDisconnected?.Invoke(connectionId, disconnectInfo.Reason.ToString());
        }

        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            if (!_connectionIdsByPeerId.TryGetValue(peer.Id, out NetworkConnectionId connectionId))
            {
                reader.Recycle();
                return;
            }

            byte[] message = reader.GetRemainingBytes();

            reader.Recycle();

            MessageReceived?.Invoke(connectionId, message, FromDeliveryMethod(deliveryMethod));
        }

        private static DeliveryMethod ToDeliveryMethod(
            NetworkDeliveryMethod deliveryMethod)
        {
            return deliveryMethod switch
            {
                NetworkDeliveryMethod.Reliable => DeliveryMethod.ReliableOrdered,
                NetworkDeliveryMethod.Unreliable => DeliveryMethod.Unreliable,
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod))
            };
        }

        private static NetworkDeliveryMethod FromDeliveryMethod(
            DeliveryMethod deliveryMethod)
        {
            return deliveryMethod switch
            {
                DeliveryMethod.Unreliable => NetworkDeliveryMethod.Unreliable,
                _ => NetworkDeliveryMethod.Reliable
            };
        }
    }
}
