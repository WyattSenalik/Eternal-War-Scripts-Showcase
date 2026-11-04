// Original Authors - Wyatt Senalik
using EternalWar.Server.Application.Networking;
using System;
using System.Collections.Generic;

namespace EternalWar.Server.Application.Messages
{
    internal sealed class SeamMessageSender : IMessageSender
    {
        private readonly Queue<(Guid ClientId, string Message)> _pendingMessages = new();
        private readonly object _lock = new();
        private NetworkGameServerTransport? _transport;

        public void Initialize(NetworkGameServerTransport transport)
        {
            // Use local variables to capture on multi-threads
            Queue<(Guid ClientId, string Message)> pendingMessages = _pendingMessages;

            lock (_lock)
            {
                _transport = transport;
                pendingMessages = new(_pendingMessages);
                _pendingMessages.Clear();
            }

            while (pendingMessages.TryDequeue(out (Guid ClientId, string Message) pendingMessage))
            {
                transport.SendMessageToClient(pendingMessage.ClientId, pendingMessage.Message);
            }
        }

        public void SendMessageToClient(Guid clientId, string message)
        {
            // Use local variables to capture on multi-threads
            NetworkGameServerTransport? transport;
            
            lock (_lock)
            {
                transport = _transport;

                if (transport is null)
                {
                    _pendingMessages.Enqueue((clientId, message));
                    return;
                }
            }

            transport.SendMessageToClient(clientId, message);
        }
    }
}
