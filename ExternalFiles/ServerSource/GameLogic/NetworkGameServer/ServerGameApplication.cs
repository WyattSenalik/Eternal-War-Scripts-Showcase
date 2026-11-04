// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Server.Application.Actions;
using EternalWar.Server.Application.Controllers;
using EternalWar.Server.Application.Messages;
using EternalWar.Server.Application.Networking;
using Networking.LiteNetLib;
using Networking.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EternalWar.Server.Application
{
    public sealed class ServerGameApplication : IDisposable
    {
        private const int TICK_LENGTH_MILLISECONDS = 14;

        private readonly MessageListener _messageListener = new();
        private readonly NetworkGameServerTransport _transport;
        private readonly GameController _gameController;
        private readonly Queue<(Guid clientGuid, bool wasConnecting)> _pendingConnectionMessages = new();
        private readonly object _transportLock = new();
        private int _hasRunStarted;
        private int _stopRequested;
        private int _wasDisposed;

        public ServerGameApplication(string absolutePathToDataFolder, int port, int maxConnections)
        {
            // Load database
            GameDatabase gameDatabase = DatabaseLoader.LoadDatabase(absolutePathToDataFolder, sanityCheck: true);

            // Create the message sender & session
            SeamMessageSender messageSender = new();
            ServerGameSession networkGameSession = new(messageSender, gameDatabase);

            // Create the controllers
            _gameController = new GameController(messageSender, networkGameSession);
            IHighLevelGameActions highLevelGameActions = _gameController.HighLevelGameActions;

            // Create the transport
            LiteNetLibServerBackend backend = new(port, connectionKey: string.Empty);
            NetworkServer server = new(backend);
            _transport = new NetworkGameServerTransport(server, maxConnections);
            _transport.ClientConnected += OnClientConnected;
            _transport.ClientDisconnected += OnClientDisconnected;

            // Finish initialization
            messageSender.Initialize(_transport);

            // Register to listen for messages
            MessageHandlerRegistration.Setup(messageSender, highLevelGameActions);

            // Start the server
            _transport.Start();
        }

        public async Task RunAsync(Action? tickedCallback = null)
        {
            if (Volatile.Read(ref _wasDisposed) != 0 || Volatile.Read(ref _stopRequested) != 0 || Interlocked.Exchange(ref _hasRunStarted, 1) != 0)
                return;

            while (Volatile.Read(ref _stopRequested) == 0 && Volatile.Read(ref _wasDisposed) == 0)
            {
                long tickStart = Stopwatch.GetTimestamp();

                lock (_transportLock)
                {
                    if (Volatile.Read(ref _stopRequested) != 0 || Volatile.Read(ref _wasDisposed) != 0)
                        break;

                    _transport.Poll();
                    while (_transport.TryDequeueIncomingMessage(out NetworkGameServerTransport.ReceivedMessage message))
                    {
                        _messageListener.HandleMessage(message);
                    }
                }

                TimeSpan elapsedTime = Stopwatch.GetElapsedTime(tickStart);
                long elapsedMilliseconds = (long)elapsedTime.TotalMilliseconds;
                if (elapsedMilliseconds < TICK_LENGTH_MILLISECONDS)
                    await Task.Delay(TICK_LENGTH_MILLISECONDS - (int)elapsedMilliseconds).ConfigureAwait(false);

                if (Volatile.Read(ref _stopRequested) != 0 || Volatile.Read(ref _wasDisposed) != 0)
                    break;

                tickedCallback?.Invoke();
            }
        }

        public void Stop()
        {
            Interlocked.Exchange(ref _stopRequested, 1);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _wasDisposed, 1) != 0)
                return;

            Interlocked.Exchange(ref _stopRequested, 1);
            lock (_transportLock)
            {
                MessageHandlerRegistration.Cleanup();
                _transport.Dispose();
            }
        }

        private void OnClientConnected(Guid clientGuid)
        {
            if (_gameController == null)
            {
                _pendingConnectionMessages.Enqueue(new(clientGuid, true));
                return;
            }

            IHighLevelNetworkActions highLevelNetworkActions = _gameController.HighLevelNetworkActions;
            highLevelNetworkActions.ClientConnected(clientGuid);
        }

        private void OnClientDisconnected(Guid disconnectedClientId)
        {
            if (_gameController == null)
            {
                _pendingConnectionMessages.Enqueue(new(disconnectedClientId, false));
                return;
            }

            IHighLevelNetworkActions highLevelNetworkActions = _gameController.HighLevelNetworkActions;
            highLevelNetworkActions.ClientDisconnected(disconnectedClientId);
        }
    }
}
