// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using Networking.Core;
using Networking.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EternalWar.Server.Application.Networking
{
    internal sealed class NetworkGameServerTransport : IDisposable
    {
        internal readonly record struct ReceivedMessage(string Message, Guid ClientId);

        public event Action<Guid>? ClientConnected;
        public event Action<Guid>? ClientDisconnected;

        private readonly Dictionary<NetworkConnectionId, Guid> _clientIdsByConnection = new();
        private readonly Dictionary<Guid, NetworkConnectionId> _connectionsByClientId = new();
        private readonly ConcurrentQueue<ReceivedMessage> _incomingMessages = new();
        private readonly NetworkServer _server;
        private readonly int _maximumClients;

        public NetworkGameServerTransport(NetworkServer server, int maximumClients)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumClients, nameof(maximumClients));
            _maximumClients = maximumClients;

            _server.ClientConnected += OnClientConnected;
            _server.ClientDisconnected += OnClientDisconnected;
            _server.MessageReceived += OnMessageReceived;
        }

        public void Start() => _server.Start();

        public void Poll() => _server.Poll();

        public bool TryDequeueIncomingMessage(out ReceivedMessage message) => _incomingMessages.TryDequeue(out message);

        public void SendMessageToClient(Guid clientId, string message)
        {
            if (!_connectionsByClientId.TryGetValue(clientId, out NetworkConnectionId connectionId))
            {
                Debug.PrintError($"Cannot send a message to disconnected client ({clientId}).");
                return;
            }

            try
            {
                byte[] payload = Encoding.UTF8.GetBytes(message);
                _server.SendMessageToClient(connectionId, payload, NetworkDeliveryMethod.Reliable);
            }
            catch (InvalidOperationException exception)
            {
                Debug.PrintError($"Failed to send a message to client ({clientId}): {exception.Message}");
            }
        }

        public void Dispose()
        {
            _server.ClientConnected -= OnClientConnected;
            _server.ClientDisconnected -= OnClientDisconnected;
            _server.MessageReceived -= OnMessageReceived;
            _server.Dispose();
        }

        private void OnClientConnected(NetworkConnectionId connectionId)
        {
            if (_connectionsByClientId.Count >= _maximumClients)
            {
                _server.DisconnectClient(connectionId);
                return;
            }

            Guid clientId = Guid.NewGuid();
            _clientIdsByConnection.Add(connectionId, clientId);
            _connectionsByClientId.Add(clientId, connectionId);
            ClientConnected?.Invoke(clientId);
        }

        private void OnClientDisconnected(NetworkConnectionId connectionId, string? reason)
        {
            _ = reason;
            if (!_clientIdsByConnection.Remove(connectionId, out Guid clientId))
                return;

            _connectionsByClientId.Remove(clientId);
            ClientDisconnected?.Invoke(clientId);
        }

        private void OnMessageReceived(NetworkConnectionId connectionId, ReadOnlyMemory<byte> payload, NetworkDeliveryMethod deliveryMethod)
        {
            _ = deliveryMethod;
            if (!_clientIdsByConnection.TryGetValue(connectionId, out Guid clientId))
                return;

            _incomingMessages.Enqueue(new ReceivedMessage(Encoding.UTF8.GetString(payload.Span), clientId));
        }
    }
}
