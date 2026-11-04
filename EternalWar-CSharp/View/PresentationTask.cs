// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Client.Application.View
{
    public interface IReadOnlyPresentationTask
    {
        bool IsFinished { get; }

        IReadOnlyPresentationTask ThenDo(Action finishedCallback);
    }

    public sealed class PresentationTask : IReadOnlyPresentationTask
    {
        public static readonly PresentationTask Instant = new() { IsFinished = true };

        public bool IsFinished
        {
            get => _isFinished;
            set
            {
                _isFinished = value;
                if (!_hasInvokedAlready && value)
                {
                    _hasInvokedAlready = true;
                    _finished?.Invoke();
                }
            }
        }
        private bool _isFinished;
        private Action? _finished;
        private bool _hasInvokedAlready;

        public PresentationTask ThenDo(Action finishedCallback)
        {
            ArgumentNullException.ThrowIfNull(finishedCallback);
            if (_hasInvokedAlready)
            {
                finishedCallback.Invoke();
                return this;
            }
            _finished += finishedCallback;
            return this;
        }
        IReadOnlyPresentationTask IReadOnlyPresentationTask.ThenDo(Action finishedCallback) => ThenDo(finishedCallback);
    }

    internal sealed class PresentationTaskGroup(params IReadOnlyPresentationTask[] tasks) : IReadOnlyPresentationTask
    {
        public bool IsFinished 
        {
            get
            {
                bool isFinished = AreAllFinished(_tasks);
                if (isFinished && !_hasInvokedAlready)
                {
                    _hasInvokedAlready = true;
                    _finished?.Invoke();
                }
                return isFinished;
            }
        }

        private readonly IReadOnlyPresentationTask[] _tasks = tasks;
        private Action? _finished;
        private bool _hasInvokedAlready;

        public PresentationTaskGroup ThenDo(Action finishedCallback)
        {
            ArgumentNullException.ThrowIfNull(finishedCallback);
            if (IsFinished)
            {
                finishedCallback.Invoke();
                return this;
            }
            _finished += finishedCallback;
            return this;
        }
        IReadOnlyPresentationTask IReadOnlyPresentationTask.ThenDo(Action finishedCallback) => ThenDo(finishedCallback);

        private static bool AreAllFinished(IReadOnlyPresentationTask[] tasks)
        {
            foreach (IReadOnlyPresentationTask task in tasks)
            {
                if (!task.IsFinished)
                    return false;
            }
            return true;
        }
    }
}
