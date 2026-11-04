// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelGame.Actions;
using EternalWar.Core.Model;
using EternalWar.Core.Network.Messages.HighLevelGame;

namespace EternalWar.Client.Application.Network.Messages.HighLevelGame
{
    internal sealed class MessageHandler_ChangeRequestDenied : MessageHandler<TypedMessageData_ChangeRequestDenied>
    {
        private readonly IHighLevelGameActions _highLevelGameActions;

        public MessageHandler_ChangeRequestDenied(IHighLevelGameActions highLevelGameActions)
        {
            _highLevelGameActions = highLevelGameActions;
        }

        protected override void Handle(TypedMessageData_ChangeRequestDenied data)
        {
            ChangeRequestId localChangeId = data.ChangeId;
            _highLevelGameActions.ChangeRequestDenied(localChangeId);
        }
    }
}
