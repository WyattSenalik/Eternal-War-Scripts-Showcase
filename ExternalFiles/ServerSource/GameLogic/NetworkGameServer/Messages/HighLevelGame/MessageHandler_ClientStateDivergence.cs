// Original Authors - Wyatt Senalik
using EternalWar.Server.Application.Actions;
using System;

namespace EternalWar.Server.Application.Messages.HighLevelGame
{
    internal sealed class MessageHandler_ClientStateDivergence(IHighLevelGameActions highLevelGameActions) : IMessageHandler
    {
        private readonly IHighLevelGameActions _highLevelGameActions = highLevelGameActions;

        public void Handle(TypedMessageWithClient typedClientMessage)
        {
            Guid clientGuid = typedClientMessage.ClientId;
            _highLevelGameActions.ClientStateDiverged(clientGuid);
        }
    }
}