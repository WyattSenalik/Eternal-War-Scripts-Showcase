// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelGame.Actions;
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Network;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using EternalWar.Core.Network.Messages;
using EternalWar.Core.Serialization;
using System;

namespace EternalWar.Client.Application.HighLevelGame.Controllers
{
    internal sealed class HighLevelGameController(ClientGameSession clientSession) : IHighLevelGameActions
    {
        private readonly ClientGameSession _clientSession = clientSession;

        public void NetworkGameCommandsApplied(GameDataHash remoteNetworkGameStateHash, DatedNetworkGameCommandBatch datedCommandBatch, ChangeRequestId? clientRequestedChangeId)
        {
            ClientGameState oldAuthoritativeClientGameState = _clientSession.GetAuthoritativeClientGameState();

            ApplyAuthoritativeCommandBatchResult result = _clientSession.ApplyAuthoritativeCommandBatch(datedCommandBatch, clientRequestedChangeId);
            bool requiresReconciliation = false;
            switch (result)
            {
                // Can't check for divergence if we've only saved it for future processing.
                case ApplyAuthoritativeCommandBatchResult.SavedForFutureProcessing:
                    return;

                case ApplyAuthoritativeCommandBatchResult.Success:
                    break;
                case ApplyAuthoritativeCommandBatchResult.AlreadyProcessed:
                    break;

                case ApplyAuthoritativeCommandBatchResult.RequiresReconciliation:
                    requiresReconciliation = true;
                    break;

                default:
                    throw new NotImplementedException($"Unhandled {nameof(ApplyAuthoritativeCommandBatchResult)}: ({result})");
            }

            if (!requiresReconciliation)
            {
                // Applying didn't point out that we needed reconciliation, but that doesn't mean something else didn't cause it.
                DatedNetworkGameState newDatedAuthoritativeNetworkGameState = _clientSession.AuthoritativeGameState;
                NetworkGameState newAuthoritativeNetworkGameState = newDatedAuthoritativeNetworkGameState.NetworkGameState;
                GameDataHash localNetworkGameStateHash = GameDataHashing.Hash(newAuthoritativeNetworkGameState);

                if (localNetworkGameStateHash == remoteNetworkGameStateHash)
                    return;
            }

            ClientGameState newAuthoritativeClientGameState = _clientSession.GetAuthoritativeClientGameState();
            ClientInfo myClientInfo = newAuthoritativeClientGameState.GetMyClientInfo();
            Debug.PrintError($"[{myClientInfo}] State Divergence from server command batch ({datedCommandBatch.Sequence}). Detected from {(result == ApplyAuthoritativeCommandBatchResult.RequiresReconciliation ? "ApplyResult" : "Hashing")}");

            // TODO: Might need to check if divergence was from reconciliation attempt.

            Debug.PrintError($"Old NetworkGameState:\n{GameDataSerialization.Serialize(oldAuthoritativeClientGameState.NetworkGameState)}\n\nNew NetworkGameState:\n{GameDataSerialization.Serialize(_clientSession.AuthoritativeGameState.NetworkGameState)}\n\n From attempted command batch:\n{GameDataSerialization.Serialize(datedCommandBatch)}");

            NetworkHelpers.SendMessageToServer(NetworkMessageTypeNames.CLIENT_STATE_DIVERGENCE);
        }

        public void NetworkGameStateSnapshotReceived(DatedNetworkGameState stateSnapshot) => _clientSession.ApplyAuthoritativeStateSnapshot(stateSnapshot);

        public void ChangeRequestDenied(ChangeRequestId changeRequestDenied)
        {
            _clientSession.DenyCommandRequest(changeRequestDenied);
            // TODO: Write to this specific client-only (interaction?) state that we were denied and a popup should show for "cannot do that" or something.
        }
    }
}
