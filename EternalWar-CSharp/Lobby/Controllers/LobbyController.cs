// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelNetwork.Actions;
using EternalWar.Client.Application.Lobby.Actions;
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Network;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using EternalWar.Server.Application;
using System;
using System.Threading.Tasks;

namespace EternalWar.Client.Application.Lobby.Controllers
{
    internal sealed class LobbyController(ClientGameSession clientSession, IHighLevelNetworkActions highLevelNetworkActions, string dataFolderAbsolutePath) : ILobbyActions, IDisposable
    {
        private const int MAX_CONNECTIONS = 4;

        private readonly ClientGameSession _clientSession = clientSession;
        private readonly IHighLevelNetworkActions _highLevelNetworkActions = highLevelNetworkActions;
        private readonly string _dataFolderAbsolutePath = dataFolderAbsolutePath;
        private ServerGameApplication? _serverApplication;

        public void HostServer(string ipAddressString, string portString)
        {
            DisposeOfServer();

            if (!NetworkHelpers.TryValidateAddressAndPort(ipAddressString, portString, out ushort port, out string errorMessage))
            {
                _highLevelNetworkActions.SelfClientCurrentlyConnecting();
                ReportConnectionFailure(errorMessage);
                return;
            }

            try
            {
                // Capture the variable locally to avoid it mutating before the task is scheduled
                ServerGameApplication serverApplication = new(_dataFolderAbsolutePath, port, MAX_CONNECTIONS);
                _serverApplication = serverApplication;
                _ = Task.Run(() => serverApplication.RunAsync())
                    .ContinueWith(failedTask =>
                    {
                        Exception serverException = failedTask.Exception?.Flatten().InnerException ?? failedTask.Exception ?? new Exception("Unknown failure from server");

                        _highLevelNetworkActions.SelfClientCurrentlyConnecting();
                        ReportConnectionFailure($"Server ran into an exception:\n{serverException}");
                        DisposeOfServer();
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception exception)
            {
                _highLevelNetworkActions.SelfClientCurrentlyConnecting();
                ReportConnectionFailure($"Failed to start the server: {exception}");
                DisposeOfServer();
                return;
            }

            if (!TryJoinServer(ipAddressString, portString))
                DisposeOfServer();
        }

        public void JoinServer(string ipAddressString, string portString)
        {
            TryJoinServer(ipAddressString, portString);
        }

        private bool TryJoinServer(string ipAddressString, string portString)
        {
            _highLevelNetworkActions.SelfClientCurrentlyConnecting();
            if (!NetworkHelpers.ConnectToServer(ipAddressString, portString, out string errorMessage))
            {
                ReportConnectionFailure(errorMessage);
                return false;
            }
            Debug.Print("Connecting to the server...");
            return true;
        }

        private void ReportConnectionFailure(string errorMessage)
        {
            _highLevelNetworkActions.SelfClientDisconnected();
            Debug.PrintError(errorMessage);
        }

        public void ToggleReady()
        {
            ClientGameState clientGameState = _clientSession.GetFinalPresentationClientGameState();
            ClientInfo myClientInfo = clientGameState.GetMyClientInfo();

            NetworkGameCommandRequest_Lobby_ToggleReady toggleReadyRequest = new(myClientInfo);
            _clientSession.ApplyLocalCommandRequestAndSendToServer(toggleReadyRequest);
        }

        public void Disconnect()
        {
            _clientSession.SetDisconnected();
            NetworkHelpers.DisconnectFromServer();
            DisposeOfServer();
        }

        public void Dispose()
        {
            DisposeOfServer();
        }

        private void DisposeOfServer()
        {
            _serverApplication?.Dispose();
            _serverApplication = null;
        }
    }
}

