// Original Authors - Jack Dekko
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Map.Actions;
using EternalWar.Core.Model;
using EternalWar.Core.Network;

namespace EternalWar.Client.Application.Run.Map.Controllers
{
    internal sealed class MapController(ClientGameSession clientSession) : IMapActions
    {
        private readonly ClientGameSession _clientSession = clientSession;

        public void VoteForNode(int nodeIndex)
        {
            ClientGameState clientGameState = _clientSession.GetFinalPresentationClientGameState();
            ClientInfo myClientInfo = clientGameState.GetMyClientInfo();

            NetworkGameCommandRequest_Run_Map_VoteForNode nodeVotingCommandRequest = new(myClientInfo, nodeIndex);
            _clientSession.ApplyLocalCommandRequestAndSendToServer(nodeVotingCommandRequest);
        }
    }
}
