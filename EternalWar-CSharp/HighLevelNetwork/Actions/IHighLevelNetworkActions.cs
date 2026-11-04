// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Network;

namespace EternalWar.Client.Application.HighLevelNetwork.Actions
{
    internal interface IHighLevelNetworkActions
    {
        void SelfClientCurrentlyConnecting();
        void SelfClientConnected(ClientInfo myClientInfo, NetworkGameState networkGameState, long sequenceId);
        void SelfClientDisconnected();
    }
}
