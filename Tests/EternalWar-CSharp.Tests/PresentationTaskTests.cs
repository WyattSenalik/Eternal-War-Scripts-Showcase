// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.View;
using NUnit.Framework;
using System;

namespace EternalWar.Client.Application.Tests
{
    [TestFixture]
    internal sealed class PresentationTaskTests
    {
        [Test]
        public void PresentationTaskInvokesCallbacksOnceAndRunsLateCallbacksImmediately()
        {
            PresentationTask task = new();
            int callbacks = 0;
            task.ThenDo(() => callbacks++).ThenDo(() => callbacks += 10);

            task.IsFinished = true;
            task.IsFinished = true;
            task.IsFinished = false;
            task.IsFinished = true;

            Assert.That(callbacks, Is.EqualTo(11));
            task.ThenDo(() => callbacks += 100);
            Assert.That(callbacks, Is.EqualTo(111));
            Assert.That(PresentationTask.Instant.IsFinished, Is.True);

            int instantCallback = 0;
            PresentationTask.Instant.ThenDo(() => instantCallback++);
            Assert.That(instantCallback, Is.EqualTo(1));
        }

        [Test]
        public void PresentationTaskRejectsNullCallback()
        {
            Assert.Throws<ArgumentNullException>(() => new PresentationTask().ThenDo(null!));
        }

        [Test]
        public void TaskGroupAcceptsInterfaceChildrenAndFiresAfterEveryChildFinishes()
        {
            PresentationTask first = new();
            PresentationTask second = new();
            IReadOnlyPresentationTask nested = new PresentationTaskGroup(first, second);
            PresentationTaskGroup group = new(nested, PresentationTask.Instant);
            int callbackCount = 0;
            group.ThenDo(() => callbackCount++);

            Assert.That(group.IsFinished, Is.False);
            first.IsFinished = true;
            Assert.That(group.IsFinished, Is.False);
            second.IsFinished = true;
            Assert.That(group.IsFinished, Is.True);
            Assert.That(group.IsFinished, Is.True);
            Assert.That(callbackCount, Is.EqualTo(1));

            int lateCallbackCount = 0;
            group.ThenDo(() => lateCallbackCount++);
            Assert.That(lateCallbackCount, Is.EqualTo(1));
        }

        [Test]
        public void EmptyTaskGroupIsFinishedAndRunsLateCallbacksImmediately()
        {
            PresentationTaskGroup group = new();
            Assert.That(group.IsFinished, Is.True);
            int callbackCount = 0;
            group.ThenDo(() => callbackCount++);
            Assert.That(callbackCount, Is.EqualTo(1));
            Assert.Throws<ArgumentNullException>(() => group.ThenDo(null!));
        }

        [Test]
        public void ChangeHandledResultsCombineOngoingAndFinishedWork()
        {
            ChangeHandledResult finished = ChangeHandledResult_Finished.Instance;
            PresentationTask ongoingTask = new();
            ChangeHandledResult ongoing = new ChangeHandledResult_OngoingTask(ongoingTask);
            ChangeHandledResult_Composite composite = finished.Combine(ongoing, finished);

            Assert.That(finished.CanBeConsumed, Is.True);
            Assert.That(ongoing.CanBeConsumed, Is.False);
            Assert.That(composite.CanBeConsumed, Is.False);
            ongoingTask.IsFinished = true;
            Assert.That(ongoing.CanBeConsumed, Is.True);
            Assert.That(composite.CanBeConsumed, Is.True);
            Assert.That(new ChangeHandledResult_Composite().CanBeConsumed, Is.True);
        }
    }
}
