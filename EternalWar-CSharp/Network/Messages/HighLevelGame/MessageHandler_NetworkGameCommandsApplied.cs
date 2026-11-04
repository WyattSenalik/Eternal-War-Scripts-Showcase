// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelGame.Actions;
using EternalWar.Core.Model;
using EternalWar.Core.Network.Messages.HighLevelGame;
using EternalWar.Core.Serialization;

namespace EternalWar.Client.Application.Network.Messages.HighLevelGame
{
    internal sealed class MessageHandler_NetworkGameCommandsApplied(IHighLevelGameActions gameActions) : MessageHandler<TypedMessageData_NetworkGameCommandsApplied>
    {
        private readonly IHighLevelGameActions _gameActions = gameActions;

        protected override void Handle(TypedMessageData_NetworkGameCommandsApplied data)
        {
            data.Deconstruct(out GameDataHash networkGameStateHash, out DatedNetworkGameCommandBatch datedCommandBatch, out ChangeRequestId? clientRequestedChangeId);
            _gameActions.NetworkGameCommandsApplied(networkGameStateHash, datedCommandBatch, clientRequestedChangeId);
        }
    }
}
