// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Serialization;

namespace EternalWar.Client.Application.HighLevelGame.Actions
{
    internal interface IHighLevelGameActions
    {
        void NetworkGameCommandsApplied(GameDataHash remoteGameStateHash, DatedNetworkGameCommandBatch datedCommandBatch, ChangeRequestId? clientRequestedChangeId);
        void NetworkGameStateSnapshotReceived(DatedNetworkGameState stateSnapshot);
        void ChangeRequestDenied(ChangeRequestId changeRequestDenied);
    }
}
