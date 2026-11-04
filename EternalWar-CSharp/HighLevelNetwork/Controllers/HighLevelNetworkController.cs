// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelNetwork.Actions;
using EternalWar.Client.Application.Model;
using EternalWar.Core.Model;
using EternalWar.Core.Network;

namespace EternalWar.Client.Application.HighLevelNetwork.Controllers
{
    internal sealed class HighLevelNetworkController(ClientGameSession clientSession) : IHighLevelNetworkActions
    {
        private readonly ClientGameSession _clientSession = clientSession;

        public void SelfClientCurrentlyConnecting()
        {
            _clientSession.SetConnecting();
        }

        public void SelfClientConnected(ClientInfo myClientInfo, NetworkGameState networkGameState, long sequenceId)
        {
            _clientSession.SetConnected(myClientInfo, networkGameState, sequenceId);
        }

        public void SelfClientDisconnected()
        {
            _clientSession.SetDisconnected();
        }
    }
}
