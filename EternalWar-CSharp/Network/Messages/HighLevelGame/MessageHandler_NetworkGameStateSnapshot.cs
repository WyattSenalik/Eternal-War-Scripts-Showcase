// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelGame.Actions;
using EternalWar.Core.Network.Messages.HighLevelGame;

namespace EternalWar.Client.Application.Network.Messages.HighLevelGame
{
    internal sealed class MessageHandler_NetworkGameStateSnapshot(IHighLevelGameActions gameActions) : MessageHandler<TypedMessageData_NetworkGameStateSnapshot>
    {
        private readonly IHighLevelGameActions _gameActions = gameActions;

        protected override void Handle(TypedMessageData_NetworkGameStateSnapshot data) => _gameActions.NetworkGameStateSnapshotReceived(data.State);
    }
}
