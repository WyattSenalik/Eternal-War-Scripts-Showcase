// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Network.Messages;
using EternalWar.Core.Network.Messages.HighLevelGame;
using Networking.Client;
using Networking.Core;
using Networking.LiteNetLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EternalWar.Client.Application.Network
{
    internal static class NetworkHelpers
    {
        public static bool IsConnected => _client?.IsConnected ?? false;
        public static bool IsConnecting => _client?.IsConnecting ?? false;

        private static readonly Queue<string> _incomingMessages = new();
        private static NetworkClient? _client;

        public static bool TryValidateAddressAndPort(string ipAddressString, string portString, out ushort port, out string errorMessage)
        {
            if (!IPAddress.TryParse(ipAddressString, out _))
            {
                port = 0;
                errorMessage = $"Argument 1 ({ipAddressString}) is not a valid IP address.";
                return false;
            }

            if (!ushort.TryParse(portString, out port) || port == 0)
            {
                errorMessage = $"Argument 2 ({portString}) is not a valid port.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public static bool ConnectToServer(string ipAddressString, string portString, out string errorMessage)
        {
            if (!TryValidateAddressAndPort(ipAddressString, portString, out ushort port, out errorMessage))
                return false;

            DisconnectFromServer();
            LiteNetLibClientBackend backend = new(ipAddressString, port, connectionKey: string.Empty);
            NetworkClient client = new(backend);
            client.MessageReceived += OnMessageReceived;

            try
            {
                client.Connect();
            }
            catch (Exception exception)
            {
                client.MessageReceived -= OnMessageReceived;
                client.Dispose();
                errorMessage = $"Failed to start a connection to {ipAddressString}:{port}: {exception.Message}";
                return false;
            }

            _client = client;
            errorMessage = string.Empty;
            return true;
        }

        public static void DisconnectFromServer()
        {
            if (_client is not null)
            {
                _client.MessageReceived -= OnMessageReceived;
                _client.Dispose();
                _client = null;
            }

            _incomingMessages.Clear();
        }

        public static void Poll()
        {
            _client?.Poll();
        }

        public static bool TryDequeueIncomingMessage(out string? message)
        {
            if (_incomingMessages.TryDequeue(out string? nextMessage))
            {
                message = nextMessage;
                return true;
            }

            message = null;
            return false;
        }

        public static bool SendMessageToServer(string messageType, object? messageData = null)
        {
            TypedMessage message = new(messageType, messageData);
            return SendMessageToServer(message.Serialize());
        }

        private static bool SendMessageToServer(string serializedMessage)
        {
            if (_client is null || !_client.IsConnected)
                return false;

            try
            {
                byte[] payload = Encoding.UTF8.GetBytes(serializedMessage);
                _client.SendMessageToServer(payload, NetworkDeliveryMethod.Reliable);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static bool SendRequestCommandMessageToServer(ChangeRequest requestedCommand)
        {
            TypedMessageData_RequestCommand requestCommandMessageData = new(requestedCommand);
            return SendMessageToServer(NetworkMessageTypeNames.REQUEST_COMMAND, requestCommandMessageData);
        }

        private static void OnMessageReceived(ReadOnlyMemory<byte> payload, NetworkDeliveryMethod deliveryMethod)
        {
            _incomingMessages.Enqueue(Encoding.UTF8.GetString(payload.Span));
        }
    }
}
