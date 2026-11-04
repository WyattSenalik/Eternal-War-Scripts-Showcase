// Original Authors - Wyatt Senalik
using LiteNetLib;
using Networking.Core;
using System;

namespace Networking.LiteNetLib
{
    public sealed class LiteNetLibClientBackend : INetworkClientBackend
    {
        public event Action? Connected;
        public event Action<string?>? Disconnected;
        public event Action<ReadOnlyMemory<byte>, NetworkDeliveryMethod>? MessageReceived;

        public bool IsConnected { get; private set; }
        public bool IsConnecting { get; private set; }

        private readonly EventBasedNetListener _listener;
        private readonly string _host;
        private readonly int _port;
        private readonly string _connectionKey;

        private NetManager? _manager;
        private NetPeer? _serverPeer;

        public LiteNetLibClientBackend(string host, int port, string connectionKey)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _connectionKey = connectionKey ?? string.Empty;

            if (port <= 0 || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));

            _port = port;

            _listener = new EventBasedNetListener();

            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;
            _listener.NetworkReceiveEvent += OnNetworkReceive;
        }

        public void Connect()
        {
            if (_manager is not null)
                throw new InvalidOperationException("The client is already started.");

            _manager = new NetManager(_listener);
            _manager.Start();

            IsConnecting = true;

            _serverPeer = _manager.Connect(_host, _port, _connectionKey);
        }

        public void Poll()
        {
            _manager?.PollEvents();
        }

        public void Disconnect()
        {
            _serverPeer?.Disconnect();
            _serverPeer = null;

            IsConnected = false;
            IsConnecting = false;

            _manager?.Stop();
            _manager = null;
        }

        public void SendMessageToServer(ReadOnlySpan<byte> message, NetworkDeliveryMethod deliveryMethod)
        {
            if (_serverPeer is null || !IsConnected)
                throw new InvalidOperationException("The client is not connected.");

            _serverPeer.Send(message, ToDeliveryMethod(deliveryMethod));
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void OnPeerConnected(NetPeer peer)
        {
            _serverPeer = peer;

            IsConnecting = false;
            IsConnected = true;

            Connected?.Invoke();
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (_serverPeer == peer)
                _serverPeer = null;

            IsConnecting = false;
            IsConnected = false;

            Disconnected?.Invoke(disconnectInfo.Reason.ToString());
        }

        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            byte[] message = reader.GetRemainingBytes();

            reader.Recycle();

            MessageReceived?.Invoke(message, FromDeliveryMethod(deliveryMethod));
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
