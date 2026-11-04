// Original Authors - Wyatt Senalik
using EternalWar.Server.Application.Actions;
using EternalWar.Server.Application.Messages;

namespace EternalWar.Server.Application.Controllers
{
    internal sealed class GameController(IMessageSender messageSender, ServerGameSession session)
    {
        public IHighLevelNetworkActions HighLevelNetworkActions => _networkController;
        public IHighLevelGameActions HighLevelGameActions => _highLevelGameController;

        private readonly HighLevelNetworkController _networkController = new(messageSender, session);
        private readonly HighLevelGameController _highLevelGameController = new(messageSender, session);
    }
}