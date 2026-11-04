// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;

namespace EternalWar.Core.Network.Messages.HighLevelGame
{
    public sealed record TypedMessageData_NetworkGameStateSnapshot(DatedNetworkGameState State);
}
