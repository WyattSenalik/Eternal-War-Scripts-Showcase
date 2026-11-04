// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Serialization;

namespace EternalWar.Core.Network.Messages.HighLevelGame
{
    public sealed record TypedMessageData_NetworkGameCommandsApplied(GameDataHash NetworkGameStateHash, DatedNetworkGameCommandBatch DatedCommandBatch, ChangeRequestId? ClientRequestedChangeId);
}
