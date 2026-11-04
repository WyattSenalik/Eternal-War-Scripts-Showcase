// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Server.Application.Messages;
using EternalWar.Server.Application.NetworkStateChange.PerformPostHandlers;

namespace EternalWar.Server.Application
{
    internal sealed class ServerGameSession
    {
        public GameDatabase Database { get; }
        public DatedNetworkGameState DatedNetworkGameState { get; private set; } = DatedNetworkGameState.InitialState;
        public NetworkGameState NetworkGameState => DatedNetworkGameState.NetworkGameState;

        private readonly IMessageSender _messageSender;
        private readonly PerformPostCommandRequestHandlerRegistrator _postCommandRequestHandlerRegistrator;

        public ServerGameSession(IMessageSender messageSender, GameDatabase database)
        {
            _messageSender = messageSender;
            Database = database;
            _postCommandRequestHandlerRegistrator = new PerformPostCommandRequestHandlerRegistrator(this, database);
        }

        public DatedNetworkGameState ApplyRequestAndRelay(NetworkGameCommandRequest request, ChangeRequestId? clientRequestedChangeId)
        {
            ValueArray<NetworkGameCommand> commands = request.ResolveToCommands(Database, NetworkGameState);
            ApplyCommandsAndRelay(commands, clientRequestedChangeId);
            if (_postCommandRequestHandlerRegistrator.TryGetHandler(request, out IPerformPostCommandRequestHandler? handler))
                handler.PerformPostRequest(request);
            return DatedNetworkGameState;
        }

        public DatedNetworkGameState ApplyCommandsAndRelay(ValueArray<NetworkGameCommand> commands, ChangeRequestId? clientRequestedChangeId)
        {
            DatedNetworkGameState newDatedNetworkGameState = DatedNetworkGameState.ApplyCommandBatch(Database, new(DatedNetworkGameState.Sequence + 1, commands));
            DatedNetworkGameState = newDatedNetworkGameState;
            _messageSender.SendCommandBatchToAllClients(DatedNetworkGameState, commands, clientRequestedChangeId);
            return DatedNetworkGameState;
        }

        public DatedNetworkGameState ApplyCommandBatch(ValueArray<NetworkGameCommand> commands)
        {
            DatedNetworkGameState = DatedNetworkGameState.ApplyCommandBatch(Database, new(DatedNetworkGameState.Sequence + 1, commands));
            return DatedNetworkGameState;
        }

        public DatedNetworkGameState ApplyCommand(NetworkGameCommand command) => ApplyCommandBatch(new ValueArray<NetworkGameCommand>(new[] { command }));
    }
}
