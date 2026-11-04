// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelNetwork.Actions;
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using EternalWar.Core.Network.Messages.HighLevelNetwork;

namespace EternalWar.Client.Application.Network.Messages.HighLevelNetwork
{
    internal sealed class MessageHandler_SelfClientConnected(IHighLevelNetworkActions networkActions) : MessageHandler<TypedMessageData_SelfClientConnected>
    {
        private readonly IHighLevelNetworkActions _networkActions = networkActions;

        protected override void Handle(TypedMessageData_SelfClientConnected data)
        {
            ClientInfo myClientInfo = data.SelfInfo;
            NetworkGameState networkGameState = data.State;
            long sequenceId = data.SequenceId;

            _networkActions.SelfClientConnected(myClientInfo, networkGameState, sequenceId);
        }
    }
}
