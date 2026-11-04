// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Network.Messages;
using EternalWar.Core.Network.Messages.HighLevelGame;
using EternalWar.Server.Application.Actions;
using EternalWar.Server.Application.Messages;
using EternalWar.Server.Application.NetworkStateChange.AllowHandlers;
using System;

namespace EternalWar.Server.Application.Controllers
{
    internal sealed class HighLevelGameController(IMessageSender messageSender, ServerGameSession session) : IHighLevelGameActions
    {
        private readonly IMessageSender _messageSender = messageSender;
        private readonly ServerGameSession _session = session;

        private readonly AllowChangeHandlerRegistrator _allowChangeHandlerRegistrator = new();

        public void ClientStateDiverged(Guid clientGuid)
        {
            // Fix client by overwriting their state with the server's.
            DatedNetworkGameState datedNetworkGameState = _session.DatedNetworkGameState;
            _messageSender.SendNetworkGameStateSnapshotToAllClients(datedNetworkGameState);
        }

        public void ClientRequestCommand(Guid requestingClientId, ChangeRequest requestedCommand)
        {
            GameDatabase database = _session.Database;
            NetworkGameState currentNetworkGameState = _session.NetworkGameState;

            NetworkGameCommandRequest commandRequest = requestedCommand.Request;
            ChangeRequestId clientChangeId = requestedCommand.LocalChangeId ?? throw new Exception($"A command request to the server has a null local request id: ({commandRequest})");

            if (clientChangeId.RequesterClientGuid != requestingClientId)
            {
                TypedMessageData_ChangeRequestDenied deniedMessageData = new(clientChangeId);
                _messageSender.SendMessageToClient(requestingClientId, NetworkMessageTypeNames.CHANGE_REQUEST_DENIED, deniedMessageData);
                return;
            }

            IAllowChangeHandler allowHandler = _allowChangeHandlerRegistrator.GetHandler(commandRequest);
            if (!allowHandler.ShouldAllowChange(database, currentNetworkGameState, requestingClientId, commandRequest))
            {
                // Should not allow the change, deny it.
                TypedMessageData_ChangeRequestDenied deniedMessageData = new(clientChangeId);
                _messageSender.SendMessageToClient(requestingClientId, NetworkMessageTypeNames.CHANGE_REQUEST_DENIED, deniedMessageData);
                return;
            }
            _session.ApplyRequestAndRelay(commandRequest, clientChangeId);
        }
    }
}

