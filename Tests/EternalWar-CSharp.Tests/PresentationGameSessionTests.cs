// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Network;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EternalWar.Client.Application.Tests
{
    [TestFixture]
    internal class PresentationGameSessionTests
    {
        private const string DATA_FOLDER_PATH = "../../../../../Data";

        private GameDatabase _database = null!;

        private static readonly Guid _clientAGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid _clientBGuid = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid _clientCGuid = Guid.Parse("00000000-0000-0000-0000-000000000003");
        private static readonly Guid _clientDGuid = Guid.Parse("00000000-0000-0000-0000-000000000004");
        private static readonly ClientInfo _clientA = new(_clientAGuid, 0);
        private static readonly ClientInfo _clientB = new(_clientBGuid, 1);
        private static readonly ClientInfo _clientC = new(_clientCGuid, 2);
        private static readonly ClientInfo _clientD = new(_clientDGuid, 3);

        [SetUp]
        public void SetUp()
        {
            _database = CreateTestDatabase();
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_ExpectedSequence_AppliesChange()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch datedChange = CreatedDatedChange(1);

            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedChange, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(1));
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(CreateLobbyState().ApplyChange(_database, datedChange.Commands[0])));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_OlderThanCurrent_IsAlreadyProcessed()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch datedChange1 = CreatedDatedChange(1);
            DatedNetworkGameCommandBatch datedChange2 = CreatedDatedChange(1);

            session.ApplyAuthoritativeCommandBatch(datedChange1, changeRequestId: null);
            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedChange2, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.AlreadyProcessed));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(1));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_DuplicateSequenceWithDifferentChange_IsProtocolError()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommand change = CreateReadyFact(_clientA);
            NetworkGameCommand differentChange = CreateReadyFact(_clientB);

            DatedNetworkGameCommandBatch datedFirstChange = new(1, change);
            DatedNetworkGameCommandBatch datedDifferentChange = new(1, differentChange);

            session.ApplyAuthoritativeCommandBatch(datedFirstChange, changeRequestId: null);
            Assert.Throws<AlreadyProcessedAuthoritativeCommandBatchDivergenceException>(() => session.ApplyAuthoritativeCommandBatch(datedDifferentChange, changeRequestId: null));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_FutureSequence_SavesForFutureProcessing()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch datedChange = CreatedDatedChange(2);

            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedChange, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.SavedForFutureProcessing));

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(0));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_ReceivingMissingSequence_AppliesPreviouslyBufferedChanges()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch datedChange1 = CreatedDatedChange(1);
            DatedNetworkGameCommandBatch datedChange2 = CreatedDatedChange(2);

            session.ApplyAuthoritativeCommandBatch(datedChange2, changeRequestId: null);
            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedChange1, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(2));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_MultipleFutureChangesArriveOutOfOrder_AppliesInSequence()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch change1 = CreatedDatedChange(1);
            DatedNetworkGameCommandBatch change2 = CreatedDatedChange(2);
            DatedNetworkGameCommandBatch change3 = CreatedDatedChange(3);
            DatedNetworkGameCommandBatch change4 = CreatedDatedChange(4);
            DatedNetworkGameCommandBatch change5 = CreatedDatedChange(5);

            session.ApplyAuthoritativeCommandBatch(change5, changeRequestId: null);
            session.ApplyAuthoritativeCommandBatch(change4, changeRequestId: null);
            session.ApplyAuthoritativeCommandBatch(change3, changeRequestId: null);
            session.ApplyAuthoritativeCommandBatch(change1, changeRequestId: null);

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(1));

            session.ApplyAuthoritativeCommandBatch(change2, changeRequestId: null);

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(5));
        }

        [Test]
        public void ApplyLocalCommandRequest_PredictsChangeImmediately()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest change = CreateChange();

            ChangeRequest request = session.ApplyLocalCommandRequest(CreateChange(), _clientAGuid);

            Assert.That(request.LocalChangeId, Is.Not.Null);
            Assert.That(session.PredictedGameState, Is.EqualTo(CreateLobbyState().ApplyChange(_database, change)));
        }

        [Test]
        public void ApplyLocalCommandRequest_MultipleChanges_ArePredictedInRequestOrder()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest change1 = CreateChange();
            NetworkGameCommandRequest change2 = CreateChange();

            session.ApplyLocalCommandRequest(change1, _clientAGuid);
            session.ApplyLocalCommandRequest(change2, _clientAGuid);

            NetworkGameState expected = CreateLobbyState().ApplyChange(_database, change1).ApplyChange(_database, change2);

            Assert.That(session.PredictedGameState, Is.EqualTo(expected));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_AcknowledgingLocalChange_RemovesItFromPrediction()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch datedChange = CreatedDatedChange(1);
            NetworkGameCommand change = datedChange.Commands[0];

            ChangeRequest request = session.ApplyLocalCommandRequest(CreateChange(), _clientAGuid);

            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedChange, request.LocalChangeId);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(1));

            Assert.That(session.PredictedGameState, Is.EqualTo(session.AuthoritativeGameState.NetworkGameState));
        }

        [Test]
        public void RemoteChangeBeforeLocalAcknowledgement_RebuildsPredictionOnAuthoritativeState()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest localChange = CreateChange();
            NetworkGameCommand remoteChange = CreateReadyFact(_clientB);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, remoteChange);

            session.ApplyLocalCommandRequest(localChange, _clientAGuid);
            session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);

            NetworkGameState expected = CreateLobbyState().ApplyChange(_database, remoteChange).ApplyChange(_database, localChange);

            Assert.That(session.PredictedGameState, Is.EqualTo(expected));
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(CreateLobbyState().ApplyChange(_database, remoteChange)));
        }

        [Test]
        public void LocalAndRemoteChanges_IfOrderIndependent_DoNotRequireReconciliation()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady localChange = new(_clientA);
            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientB);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, remoteChange);

            session.ApplyLocalCommandRequest(localChange, _clientAGuid);

            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));
            Assert.That(session.PredictedGameState, Is.EqualTo(CreateLobbyState().ApplyChange(_database, remoteChange).ApplyChange(_database, localChange)));
        }

        [Test]
        public void LocalAndRemoteChanges_OrderDependent()
        {
            NetworkGameState networkGameState = CreateCombatState(_database);

            NetworkGameCommandRequest localChange = CreateOrderDependentChangeA(networkGameState);
            NetworkGameCommandRequest remoteChange = CreateOrderDependentChangeB(networkGameState);

            NetworkGameState localThenRemote = networkGameState.ApplyChange(_database, localChange).ApplyChange(_database, remoteChange);
            NetworkGameState remoteThenLocal = networkGameState.ApplyChange(_database, remoteChange).ApplyChange(_database, localChange);

            Assert.That(localThenRemote, Is.Not.EqualTo(remoteThenLocal));
        }

        [Test]
        public void LocalAndRemoteChanges_OrderIndependent()
        {
            NetworkGameState networkGameState = CreateLobbyState();

            NetworkGameCommandRequest localChange = CreateOrderIndependentChangeA();
            NetworkGameCommandRequest remoteChange = CreateOrderIndependentChangeB();

            NetworkGameState localThenRemote = networkGameState.ApplyChange(_database, localChange).ApplyChange(_database, remoteChange);
            NetworkGameState remoteThenLocal = networkGameState.ApplyChange(_database, remoteChange).ApplyChange(_database, localChange);

            Assert.That(localThenRemote, Is.EqualTo(remoteThenLocal));
        }

        [Test]
        public void LocalAndRemoteChanges_IfOrderDependent_RequiresReconciliation()
        {
            PresentationGameSession session = CreateCombatSession();
            NetworkGameState networkGameState = session.AuthoritativeGameState.NetworkGameState;

            NetworkGameCommandRequest localChange = CreateOrderDependentChangeA(networkGameState);
            NetworkGameCommandRequest remoteChange = CreateOrderDependentChangeB(networkGameState);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, ExpandRequest(networkGameState, remoteChange));
            DatedNetworkGameCommandBatch datedLocalChange = new(2, ExpandRequest(networkGameState, localChange));

            ChangeRequest localRequest = session.ApplyLocalCommandRequest(localChange, _clientAGuid);
            while (session.TryPeekVisualizationEvent(out _))
            {
                session.ConsumeVisualizationEvent();
            }

            Assert.That(session.CurrentGameState, Is.EqualTo(networkGameState.ApplyChange(_database, localChange)));

            ApplyAuthoritativeCommandBatchResult remoteResult = session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);

            Assert.That(remoteResult, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.RequiresReconciliation));

            ApplyAuthoritativeCommandBatchResult localResult = session.ApplyAuthoritativeCommandBatch(datedLocalChange, localRequest.LocalChangeId);

            Assert.That(localResult, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.RequiresReconciliation));
        }

        [Test]
        public void LocalChangeAfterMultipleRemoteChanges_UsesEntireOrderingForReconciliation()
        {
            PresentationGameSession session = CreateCombatSession();
            NetworkGameState networkGameState = session.AuthoritativeGameState.NetworkGameState;

            // TODO: Make the order dependent changes affect health and create a test action that heals to full, so if self takes damage, then heals self to full, that doesn't equal the same thing as healing to full then taking damage.
            NetworkGameCommandRequest a = CreateOrderDependentChangeA(networkGameState);
            NetworkGameCommandRequest b = CreateOrderDependentChangeB(networkGameState);
            NetworkGameCommandRequest c = CreateOrderDependentChangeC(networkGameState);

            DatedNetworkGameCommandBatch datedB = new(1, ExpandRequest(networkGameState, b));
            DatedNetworkGameCommandBatch datedC = new(2, ExpandRequest(networkGameState, c));
            NetworkGameState stateAfterB = networkGameState.ApplyCommands(_database, datedB.Commands).State;
            NetworkGameState stateAfterC = stateAfterB.ApplyCommands(_database, datedC.Commands).State;
            DatedNetworkGameCommandBatch datedA = new(3, ExpandRequest(stateAfterC, a));

            ChangeRequest request = session.ApplyLocalCommandRequest(a, _clientAGuid);
            session.ConsumeVisualizationEvent();
            session.ApplyAuthoritativeCommandBatch(datedB, changeRequestId: null);
            session.ApplyAuthoritativeCommandBatch(datedC, changeRequestId: null);
            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedA, request.LocalChangeId);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.RequiresReconciliation));
        }

        [Test]
        public void MultipleLocalRequests_ServerAcknowledgesThemInDifferentOrder()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady a = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady b = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady c = new(_clientB);

            DatedNetworkGameCommandBatch datedA = new(2, CreateReadyFact(_clientA));
            DatedNetworkGameCommandBatch datedB = new(1, CreateReadyFact(_clientA));
            DatedNetworkGameCommandBatch datedC = new(3, CreateReadyFact(_clientB));

            ChangeRequest requestA = session.ApplyLocalCommandRequest(a, _clientAGuid);
            ChangeRequest requestB = session.ApplyLocalCommandRequest(b, _clientAGuid);
            ChangeRequest requestC = session.ApplyLocalCommandRequest(c, _clientAGuid);

            session.ApplyAuthoritativeCommandBatch(datedB, requestB.LocalChangeId);
            session.ApplyAuthoritativeCommandBatch(datedA, requestA.LocalChangeId);
            session.ApplyAuthoritativeCommandBatch(datedC, requestC.LocalChangeId);

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(3));
        }

        [Test]
        public void DenyChangeRequest_BeforeVisualization_RemovesPrediction()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest change = CreateChange();

            ChangeRequest request = session.ApplyLocalCommandRequest(CreateChange(), _clientAGuid);

            session.DenyCommandRequest(request.LocalChangeId!);

            Assert.That(session.PredictedGameState, Is.EqualTo(session.AuthoritativeGameState.NetworkGameState));
            Assert.That(session.TryPeekVisualizationEvent(out _), Is.False);
        }

        [Test]
        public void DenyChangeRequest_AfterVisualization_ReconcilesPresentationState()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest change = CreateChange();

            ChangeRequest request = session.ApplyLocalCommandRequest(change, _clientAGuid);

            // The client has presented the predicted change.
            Assert.That(session.TryPeekVisualizationEvent(out NetworkGameEvent? peekedChange), Is.True);
            Assert.That(peekedChange, Is.EqualTo(FirstEvent(initialLobbyState, request.Request.ResolveToCommands(_database, initialLobbyState))));
            // Make sure predicted game state is what we think it should be
            NetworkGameState expectedStateAfterLocalChange = initialLobbyState.ApplyChange(_database, change);
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedStateAfterLocalChange));

            session.ConsumeVisualizationEvent();

            // The prediction is now visually present.
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedStateAfterLocalChange));
            Assert.That(session.CurrentGameState, Is.EqualTo(expectedStateAfterLocalChange));
            Assert.That(session.CurrentGameState, Is.EqualTo(session.PredictedGameState));

            session.DenyCommandRequest(request.LocalChangeId!);

            // Desired behavior:
            Assert.That(session.CurrentGameState, Is.EqualTo(session.AuthoritativeGameState.NetworkGameState));
        }

        [Test]
        public void DenyFirstOfMultiplePendingChanges_RebuildsRemainingPrediction()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady a = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady b = new(_clientB);

            ChangeRequest requestA = session.ApplyLocalCommandRequest(a, _clientAGuid);
            session.ApplyLocalCommandRequest(b, _clientAGuid);

            session.DenyCommandRequest(requestA.LocalChangeId!);

            NetworkGameState expected = CreateLobbyState().ApplyChange(_database, b);

            Assert.That(session.PredictedGameState, Is.EqualTo(expected));
        }

        [Test]
        public void DenyMiddlePendingChange_RebuildsPrediction()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady a = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady b = new(_clientB);
            NetworkGameCommandRequest_Lobby_ToggleReady c = new(_clientC);

            session.ApplyLocalCommandRequest(a, _clientAGuid);
            ChangeRequest requestB = session.ApplyLocalCommandRequest(b, _clientAGuid);
            session.ApplyLocalCommandRequest(c, _clientAGuid);

            session.DenyCommandRequest(requestB.LocalChangeId!);

            NetworkGameState expected = CreateLobbyState().ApplyChange(_database, a).ApplyChange(_database, c);

            Assert.That(session.PredictedGameState, Is.EqualTo(expected));
        }

        [Test]
        public void DenyChangeRequest_UnknownRequest_DoesNotChangeState()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState before = session.PredictedGameState;

            ChangeRequestId unknown = ChangeRequestId.New(_clientAGuid);

            session.DenyCommandRequest(unknown);

            Assert.That(session.PredictedGameState, Is.EqualTo(before));
            Assert.That(session.CurrentGameState, Is.EqualTo(CreateLobbyState()));
        }

        [Test]
        public void AuthoritativeChange_WithUnknownRequestId_IsHandledAsRemoteChange()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch datedChange = CreatedDatedChange(1);

            ChangeRequestId unknown = ChangeRequestId.New(_clientAGuid);

            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedChange, unknown);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(1));
        }

        [Test]
        public void AcknowledgedLocalChange_BeforeVisualization_IsVisualizedOnce()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch datedChange = CreatedDatedChange(1);
            NetworkGameCommand change = datedChange.Commands[0];

            ChangeRequest request = session.ApplyLocalCommandRequest(CreateChange(), _clientAGuid);
            session.ApplyAuthoritativeCommandBatch(datedChange, request.LocalChangeId);

            Assert.That(session.TryPeekVisualizationEvent(out NetworkGameEvent? visualizationChange), Is.True);
            Assert.That(visualizationChange, Is.EqualTo(FirstEvent(session.CurrentGameState, new ValueArray<NetworkGameCommand>(change))));

            session.ConsumeVisualizationEvent();

            Assert.That(session.TryPeekVisualizationEvent(out _), Is.False);
        }

        [Test]
        public void AcknowledgedLocalChange_AfterVisualization_IsNotVisualizedTwice()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch datedChange = CreatedDatedChange(1);
            NetworkGameCommand change = datedChange.Commands[0];

            ChangeRequest request = session.ApplyLocalCommandRequest(CreateChange(), _clientAGuid);

            session.ConsumeVisualizationEvent();
            session.ApplyAuthoritativeCommandBatch(datedChange, request.LocalChangeId);

            Assert.That(session.TryPeekVisualizationEvent(out _), Is.False);
        }

        [Test]
        public void RemoteChangeWhileLocalPredictionIsUnvisualized_AuthoritativeVisualizationTakesPriority()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady localChange = new(_clientA);
            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientB);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, remoteChange);

            session.ApplyLocalCommandRequest(localChange, _clientAGuid);
            session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);

            Assert.That(session.TryPeekVisualizationEvent(out NetworkGameEvent? visualizationChange), Is.True);
            Assert.That(visualizationChange, Is.EqualTo(FirstEvent(session.CurrentGameState, new ValueArray<NetworkGameCommand>(remoteChange))));
        }

        [Test]
        public void AcknowledgedLocalChangeThatIsFuture_IsHeldUntilMissingSequenceArrives()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady a = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady b = new(_clientB);

            DatedNetworkGameCommandBatch datedA = new(2, CreateReadyFact(_clientA));
            DatedNetworkGameCommandBatch datedB = new(1, CreateReadyFact(_clientB));

            ChangeRequest requestA = session.ApplyLocalCommandRequest(a, _clientAGuid);
            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedA, requestA.LocalChangeId);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.SavedForFutureProcessing));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(0));

            session.ApplyAuthoritativeCommandBatch(datedB, changeRequestId: null);

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(2));
        }

        [Test]
        public void Reset_ClearsAuthoritativePredictionVisualizationAndBufferedChanges()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest localChange = CreateChange();
            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientB);

            DatedNetworkGameCommandBatch datedRemoteChange = new(3, remoteChange);

            session.ApplyLocalCommandRequest(localChange, _clientAGuid);
            session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);

            session.Reset();

            Assert.That(session.AuthoritativeGameState, Is.EqualTo(DatedNetworkGameState.InitialState));
            Assert.That(session.PredictedGameState, Is.EqualTo(NetworkGameState.InitialState));
            Assert.That(session.CurrentGameState, Is.EqualTo(NetworkGameState.InitialState));
            Assert.That(session.TryPeekVisualizationEvent(out _), Is.False);
        }

        [Test]
        public void OverwriteAuthoritativeState_ClearsAllLocalAndBufferedState()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientB);

            DatedNetworkGameCommandBatch datedRemoteChange = new(10, remoteChange);

            session.ApplyLocalCommandRequest(CreateChange(), _clientAGuid);
            session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);

            NetworkGameState newState = CreateCombatState(_database);

            session.OverwriteAuthoritativeState(new DatedNetworkGameState(100, newState));

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(100));
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(newState));
            Assert.That(session.PredictedGameState, Is.EqualTo(newState));
            Assert.That(session.CurrentGameState, Is.EqualTo(newState));
            Assert.That(session.TryPeekVisualizationEvent(out _), Is.False);
        }

        [Test]
        public void DenyFirstVisualizedLocalChange_ReconcilesToStateWithoutDeniedChange()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest_Lobby_ToggleReady changeA = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady changeB = new(_clientB);

            ChangeRequest requestA = session.ApplyLocalCommandRequest(changeA, _clientAGuid);
            session.ApplyLocalCommandRequest(changeB, _clientAGuid);

            // Present both predicted changes.
            session.ConsumeVisualizationEvent();
            session.ConsumeVisualizationEvent();

            NetworkGameState expectedBeforeDenial = initialLobbyState.ApplyChange(_database, changeA).ApplyChange(_database, changeB);

            Assert.That(session.CurrentGameState, Is.EqualTo(expectedBeforeDenial));

            session.DenyCommandRequest(requestA.LocalChangeId!);

            NetworkGameState expectedPredictedAfterDenial = initialLobbyState.ApplyChange(_database, changeB);

            Assert.That(session.CurrentGameState, Is.EqualTo(expectedPredictedAfterDenial));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredictedAfterDenial));
        }

        [Test]
        public void DenySecondVisualizedLocalChange_ReconcilesToStateWithoutDeniedChange()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest_Lobby_ToggleReady changeA = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady changeB = new(_clientB);

            session.ApplyLocalCommandRequest(changeA, _clientAGuid);
            ChangeRequest requestB = session.ApplyLocalCommandRequest(changeB, _clientAGuid);

            // Present both predicted changes.
            session.ConsumeVisualizationEvent();
            session.ConsumeVisualizationEvent();

            NetworkGameState expectedBeforeDenial = initialLobbyState.ApplyChange(_database, changeA).ApplyChange(_database, changeB);

            Assert.That(session.CurrentGameState, Is.EqualTo(expectedBeforeDenial));

            session.DenyCommandRequest(requestB.LocalChangeId!);

            NetworkGameState expectedAfterDenial = initialLobbyState.ApplyChange(_database, changeA);

            Assert.That(session.CurrentGameState, Is.EqualTo(expectedAfterDenial));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedAfterDenial));
        }

        [Test]
        public void VisualizedLocalThenRemoteThenVisualizedLocal_MaintainsCorrectState()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest_Lobby_ToggleReady localChangeA = new(_clientA);
            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientB);
            NetworkGameCommandRequest_Lobby_ToggleReady localChangeB = new(_clientC);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, remoteChange);

            ChangeRequest requestA = session.ApplyLocalCommandRequest(localChangeA, _clientAGuid);
            session.ConsumeVisualizationEvent();
            ApplyAuthoritativeCommandBatchResult remoteResult = session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);


            NetworkGameState expectedCurrent = initialLobbyState.ApplyChange(_database, localChangeA);
            NetworkGameState expectedPredicted = initialLobbyState.ApplyChange(_database, localChangeA, remoteChange);
            NetworkGameState expectedAuthoritative = initialLobbyState.ApplyChange(_database, remoteChange);

            Assert.That(remoteResult, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));
            Assert.That(session.CurrentGameState, Is.EqualTo(expectedCurrent));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredicted));
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expectedAuthoritative));

            NetworkGameState aThenRemote = initialLobbyState.ApplyChange(_database, localChangeA, remoteChange);
            NetworkGameState remoteThenA = initialLobbyState.ApplyChange(_database, remoteChange, localChangeA);

            Assert.That(aThenRemote, Is.EqualTo(remoteThenA));

            ChangeRequest requestB = session.ApplyLocalCommandRequest(localChangeB, _clientAGuid);
            session.ConsumeVisualizationEvent();

            expectedCurrent = initialLobbyState.ApplyChange(_database, localChangeA, remoteChange);
            expectedPredicted = initialLobbyState.ApplyChange(_database, remoteChange, localChangeA, localChangeB);
            expectedAuthoritative = initialLobbyState.ApplyChange(_database, remoteChange);

            Assert.That(session.CurrentGameState, Is.EqualTo(expectedCurrent));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredicted));
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expectedAuthoritative));

            session.ConsumeVisualizationEvent();

            expectedCurrent = initialLobbyState.ApplyChange(_database, localChangeA, remoteChange, localChangeB);
            expectedPredicted = initialLobbyState.ApplyChange(_database, remoteChange, localChangeA, localChangeB);
            expectedAuthoritative = initialLobbyState.ApplyChange(_database, remoteChange);

            Assert.That(session.CurrentGameState, Is.EqualTo(expectedCurrent));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredicted));
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expectedAuthoritative));
            Assert.That(session.CurrentGameState, Is.EqualTo(session.PredictedGameState));

            // Make sure the requests are still independently tracked.
            Assert.That(requestA.LocalChangeId, Is.Not.Null);
            Assert.That(requestB.LocalChangeId, Is.Not.Null);
        }

        [Test]
        public void VisualizedLocalAndRemoteChanges_IfOrderIndependent_DoNotRequireReconciliation()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest_Lobby_ToggleReady localChange = new(_clientA);
            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientB);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, remoteChange);

            session.ApplyLocalCommandRequest(localChange, _clientAGuid);
            session.ConsumeVisualizationEvent();

            Assert.That(session.CurrentGameState, Is.EqualTo(initialLobbyState.ApplyChange(_database, localChange)));

            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));

            NetworkGameState expectedAuthoritative = initialLobbyState.ApplyChange(_database, remoteChange);
            NetworkGameState expectedPredicted = expectedAuthoritative.ApplyChange(_database, localChange);

            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expectedAuthoritative));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredicted));
            Assert.That(session.CurrentGameState, Is.EqualTo(initialLobbyState.ApplyChange(_database, localChange)));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_DuplicateFutureSequence_DoesNotThrow()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch change = CreatedDatedChange(2);

            ApplyAuthoritativeCommandBatchResult firstResult = session.ApplyAuthoritativeCommandBatch(change, changeRequestId: null);

            ApplyAuthoritativeCommandBatchResult secondResult = session.ApplyAuthoritativeCommandBatch(change, changeRequestId: null);

            Assert.That(firstResult, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.SavedForFutureProcessing));

            Assert.That(secondResult, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.SavedForFutureProcessing));

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(0));
        }

        [Test]
        public void Reset_ClearsUnacknowledgedVisualizationSnapshots()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady firstChange = new(_clientA);

            ChangeRequest firstRequest = session.ApplyLocalCommandRequest(firstChange, _clientAGuid);
            session.ConsumeVisualizationEvent();
            session.Reset();

            NetworkGameCommandRequest_Lobby_ToggleReady secondChange = new(_clientB);

            ChangeRequest secondRequest = session.ApplyLocalCommandRequest(secondChange, _clientAGuid);
            session.ConsumeVisualizationEvent();
            session.DenyCommandRequest(secondRequest.LocalChangeId!);

            Assert.That(session.CurrentGameState, Is.EqualTo(NetworkGameState.InitialState));
            Assert.That(session.PredictedGameState, Is.EqualTo(NetworkGameState.InitialState));
        }

        [Test]
        public void Reset_OldRequestCannotAffectNewPresentationState()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady changeA = new(_clientA);

            ChangeRequest oldRequest = session.ApplyLocalCommandRequest(changeA, _clientAGuid);
            session.ConsumeVisualizationEvent();
            session.Reset();
            // The old request has already been invalidated by Reset.
            session.DenyCommandRequest(oldRequest.LocalChangeId!);

            Assert.That(session.CurrentGameState, Is.EqualTo(NetworkGameState.InitialState));
            Assert.That(session.PredictedGameState, Is.EqualTo(NetworkGameState.InitialState));
        }

        [Test]
        public void OverwriteAuthoritativeState_OldRequestCannotAffectNewPresentationState()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest_Lobby_ToggleReady changeA = new(_clientA);

            ChangeRequest oldRequest = session.ApplyLocalCommandRequest(changeA, _clientAGuid);
            session.ConsumeVisualizationEvent();

            NetworkGameState newState = CreateCombatState(_database);
            DatedNetworkGameState datedNewState = new(100, newState);

            session.OverwriteAuthoritativeState(datedNewState);
            session.DenyCommandRequest(oldRequest.LocalChangeId!);

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(100));
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(newState));
            Assert.That(session.CurrentGameState, Is.EqualTo(newState));
            Assert.That(session.PredictedGameState, Is.EqualTo(newState));
        }

        [Test]
        public void AcknowledgingOneOfMultipleLocalChanges_LeavesOtherPredictionIntact()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest_Lobby_ToggleReady changeA = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady changeB = new(_clientB);

            DatedNetworkGameCommandBatch datedChangeA = new(1, CreateReadyFact(_clientA));

            ChangeRequest requestA = session.ApplyLocalCommandRequest(changeA, _clientAGuid);
            session.ApplyLocalCommandRequest(changeB, _clientAGuid);
            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedChangeA, requestA.LocalChangeId);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));

            NetworkGameState expected = initialLobbyState.ApplyChange(_database, changeA).ApplyChange(_database, changeB);

            Assert.That(session.PredictedGameState, Is.EqualTo(expected));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(1));
        }

        [Test]
        public void DenyVisualizedLocalChange_AfterRemoteChangeArrives_ReconcilesRemoteChange()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest_Lobby_ToggleReady localChange = new(_clientA);
            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientB);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, remoteChange);

            ChangeRequest localRequest = session.ApplyLocalCommandRequest(localChange, _clientAGuid);
            session.ConsumeVisualizationEvent();

            // Remote change arrives after the local prediction was visualized.
            session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);
            // Local change is denied.
            session.DenyCommandRequest(localRequest.LocalChangeId!);

            NetworkGameState expected = initialLobbyState.ApplyChange(_database, remoteChange);

            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expected));
            Assert.That(session.PredictedGameState, Is.EqualTo(expected));
            Assert.That(session.CurrentGameState, Is.EqualTo(initialLobbyState));
            Assert.That(session.TryPeekVisualizationEvent(out NetworkGameEvent? visualizationChange), Is.True);
            Assert.That(visualizationChange, Is.EqualTo(FirstEvent(session.CurrentGameState, new ValueArray<NetworkGameCommand>(remoteChange))));

            session.ConsumeVisualizationEvent();
            Assert.That(session.CurrentGameState, Is.EqualTo(expected));
        }

        [Test]
        public void DenyVisualizedLocalChange_AfterRemoteChangeArrives_AndOtherLocalChangeVisualized_ReconcilesRemoteChange()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest_Lobby_ToggleReady localChangeA = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady localChangeB = new(_clientB);
            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientC);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, remoteChange);

            ChangeRequest localRequestA = session.ApplyLocalCommandRequest(localChangeA, _clientAGuid);
            session.ConsumeVisualizationEvent();

            // Remote change arrives after the local prediction was visualized.
            session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);
            // Another local change gets applied
            session.ApplyLocalCommandRequest(localChangeB, _clientAGuid);
            // Local change is denied.
            session.DenyCommandRequest(localRequestA.LocalChangeId!);

            NetworkGameState expectedAuthoritative = initialLobbyState.ApplyChange(_database, remoteChange);
            NetworkGameState expectedPredicted = initialLobbyState.ApplyChange(_database, remoteChange, localChangeB);

            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expectedAuthoritative));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredicted));
            Assert.That(session.CurrentGameState, Is.EqualTo(initialLobbyState));

            Assert.That(session.TryPeekVisualizationEvent(out NetworkGameEvent? visualizationChange), Is.True);
            Assert.That(visualizationChange, Is.EqualTo(FirstEvent(session.CurrentGameState, new ValueArray<NetworkGameCommand>(remoteChange))));

            session.ConsumeVisualizationEvent();
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expectedAuthoritative));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredicted));
            Assert.That(session.CurrentGameState, Is.EqualTo(expectedAuthoritative));

            Assert.That(session.TryPeekVisualizationEvent(out visualizationChange), Is.True);
            Assert.That(visualizationChange, Is.EqualTo(FirstEvent(expectedAuthoritative, localChangeB.ResolveToCommands(_database, expectedAuthoritative))));

            session.ConsumeVisualizationEvent();
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expectedAuthoritative));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredicted));
            Assert.That(session.CurrentGameState, Is.EqualTo(expectedPredicted));
        }

        [Test]
        public void DenyVisualizedLocalChange_AfterRemoteChangeArrivesAndVisualized_AndOtherLocalChangeVisualized_ReconcilesRemoteChange()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommandRequest_Lobby_ToggleReady localChangeA = new(_clientA);
            NetworkGameCommandRequest_Lobby_ToggleReady localChangeB = new(_clientB);
            NetworkGameCommand_Lobby_ReadyToggled remoteChange = new(_clientC);

            DatedNetworkGameCommandBatch datedRemoteChange = new(1, remoteChange);

            ChangeRequest localRequestA = session.ApplyLocalCommandRequest(localChangeA, _clientAGuid);
            session.ConsumeVisualizationEvent();

            session.ApplyAuthoritativeCommandBatch(datedRemoteChange, changeRequestId: null);
            session.ConsumeVisualizationEvent();

            session.ApplyLocalCommandRequest(localChangeB, _clientAGuid);
            session.ConsumeVisualizationEvent();

            // Deny the first change
            session.DenyCommandRequest(localRequestA.LocalChangeId!);

            NetworkGameState expectedAuthoritative = initialLobbyState.ApplyChange(_database, remoteChange);
            NetworkGameState expectedPredicted = initialLobbyState.ApplyChange(_database, remoteChange, localChangeB);

            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(expectedAuthoritative));
            Assert.That(session.PredictedGameState, Is.EqualTo(expectedPredicted));
            Assert.That(session.CurrentGameState, Is.EqualTo(expectedPredicted));

            Assert.That(session.TryPeekVisualizationEvent(out NetworkGameEvent? visualizationChange), Is.False);
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_DuplicateFutureSequenceWithDifferentChange_IsProtocolError()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch first = new(2, CreateReadyFact(_clientA));
            DatedNetworkGameCommandBatch different = new(2, CreateReadyFact(_clientB));

            session.ApplyAuthoritativeCommandBatch(first, changeRequestId: null);

            Assert.Throws<AlreadyPendingAuthoritativeCommandBatchDivergenceException>(() => session.ApplyAuthoritativeCommandBatch(different, null));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_VeryOldChange_RemainsRejectedAfterFirstRejection()
        {
            PresentationGameSession session = CreateLobbySession();

            // Fill the recent-change buffer so sequence 1 is no longer remembered.
            for (long sequence = 1; sequence <= PresentationGameSession.RECENT_COMMAND_BATCH_COUNT_TO_HOLD + 1; ++sequence)
            {
                session.ApplyAuthoritativeCommandBatch(CreatedDatedChange(sequence), changeRequestId: null);
            }

            DatedNetworkGameCommandBatch veryOldChange = CreatedDatedChange(1);

            Assert.Throws<TooOldCommandBatchException>(() => session.ApplyAuthoritativeCommandBatch(veryOldChange, changeRequestId: null));

            // The rejected change was never processed, so rejecting it again should
            // still produce the same protocol error rather than AlreadyProcessed.
            Assert.Throws<TooOldCommandBatchException>(() => session.ApplyAuthoritativeCommandBatch(veryOldChange, changeRequestId: null));

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(33));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_ConflictingDuplicate_DoesNotBecomeRecognizedAsProcessed()
        {
            PresentationGameSession session = CreateLobbySession();
            NetworkGameState initialLobbyState = session.CurrentGameState;

            NetworkGameCommand originalChange = CreateReadyFact(_clientA);
            NetworkGameCommand conflictingChange = CreateReadyFact(_clientB);

            DatedNetworkGameCommandBatch datedOriginalChange = new(1, originalChange);
            DatedNetworkGameCommandBatch datedConflictingChange = new(1, conflictingChange);

            session.ApplyAuthoritativeCommandBatch(datedOriginalChange, changeRequestId: null);

            Assert.Throws<AlreadyProcessedAuthoritativeCommandBatchDivergenceException>(() => session.ApplyAuthoritativeCommandBatch(datedConflictingChange, changeRequestId: null));

            // The conflicting change was never processed. The valid original change
            // should still be the one recognized as having been processed.
            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedOriginalChange, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.AlreadyProcessed));
            Assert.That(session.AuthoritativeGameState.NetworkGameState, Is.EqualTo(initialLobbyState.ApplyChange(_database, originalChange)));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_ConflictingDuplicate_DoesNotPoisonRecentChanges()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommand originalChange = CreateReadyFact(_clientA);
            NetworkGameCommand conflictingChange = CreateReadyFact(_clientB);

            DatedNetworkGameCommandBatch datedOriginalChange = new(1, originalChange);
            DatedNetworkGameCommandBatch datedConflictingChange = new(1, conflictingChange);

            session.ApplyAuthoritativeCommandBatch(datedOriginalChange, changeRequestId: null);

            Assert.Throws<AlreadyProcessedAuthoritativeCommandBatchDivergenceException>(() => session.ApplyAuthoritativeCommandBatch(datedConflictingChange, changeRequestId: null));

            // Push the original sequence out of the 32-entry buffer while leaving
            // the invalid conflicting entry potentially inside it.
            for (long sequence = 2; sequence <= PresentationGameSession.RECENT_COMMAND_BATCH_COUNT_TO_HOLD + 1; ++sequence)
            {
                session.ApplyAuthoritativeCommandBatch(CreatedDatedChange(sequence), changeRequestId: null);
            }

            // Sequence 1 is no longer a valid recent processed change. In particular,
            // the conflicting message must not have become an accepted historical change.
            Assert.Throws<TooOldCommandBatchException>(() => session.ApplyAuthoritativeCommandBatch(datedConflictingChange, changeRequestId: null));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_AcknowledgedRequestWithDifferentChange_ThrowsAndLeavesRequestPending()
        {
            PresentationGameSession session = CreateLobbySession();

            NetworkGameCommandRequest localChange = CreateChange();
            NetworkGameCommand differentChange = CreateReadyFact(_clientB);

            ChangeRequest request = session.ApplyLocalCommandRequest(localChange, _clientAGuid);
            DatedNetworkGameCommandBatch divergentAcknowledgement = new(1, differentChange);

            Assert.Throws<AcknowledgedCommandBatchDivergenceException>(() => session.ApplyAuthoritativeCommandBatch(divergentAcknowledgement, request.LocalChangeId));

            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(0));
            Assert.That(session.PredictedGameState, Is.EqualTo(CreateLobbyState().ApplyChange(_database, localChange)));
        }

        [Test]
        public void ApplyAuthoritativeCommandBatch_RepeatedFutureDuplicateDoesNotEvictRecentProcessedChanges()
        {
            PresentationGameSession session = CreateLobbySession();

            DatedNetworkGameCommandBatch change1 = CreatedDatedChange(1);
            DatedNetworkGameCommandBatch change2 = CreatedDatedChange(2);
            DatedNetworkGameCommandBatch change3 = CreatedDatedChange(3);

            session.ApplyAuthoritativeCommandBatch(change1, changeRequestId: null);

            // A future change can be retransmitted many times before the missing sequence arrives. Those retransmissions should not count as distinct recently received changes.
            ApplyAuthoritativeCommandBatchResult result;
            for (int i = 0; i < PresentationGameSession.RECENT_COMMAND_BATCH_COUNT_TO_HOLD; ++i)
            {
                result = session.ApplyAuthoritativeCommandBatch(change3, changeRequestId: null);
                Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.SavedForFutureProcessing));
            }

            session.ApplyAuthoritativeCommandBatch(change2, changeRequestId: null);

            // Sequence 1 is only two processed changes behind the head, so it should still be recognized as a valid duplicate.
            result = session.ApplyAuthoritativeCommandBatch(change1, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.AlreadyProcessed));
        }

        [Test]
        public void ConsumeVisualizationEvent_WithNoPendingChanges_ThrowsEmptyConsumeVisualizationException()
        {
            PresentationGameSession session = CreateLobbySession();

            Assert.Throws<EmptyConsumeVisualizationException>(() => session.ConsumeVisualizationEvent());
        }

        [Test]
        public void AuthoritativeChangeWhilePreviousChangeIsBeingPresented_DoesNotRequireReconciliation()
        {
            PresentationGameSession session = CreateCombatSession();
            NetworkGameState initialCombatState = session.CurrentGameState;

            NetworkGameCommandRequest changeA = CreateChangeThatTakesMultipleConsumptionsA(initialCombatState);
            NetworkGameCommandRequest changeB = CreateChangeThatTakesMultipleConsumptionsB(initialCombatState);

            DatedNetworkGameCommandBatch datedChangeA = new(1, ExpandRequest(initialCombatState, changeA));
            DatedNetworkGameCommandBatch datedChangeB = new(2, ExpandRequest(initialCombatState, changeB));

            session.ApplyAuthoritativeCommandBatch(datedChangeA, changeRequestId: null);

            // Begin presenting A, but deliberately don't finish it.
            session.ConsumeVisualizationEvent();

            // Make sure it isn't finished
            Assert.That(session.TryPeekVisualizationEvent(out _), Is.True);

            // Now receive B while A is still being presented.
            ApplyAuthoritativeCommandBatchResult result = session.ApplyAuthoritativeCommandBatch(datedChangeB, changeRequestId: null);

            Assert.That(result, Is.EqualTo(ApplyAuthoritativeCommandBatchResult.Success));
        }

        private PresentationGameSession CreateLobbySession()
        {
            PresentationGameSession session = new(_database);

            NetworkGameState networkGameState = CreateLobbyState();
            DatedNetworkGameState datedNetworkGameState = new(0, networkGameState);
            session.OverwriteAuthoritativeState(datedNetworkGameState);

            return session;
        }

        private PresentationGameSession CreateCombatSession()
        {
            PresentationGameSession session = new(_database);

            NetworkGameState networkGameState = CreateCombatState(_database);
            DatedNetworkGameState datedNetworkGameState = new(0, networkGameState);
            session.OverwriteAuthoritativeState(datedNetworkGameState);

            return session;
        }

        private static DatedNetworkGameCommandBatch CreatedDatedChange(long sequence)
        {
            return new DatedNetworkGameCommandBatch(sequence, CreateReadyFact(_clientA));
        }

        private ValueArray<NetworkGameCommand> ExpandRequest(NetworkGameState networkGameState, NetworkGameCommandRequest request) => request.ResolveToCommands(_database, networkGameState);

        private NetworkGameEvent FirstEvent(NetworkGameState state, ValueArray<NetworkGameCommand> commands) => state.ApplyCommands(_database, commands).Events[0];

        private static NetworkGameCommand_Lobby_ReadyToggled CreateReadyFact(ClientInfo client) => new(client);

        private static NetworkGameCommandRequest_Lobby_ToggleReady CreateChange() => new(_clientA);

        private static NetworkGameCommandRequest_Lobby_ToggleReady CreateDifferentChange() => new(_clientB);

        private static NetworkGameCommandRequest_Lobby_ToggleReady CreateOrderIndependentChangeA() => new(_clientA);

        private static NetworkGameCommandRequest_Lobby_ToggleReady CreateOrderIndependentChangeB() => new(_clientB);

        private static NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence CreateOrderDependentChangeA(NetworkGameState networkGameState)
        {
            // Action for hitting self for 5 (unit who starts at (3, 0))
            const string ACTION_SEQUENCE_ID = "Debug_HitSelf_5";

            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatInstance combatInstance = combatRuntime.Instance;

            GridPosition desiredUnitGridPosition = new(3, 0);
            CombatUnitUniqueIdentifier unitUniqueIdentifier = combatInstance.UnitMap.First(kvp => kvp.Value.StartingPosition == desiredUnitGridPosition).Key;

            ValueArray<GridPositionGroup> selection = new(GridPositionGroup.CreateForSinglePosition(desiredUnitGridPosition));

            ActionSelectionContext actionSelectionContext = new(selection);
            ActionExecutionContext actionExecutionContext = new(ACTION_SEQUENCE_ID, unitUniqueIdentifier, actionSelectionContext);
            return new NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence(actionExecutionContext);
        }

        private static NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence CreateOrderDependentChangeB(NetworkGameState networkGameState)
        {
            // Action for full healing self (unit who starts at (3, 0))
            const string ACTION_SEQUENCE_ID = "Debug_FullHeal";

            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatInstance combatInstance = combatRuntime.Instance;

            GridPosition desiredUnitGridPosition = new(3, 0);
            CombatUnitUniqueIdentifier unitUniqueIdentifier = combatInstance.UnitMap.First(kvp => kvp.Value.StartingPosition == desiredUnitGridPosition).Key;

            ValueArray<GridPositionGroup> selection = new(GridPositionGroup.CreateForSinglePosition(desiredUnitGridPosition));

            ActionSelectionContext actionSelectionContext = new(selection);
            ActionExecutionContext actionExecutionContext = new(ACTION_SEQUENCE_ID, unitUniqueIdentifier, actionSelectionContext);
            return new NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence(actionExecutionContext);
        }

        private static NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence CreateOrderDependentChangeC(NetworkGameState networkGameState)
        {
            // Action for hitting self for 1 (unit who starts at (3, 0))
            const string ACTION_SEQUENCE_ID = "Debug_HitSelf_1";

            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatInstance combatInstance = combatRuntime.Instance;

            GridPosition desiredUnitGridPosition = new(3, 0);
            CombatUnitUniqueIdentifier unitUniqueIdentifier = combatInstance.UnitMap.First(kvp => kvp.Value.StartingPosition == desiredUnitGridPosition).Key;

            ValueArray<GridPositionGroup> selection = new(GridPositionGroup.CreateForSinglePosition(desiredUnitGridPosition));

            ActionSelectionContext actionSelectionContext = new(selection);
            ActionExecutionContext actionExecutionContext = new(ACTION_SEQUENCE_ID, unitUniqueIdentifier, actionSelectionContext);
            return new NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence(actionExecutionContext);
        }

        private static NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence CreateChangeThatTakesMultipleConsumptionsA(NetworkGameState networkGameState)
        {
            // Action for hitting self for 1 (unit who starts at (3, 0))
            const string ACTION_SEQUENCE_ID = "Debug_HitSelf_1";

            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatInstance combatInstance = combatRuntime.Instance;

            GridPosition desiredUnitGridPosition = new(3, 0);
            CombatUnitUniqueIdentifier unitUniqueIdentifier = combatInstance.UnitMap.First(kvp => kvp.Value.StartingPosition == desiredUnitGridPosition).Key;

            ValueArray<GridPositionGroup> selection = new(GridPositionGroup.CreateForSinglePosition(desiredUnitGridPosition));

            ActionSelectionContext actionSelectionContext = new(selection);
            ActionExecutionContext actionExecutionContext = new(ACTION_SEQUENCE_ID, unitUniqueIdentifier, actionSelectionContext);
            return new NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence(actionExecutionContext);
        }

        private static NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence CreateChangeThatTakesMultipleConsumptionsB(NetworkGameState networkGameState)
        {
            // Action for hitting self for 5 (unit who starts at (3, 0))
            const string ACTION_SEQUENCE_ID = "Debug_HitSelf_5";

            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatInstance combatInstance = combatRuntime.Instance;

            GridPosition desiredUnitGridPosition = new(3, 0);
            CombatUnitUniqueIdentifier unitUniqueIdentifier = combatInstance.UnitMap.First(kvp => kvp.Value.StartingPosition == desiredUnitGridPosition).Key;

            ValueArray<GridPositionGroup> selection = new(GridPositionGroup.CreateForSinglePosition(desiredUnitGridPosition));

            ActionSelectionContext actionSelectionContext = new(selection);
            ActionExecutionContext actionExecutionContext = new(ACTION_SEQUENCE_ID, unitUniqueIdentifier, actionSelectionContext);
            return new NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence(actionExecutionContext);
        }

        private static NetworkGameState CreateLobbyState()
        {
            ValueArray<ClientInfo> connectedClients = new(_clientA, _clientB, _clientC, _clientD);
            GameState gameState = new(LobbyRuntime.InitialState);
            return new NetworkGameState(connectedClients, gameState);
        }

        private static NetworkGameState CreateCombatState(GameDatabase database)
        {
            const string COMBAT_ID = "Forest_Test";
            string[] TEST_PLAYER_UNIT_IDS = new string[4] {
                "Apothecary",
                "Warrior",
                "Barbarian",
                "Soldier"
            };

            ValueArray<ClientInfo> connectedClients = new(_clientA, _clientB);

            ValueArray<CombatUnitUniqueIdentifier> fakePlayerChosenIdentifiers = CreatePlayerUnitIdentifiers(connectedClients, TEST_PLAYER_UNIT_IDS);
            RunRuntime runRuntime = RunInitialization.InitializeRun(fakePlayerChosenIdentifiers, RandomnessSeed: 0);

            ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> fakePlayerUnitPlacements = CreateFakePlayerUnitPlacements(database, COMBAT_ID, fakePlayerChosenIdentifiers);
            CombatRuntime combatRuntime = CombatInitialization.InitializeCombat(database, runRuntime, COMBAT_ID, Vector3.Zero, fakePlayerUnitPlacements, out DeterministicRandomState randomStateResult);

            runRuntime = runRuntime with
            {
                State = runRuntime.State with
                {
                    Phase = combatRuntime,
                    RandomState = randomStateResult
                }
            };
            GameState gameState = new(runRuntime);
            return new NetworkGameState(connectedClients, gameState);
        }

        private static GameDatabase CreateTestDatabase()
        {
            return DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
        }

        private static ValueArray<CombatUnitUniqueIdentifier> CreatePlayerUnitIdentifiers(ValueArray<ClientInfo> connectedClients, IEnumerable<string> playerUnitDefinitionIds)
        {
            using ValueArray<CombatUnitUniqueIdentifier>.Builder unitIdentifiersBuilder = new();

            using PooledList<ClientInfo> clientIds = new(connectedClients);
            clientIds.Sort((a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
            foreach (string unitId in playerUnitDefinitionIds)
            {
                // TODO: Add something that can assign an owner based session save data
                if (clientIds.Count <= 0)
                {
                    clientIds.AddRange(connectedClients);
                    clientIds.Sort((a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
                }
                CombatUnitOwner owner = new CombatUnitOwner_Player(clientIds[0]);
                clientIds.RemoveAt(0);
                int existingUnitCountWithSameIdentifier = unitIdentifiersBuilder.Count(identifier => identifier.Owner == owner && identifier.DefinitionId == unitId);
                CombatUnitUniqueIdentifier unitIdentifier = new(owner, unitId, existingUnitCountWithSameIdentifier);
                unitIdentifiersBuilder.Add(unitIdentifier);
            }

            return unitIdentifiersBuilder.ToValueArray();
        }

        private static ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> CreateFakePlayerUnitPlacements(GameDatabase database, string combatId, ValueArray<CombatUnitUniqueIdentifier> playerUnitIdentifiers)
        {
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo>.Builder unitPlacementMapBuilder = ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo>.CreateBuilder();

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatId);
            ValueArray<UnitPlacementOptionInfo> playerUnitPlacementOptions = combatDefinition.PlayerUnitPlacementOptions;
            if (playerUnitPlacementOptions.Count < playerUnitIdentifiers.Count)
            {
                throw new Exception($"Scenario has too few placement options. Has {playerUnitPlacementOptions.Count} but there are {playerUnitIdentifiers.Count} player characters. Scenario Id: ({combatId})");
            }
            if (playerUnitIdentifiers.Count <= 0)
            {
                throw new Exception("Tried to create fake unit placements with no player characters.");
            }

            // Actually place them
            for (int i = 0; i < playerUnitIdentifiers.Count; ++i)
            {
                CombatUnitUniqueIdentifier unitIdentifier = playerUnitIdentifiers[i];
                UnitPlacementOptionInfo placementOption = playerUnitPlacementOptions[i];

                unitPlacementMapBuilder.Add(unitIdentifier, placementOption);
            }

            return unitPlacementMapBuilder.ToValueDictionary();
        }
    }

    internal static class NetworkGameStateRequestTestExtensions
    {
        public static NetworkGameState ApplyChange(this NetworkGameState networkGameState, GameDatabase database, NetworkGameCommand change) => networkGameState.ApplyCommands(database, new ValueArray<NetworkGameCommand>(change)).State;

        public static NetworkGameState ApplyChange(this NetworkGameState networkGameState, GameDatabase database, NetworkGameCommand change, object followingChange, params object[] remainingChanges)
        {
            networkGameState = ApplyOne(networkGameState, database, change);
            networkGameState = ApplyOne(networkGameState, database, followingChange);
            foreach (object remainingChange in remainingChanges)
                networkGameState = ApplyOne(networkGameState, database, remainingChange);
            return networkGameState;
        }

        public static NetworkGameState ApplyChange(this NetworkGameState networkGameState, GameDatabase database, NetworkGameCommandRequest request, params object[] followingChanges)
        {
            networkGameState = ApplyOne(networkGameState, database, request);
            foreach (object followingChange in followingChanges)
                networkGameState = ApplyOne(networkGameState, database, followingChange);
            return networkGameState;
        }

        private static NetworkGameState ApplyOne(NetworkGameState networkGameState, GameDatabase database, object change)
        {
            ValueArray<NetworkGameCommand> commands = change switch
            {
                NetworkGameCommand command => new ValueArray<NetworkGameCommand>(command),
                NetworkGameCommandRequest request => request.ResolveToCommands(database, networkGameState),
                _ => throw new ArgumentException($"Unsupported test change type ({change?.GetType()})", nameof(change))
            };
            return networkGameState.ApplyCommands(database, commands).State;
        }
    }
}
