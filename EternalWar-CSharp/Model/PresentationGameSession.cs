// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EternalWar.Client.Application.Model
{
    public enum ApplyAuthoritativeCommandBatchResult
    {
        Success,
        AlreadyProcessed,
        SavedForFutureProcessing,
        RequiresReconciliation
    }

    internal sealed record PendingAuthoritativeCommandBatch(DatedNetworkGameCommandBatch DatedCommandBatch, ChangeRequestId? ChangeRequestId);
    internal sealed record VisualizedNetworkGameCommand(NetworkGameCommand? Command, ChangeRequestId? LocalChangeId);
    internal sealed record PresentationStep(NetworkGameCommand? Command, NetworkGameEvent? Event);

    internal sealed class UnacknowledgedCommandSnapshot(ValueArray<(long Sequence, DatedNetworkGameCommandBatch Batch)> unvisualizedAuthoritativeBatches, ValueArray<ChangeRequest> unvisualizedLocalCommands, NetworkGameState currentGameState)
    {
        public ValueArray<(long Sequence, DatedNetworkGameCommandBatch Batch)> UnvisualizedAuthoritativeBatches { get; } = unvisualizedAuthoritativeBatches;
        public ValueArray<ChangeRequest> UnvisualizedLocalCommands { get; } = unvisualizedLocalCommands;
        public NetworkGameState CurrentGameState { get; } = currentGameState;
        public IReadOnlyList<VisualizedNetworkGameCommand> VisualizedCommandsSinceSnapshot => _visualizedCommandsSinceSnapshot;

        private readonly List<VisualizedNetworkGameCommand> _visualizedCommandsSinceSnapshot = new();

        public void AddVisualizedCommand(NetworkGameCommand? command, ChangeRequestId? localChangeId) => _visualizedCommandsSinceSnapshot.Add(new(command, localChangeId));

        public bool RemoveVisualizedCommand(ChangeRequestId requestId)
        {
            bool didRemoveAny = false;
            for (int i = _visualizedCommandsSinceSnapshot.Count - 1; i >= 0; --i)
            {
                if (_visualizedCommandsSinceSnapshot[i].LocalChangeId != requestId)
                    continue;
                _visualizedCommandsSinceSnapshot.RemoveAt(i);
                didRemoveAny = true;
            }
            return didRemoveAny;
        }
    }

    internal enum VisualizationCommandSourceList { Authoritative, Local }

    internal sealed class PresentationCommandBatch(IReadOnlyList<PresentationStep> steps, ChangeRequestId? localChangeId)
    {
        public IReadOnlyList<PresentationStep> Steps { get; } = steps;
        public ChangeRequestId? LocalChangeId { get; } = localChangeId;
        public int NextStepIndex { get; private set; }
        public bool HasNextStep => NextStepIndex < Steps.Count;
        public PresentationStep NextStep => Steps[NextStepIndex];

        public static PresentationCommandBatch FromCommands(GameDatabase database, NetworkGameState initialState, IEnumerable<NetworkGameCommand> commands, ChangeRequestId? localChangeId)
        {
            NetworkGameState state = initialState;
            using ValueArray<PresentationStep>.Builder steps = new();
            foreach (NetworkGameCommand command in commands)
            {
                NetworkGameCommandApplyResult result = state.ApplyCommand(database, command);
                state = result.State;
                bool isFirstEvent = true;
                foreach (NetworkGameEvent gameEvent in result.Events)
                {
                    steps.Add(new PresentationStep(isFirstEvent ? command : null, gameEvent));
                    isFirstEvent = false;
                }
                if (isFirstEvent)
                    steps.Add(new PresentationStep(command, null));
            }
            if (state.TryGetCombatState(out CombatState? combatState))
            {
                NetworkGameEvent? endEvent = combatState.DeriveStatus() switch
                {
                    CombatStatus_Victory => new NetworkGameEvent_Run_Combat_Victory(),
                    CombatStatus_Defeat => new NetworkGameEvent_Run_Combat_Defeat(),
                    _ => null
                };
                if (endEvent != null)
                    steps.Add(new PresentationStep(null, endEvent));
            }
            return new PresentationCommandBatch(steps.ToValueArray(), localChangeId);
        }

        public void ConsumeNextStep() => ++NextStepIndex;

        public IEnumerable<PresentationStep> GetUnconsumedSteps()
        {
            for (int i = NextStepIndex; i < Steps.Count; ++i)
                yield return Steps[i];
        }
    }

    internal sealed class PresentationGameSession(GameDatabase database, DatedNetworkGameState initialState)
    {
        public const int RECENT_COMMAND_BATCH_COUNT_TO_HOLD = 32;

        public GameDatabase Database { get; } = database;
        /// <summary>
        /// Most recent state confirmed by the server.
        /// </summary>
        public DatedNetworkGameState AuthoritativeGameState { get; private set; } = initialState;
        /// <summary>
        /// Predicted state after applying every pending local change.
        /// AuthoritativeGameState + PendingLocalChanges.
        /// </summary>
        public NetworkGameState PredictedGameState { get; private set; } = initialState.NetworkGameState;
        /// <summary>
        /// Current state being presented by the game views. Approaches <see cref="PredictedGameState"/> as changes are consumed.
        /// May be behind AuthoritativeGameState or further than it.
        /// CurrentGameState + UnvisualizedChanges = PredictedGameState.
        /// </summary>
        public NetworkGameState CurrentGameState { get; private set; } = initialState.NetworkGameState;

        // Changes that are yet to be approved by the server.
        private readonly List<ChangeRequest> _pendingLocalRequests = new();
        // Univisualized changes haven't been visualized by the client yet.
        // CurrentGameState + UnvisualizedAuthoritativeChanges = AuthoritativeGameState when CurrentGameState < AuthoritativeGameState.
        private readonly SortedList<long, DatedNetworkGameCommandBatch> _unvisualizedAuthoritativeBatches = new();
        // CurrentGameState + UnvisualizedLocalChanges = PredictedGameState when CurrentGameState >= AuthoritativeGameState.
        private readonly List<ChangeRequest> _unvisualizedLocalRequests = new();
        // If we receive a change that is in the future, we should hold onto it until we receive the next one we need.
        private readonly SortedDictionary<long, PendingAuthoritativeCommandBatch> _pendingAuthoritativeBatches = new();
        private readonly DatedNetworkGameState _initialState = initialState;
        private readonly CircularBuffer<DatedNetworkGameCommandBatch> _recentlyReceivedAuthoritativeBatches = new(RECENT_COMMAND_BATCH_COUNT_TO_HOLD);
        private readonly Dictionary<ChangeRequestId, UnacknowledgedCommandSnapshot> _unacknowledgedVisualCommandsAppliedSnapshots = new();

        private PresentationCommandBatch? _commandBatchCurrentlyBeingPresented;

        public PresentationGameSession(GameDatabase database) : this(database, DatedNetworkGameState.InitialState) { }

        public void OverwriteAuthoritativeState(DatedNetworkGameState newAuthoritativeDatedGameState)
        {
            Reset();
            AuthoritativeGameState = newAuthoritativeDatedGameState;
            PredictedGameState = AuthoritativeGameState.NetworkGameState;
            CurrentGameState = AuthoritativeGameState.NetworkGameState;
        }

        public ApplyAuthoritativeCommandBatchResult ApplyAuthoritativeCommandBatch(DatedNetworkGameCommandBatch datedCommandBatch, ChangeRequestId? changeRequestId)
        {
            long expectedSequence = AuthoritativeGameState.Sequence + 1;
            long actualSequence = datedCommandBatch.Sequence;
            if (actualSequence < expectedSequence)
            {
                // Already processed
                int foundIndex = _recentlyReceivedAuthoritativeBatches.FindIndex(batch => batch.Sequence == actualSequence);
                if (foundIndex >= 0)
                {
                    DatedNetworkGameCommandBatch previousBatch = _recentlyReceivedAuthoritativeBatches[foundIndex];
                    if (datedCommandBatch != previousBatch)
                        throw new AlreadyProcessedAuthoritativeCommandBatchDivergenceException(previousBatch, datedCommandBatch);
                    // Is the same change, just a duplicate message. That's fine, just ignore.
                    return ApplyAuthoritativeCommandBatchResult.AlreadyProcessed;
                }
                throw new TooOldCommandBatchException(datedCommandBatch);
            }
            if (actualSequence > expectedSequence)
            {
                if (_pendingAuthoritativeBatches.TryGetValue(actualSequence, out PendingAuthoritativeCommandBatch? existingBatch))
                {
                    if (datedCommandBatch != existingBatch.DatedCommandBatch)
                        throw new AlreadyPendingAuthoritativeCommandBatchDivergenceException(existingBatch, datedCommandBatch);
                    return ApplyAuthoritativeCommandBatchResult.SavedForFutureProcessing;
                }
                // Save it for later processing
                _pendingAuthoritativeBatches.Add(actualSequence, new(datedCommandBatch, changeRequestId));
                _recentlyReceivedAuthoritativeBatches.Add(datedCommandBatch);
                return ApplyAuthoritativeCommandBatchResult.SavedForFutureProcessing;
            }

            ChangeRequest? acknowledgedLocalRequest = null;
            if (changeRequestId != null)
            {
                // Try to find the acknowledgedLocalChange remove it from pending local (it's confirmed now).
                int foundLocalIndex = _pendingLocalRequests.FindIndex(localRequest => localRequest.LocalChangeId == changeRequestId);
                if (foundLocalIndex >= 0)
                {
                    acknowledgedLocalRequest = _pendingLocalRequests[foundLocalIndex];
                    ValueArray<NetworkGameCommand> expectedCommands = acknowledgedLocalRequest.Request.ResolveToCommands(Database, AuthoritativeGameState.NetworkGameState);
                    if (!expectedCommands.SequenceEqual(datedCommandBatch.Commands))
                        throw new AcknowledgedCommandBatchDivergenceException(acknowledgedLocalRequest, datedCommandBatch);

                    // Remove unacknowledged snapshot if we already applied it.
                    _unacknowledgedVisualCommandsAppliedSnapshots.Remove(changeRequestId);
                    _pendingLocalRequests.RemoveAt(foundLocalIndex);

                    // If we haven't visualized the local change, get rid of it and have it be the next authoritative change we make
                    if (_unvisualizedLocalRequests.Remove(acknowledgedLocalRequest))
                        _unvisualizedAuthoritativeBatches.Add(expectedSequence, datedCommandBatch);
                }
            }

            // The change was not one we requested, so add it to the unvisualized authoritative changes
            if (acknowledgedLocalRequest == null)
                _unvisualizedAuthoritativeBatches.Add(expectedSequence, datedCommandBatch);

            NetworkGameCommandBatchApplyResult applyResult = AuthoritativeGameState.NetworkGameState.ApplyCommands(Database, datedCommandBatch.Commands);
            AuthoritativeGameState = new DatedNetworkGameState(actualSequence, applyResult.State);
            RebuildPredictedState();

            if (!_recentlyReceivedAuthoritativeBatches.Contains(datedCommandBatch))
                _recentlyReceivedAuthoritativeBatches.Add(datedCommandBatch);

            // Check if the state needs to be reconciled because it got out of sync in a meaningful way.
            if (DoesCurrentStateRequireReconciliation())
                return ApplyAuthoritativeCommandBatchResult.RequiresReconciliation;

            long nextExpectedSequence = expectedSequence + 1;
            if (!_pendingAuthoritativeBatches.TryGetValue(nextExpectedSequence, out PendingAuthoritativeCommandBatch? nextBatch))
                return ApplyAuthoritativeCommandBatchResult.Success;

            _pendingAuthoritativeBatches.Remove(nextExpectedSequence);
            return ApplyAuthoritativeCommandBatch(nextBatch.DatedCommandBatch, nextBatch.ChangeRequestId);
        }

        public ChangeRequest ApplyLocalCommandRequest(NetworkGameCommandRequest request, Guid myClientGuid)
        {
            ChangeRequest localRequest = new(request, ChangeRequestId.New(myClientGuid));
            _pendingLocalRequests.Add(localRequest);
            _unvisualizedLocalRequests.Add(localRequest);
            RebuildPredictedState();
            return localRequest;
        }

        public void DenyCommandRequest(ChangeRequestId changeRequestId)
        {
            int foundIndex = _pendingLocalRequests.FindIndex(localRequest => localRequest.LocalChangeId == changeRequestId);
            if (foundIndex < 0)
            {
                Debug.PrintError($"Tried to deny a command request that wasn't pending? ({changeRequestId})");
                return;
            }
            ChangeRequest deniedRequest = _pendingLocalRequests[foundIndex];
            _pendingLocalRequests.RemoveAt(foundIndex);

            // Update snapshots to know about this denial
            foreach (UnacknowledgedCommandSnapshot snapshot in _unacknowledgedVisualCommandsAppliedSnapshots.Values)
            {
                snapshot.RemoveVisualizedCommand(changeRequestId);
            }

            RebuildPredictedState();

            // If the change has not been visualized yet, we are all good, no need to reconcile.
            if (_unvisualizedLocalRequests.Remove(deniedRequest))
                return;

            // Otherwise, the change has already been visualized, need to reconcile that.
            if (!_unacknowledgedVisualCommandsAppliedSnapshots.TryGetValue(changeRequestId, out UnacknowledgedCommandSnapshot? foundSnapshot))
                throw new KeyNotFoundException($"Could not find ({changeRequestId}) as a key for a snapshot");
            ApplySnapshot(foundSnapshot);
        }

        public bool TryPeekVisualizationEvent([NotNullWhen(true)] out NetworkGameEvent? visualizationEvent)
        {
            while (true)
            {
                if (_commandBatchCurrentlyBeingPresented == null || !_commandBatchCurrentlyBeingPresented.HasNextStep)
                {
                    if (!TryStartNextCommandBatch())
                    {
                        visualizationEvent = null;
                        return false;
                    }
                }

                PresentationCommandBatch currentBatch = _commandBatchCurrentlyBeingPresented
                    ?? throw new InvalidOperationException("Expected an active command batch while inspecting presentation events.");
                PresentationStep nextStep = currentBatch.NextStep;
                if (nextStep.Event != null)
                {
                    visualizationEvent = nextStep.Event;
                    return true;
                }

                // Commands with no observer output still advance presentation state in order.
                ConsumeNextPresentationStep();
            }
        }

        public void ConsumeVisualizationEvent()
        {
            if (!TryPeekVisualizationEvent(out _))
                throw new EmptyConsumeVisualizationException();
            ConsumeNextPresentationStep();
        }

        private bool TryStartNextCommandBatch()
        {
            if (!TryDetermineNextCommandBatch(out DatedNetworkGameCommandBatch? nextAuthoritativeBatch, out ChangeRequest? nextLocalRequest, out VisualizationCommandSourceList sourceList))
                return false;
            switch (sourceList)
            {
                case VisualizationCommandSourceList.Authoritative:
                    _unvisualizedAuthoritativeBatches.RemoveAt(0);
                    break;
                case VisualizationCommandSourceList.Local:
                    _unvisualizedLocalRequests.RemoveAt(0);
                    break;
                default:
                    throw new NotImplementedException($"Unhandled {nameof(VisualizationCommandSourceList)}: ({sourceList})");
            }
            _commandBatchCurrentlyBeingPresented = CreatePresentationCommandBatch(nextAuthoritativeBatch, nextLocalRequest);
            return true;
        }

        private void ConsumeNextPresentationStep()
        {
            PresentationCommandBatch currentBatch = _commandBatchCurrentlyBeingPresented
                ?? throw new InvalidOperationException("Cannot consume a presentation step without an active command batch.");
            ChangeRequestId? sourceRequestId = currentBatch.LocalChangeId;
            PresentationStep nextStep = currentBatch.NextStep;
            currentBatch.ConsumeNextStep();

            foreach (UnacknowledgedCommandSnapshot snapshot in _unacknowledgedVisualCommandsAppliedSnapshots.Values)
                snapshot.AddVisualizedCommand(nextStep.Command, sourceRequestId);
            if (sourceRequestId != null && !_unacknowledgedVisualCommandsAppliedSnapshots.ContainsKey(sourceRequestId))
            {
                bool isAcknowledged = !_pendingLocalRequests.Any(request => request.LocalChangeId == sourceRequestId);
                if (!isAcknowledged)
                {
                    ValueArray<(long, DatedNetworkGameCommandBatch)> copiedAuthoritativeBatches = new(_unvisualizedAuthoritativeBatches.Select(kvp => (kvp.Key, kvp.Value)));
                    ValueArray<ChangeRequest> copiedLocalRequests = new(_unvisualizedLocalRequests);
                    _unacknowledgedVisualCommandsAppliedSnapshots.Add(sourceRequestId, new(copiedAuthoritativeBatches, copiedLocalRequests, CurrentGameState));
                }
            }

            if (nextStep.Command != null)
                CurrentGameState = CurrentGameState.ApplyCommand(Database, nextStep.Command).State;
        }

        public void Reset()
        {
            AuthoritativeGameState = _initialState;
            PredictedGameState = _initialState.NetworkGameState;
            CurrentGameState = _initialState.NetworkGameState;

            _commandBatchCurrentlyBeingPresented = null;

            _pendingLocalRequests.Clear();
            _unvisualizedAuthoritativeBatches.Clear();
            _unvisualizedLocalRequests.Clear();
            _pendingAuthoritativeBatches.Clear();
            _recentlyReceivedAuthoritativeBatches.Clear();
            _unacknowledgedVisualCommandsAppliedSnapshots.Clear();
        }

        private bool TryDetermineNextCommandBatch(out DatedNetworkGameCommandBatch? authoritativeBatch, out ChangeRequest? localRequest, out VisualizationCommandSourceList sourceList)
        {
            if (_unvisualizedAuthoritativeBatches.Count > 0)
            {
                authoritativeBatch = _unvisualizedAuthoritativeBatches.Values[0];
                localRequest = null;
                sourceList = VisualizationCommandSourceList.Authoritative;
                return true;
            }
            if (_unvisualizedLocalRequests.Count > 0)
            {
                authoritativeBatch = null;
                localRequest = _unvisualizedLocalRequests[0];
                sourceList = VisualizationCommandSourceList.Local;
                return true;
            }
            authoritativeBatch = null;
            localRequest = null;
            sourceList = default;
            return false;
        }

        private void RebuildPredictedState()
        {
            NetworkGameState state = AuthoritativeGameState.NetworkGameState;
            foreach (ChangeRequest localRequest in _pendingLocalRequests)
            {
                NetworkGameCommandRequest commandRequest = localRequest.Request;
                ValueArray<NetworkGameCommand> commands = commandRequest.ResolveToCommands(Database, state);
                NetworkGameCommandBatchApplyResult result = state.ApplyCommands(Database, commands);
                state = result.State;
            }
            PredictedGameState = state;
        }

        private bool DoesCurrentStateRequireReconciliation()
        {
            NetworkGameState state = CurrentGameState;
            if (_commandBatchCurrentlyBeingPresented != null)
            {
                foreach (PresentationStep remainingStep in _commandBatchCurrentlyBeingPresented.GetUnconsumedSteps())
                {
                    NetworkGameCommand? command = remainingStep.Command;
                    if (command != null)
                    {
                        NetworkGameCommandApplyResult result = state.ApplyCommand(Database, command);
                        state = result.State;
                    }
                }
            }
            foreach (DatedNetworkGameCommandBatch authoritativeBatch in _unvisualizedAuthoritativeBatches.Values)
            {
                NetworkGameCommandBatchApplyResult result = state.ApplyCommands(Database, authoritativeBatch.Commands);
                state = result.State;
            }
            foreach (ChangeRequest localRequest in _unvisualizedLocalRequests)
            {
                NetworkGameCommandRequest commandRequest = localRequest.Request;
                ValueArray<NetworkGameCommand> commands = commandRequest.ResolveToCommands(Database, state);
                NetworkGameCommandBatchApplyResult result = state.ApplyCommands(Database, commands);
                state = result.State;
            }
            return state != PredictedGameState;
        }

        private void ApplySnapshot(UnacknowledgedCommandSnapshot snapshot)
        {
            // Authoritative Changes
            foreach ((long sequence, DatedNetworkGameCommandBatch batch) in snapshot.UnvisualizedAuthoritativeBatches)
            {
                if (!_unvisualizedAuthoritativeBatches.ContainsKey(sequence))
                    _unvisualizedAuthoritativeBatches.Add(sequence, batch);
            }

            // Local Changes
            using PooledList<ChangeRequest> currentLocalRequests = new(_unvisualizedLocalRequests);
            using PooledHashSet<ChangeRequest> newLocalRequests = new(snapshot.UnvisualizedLocalCommands);
            _unvisualizedLocalRequests.Clear();
            _unvisualizedLocalRequests.AddRange(snapshot.UnvisualizedLocalCommands);
            foreach (ChangeRequest removedLocalRequest in currentLocalRequests)
            {
                if (!newLocalRequests.Contains(removedLocalRequest))
                    _unvisualizedLocalRequests.Add(removedLocalRequest);
            }

            // Current State
            CurrentGameState = snapshot.CurrentGameState;

            // Apply the changes that were visualized afterwards
            foreach (VisualizedNetworkGameCommand visualizedCommand in snapshot.VisualizedCommandsSinceSnapshot)
            {
                NetworkGameCommand? gameCommand = visualizedCommand.Command;
                if (gameCommand == null)
                    continue;
                NetworkGameCommandApplyResult result = CurrentGameState.ApplyCommand(Database, gameCommand);
                CurrentGameState = result.State;
            }
        }

        private PresentationCommandBatch CreatePresentationCommandBatch(DatedNetworkGameCommandBatch? authoritativeBatch, ChangeRequest? localRequest)
        {
            if (localRequest != null)
            {
                ValueArray<NetworkGameCommand> commands = localRequest.Request.ResolveToCommands(Database, CurrentGameState);
                return PresentationCommandBatch.FromCommands(Database, CurrentGameState, commands, localRequest.LocalChangeId);
            }
            DatedNetworkGameCommandBatch batch = authoritativeBatch ?? throw new ArgumentNullException(nameof(authoritativeBatch));
            return PresentationCommandBatch.FromCommands(Database, CurrentGameState, batch.Commands, null);
        }
    }

    [Serializable]
    internal sealed class AlreadyProcessedAuthoritativeCommandBatchDivergenceException(DatedNetworkGameCommandBatch existingBatch, DatedNetworkGameCommandBatch newBatch) : Exception($"Received a processed command batch that differed from the one already received.\nOld:\n{existingBatch}\n\nNew:\n{newBatch}")
    {
    }

    [Serializable]
    internal sealed class AlreadyPendingAuthoritativeCommandBatchDivergenceException(PendingAuthoritativeCommandBatch pendingBatch, DatedNetworkGameCommandBatch newBatch) : Exception($"Received a pending command batch that differed from the one already received.\nOld:\n{pendingBatch}\n\nNew:\n{newBatch}")
    {
    }

    [Serializable]
    internal sealed class AcknowledgedCommandBatchDivergenceException(ChangeRequest acknowledgedRequest, DatedNetworkGameCommandBatch remoteBatch) : Exception($"The acknowledged local request did not resolve to the authoritative command batch.\nRequest:\n{acknowledgedRequest}\nRemote:\n{remoteBatch}")
    {
    }

    [Serializable]
    internal sealed class TooOldCommandBatchException(DatedNetworkGameCommandBatch datedBatch) : Exception($"Received a very old command batch that wasn't in the recent batch history ({datedBatch})")
    {
    }

    [Serializable]
    internal sealed class EmptyConsumeVisualizationException : Exception
    {
        public EmptyConsumeVisualizationException() : base("Cannot consume visualization event when there are none") { }
    }
}
