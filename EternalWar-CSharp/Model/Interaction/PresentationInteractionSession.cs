// Original Authors - Wyatt Senalik
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Client.Application.Model.Interaction
{
    public sealed class PresentationInteractionSession
    {
        public InteractionState PredictedInteractionState { get; private set; } = InteractionState.Initial;
        public InteractionState CurrentVisualizedState { get; private set; } = InteractionState.Initial;

        private readonly Queue<InteractionStateChange> _unvisualizedChanges = new();

        public void ApplyChange(InteractionStateChange change)
        {
            _unvisualizedChanges.Enqueue(change);
            PredictedInteractionState = change.Apply(PredictedInteractionState);
        }

        public bool TryPeekVisualizationChange([NotNullWhen(true)] out InteractionStateChange? visualizationChange)
        {
            return _unvisualizedChanges.TryPeek(out visualizationChange);
        }

        public void ConsumeVisualizationChange()
        {
            InteractionStateChange change = _unvisualizedChanges.Dequeue();
            CurrentVisualizedState = change.Apply(CurrentVisualizedState);
        }

        public void Reset()
        {
            PredictedInteractionState = InteractionState.Initial;
            CurrentVisualizedState = InteractionState.Initial;
            _unvisualizedChanges.Clear();
        }
    }
}
