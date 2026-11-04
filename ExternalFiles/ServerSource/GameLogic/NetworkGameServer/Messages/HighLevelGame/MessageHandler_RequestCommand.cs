// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages.HighLevelGame;
using EternalWar.Server.Application.Actions;
using EternalWar.Server.Application.Messages;
using System;

namespace EternalWar.Server.Application.Messages.HighLevelGame
{
    internal sealed class MessageHandler_RequestCommand(IHighLevelGameActions highLevelGameActions) : MessageHandler<TypedMessageData_RequestCommand>
    {
        private readonly IHighLevelGameActions _highLevelGameActions = highLevelGameActions;

        protected override void Handle(Guid clientGuid, TypedMessageData_RequestCommand messageData)
        {
            _highLevelGameActions.ClientRequestCommand(clientGuid, messageData.RequestedCommand);
        }
    }
}
