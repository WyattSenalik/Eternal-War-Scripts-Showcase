using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.StateMachine;
using EternalWar.Core.Model.Combat.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class HelperEdgeCaseTests
    {
        [Test]
        public void CircularBuffer_EmptyAndWrappedReadsPreserveLogicalOrder()
        {
            CircularBuffer<int> buffer = new(3);

            Assert.That(buffer.Count, Is.Zero);
            Assert.That(buffer.Capacity, Is.EqualTo(3));
            Assert.That(buffer.FindIndex(value => value == 1), Is.EqualTo(-1));

            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4);

            Assert.That(buffer.Count, Is.EqualTo(3));
            Assert.That(buffer[0], Is.EqualTo(2));
            Assert.That(buffer[1], Is.EqualTo(3));
            Assert.That(buffer[2], Is.EqualTo(4));
            Assert.That(buffer.FindIndex(value => value == 3), Is.EqualTo(1));
            Assert.That(buffer.Contains(2), Is.True);
            Assert.That(buffer.Contains(1), Is.False);

            buffer.Clear();
            Assert.That(buffer.Count, Is.Zero);
            Assert.That(buffer.FindIndex(value => true), Is.EqualTo(-1));
        }

        [Test]
        public void CircularBuffer_ContainsHandlesNullItems()
        {
            CircularBuffer<string?> buffer = new(2);
            buffer.Add(null);
            buffer.Add("value");

            Assert.That(buffer.Contains(null), Is.True);
            Assert.That(buffer.Contains("value"), Is.True);
            Assert.That(buffer.Contains("missing"), Is.False);
        }

        [Test]
        public void CircularBuffer_RejectsInvalidCapacityIndicesAndPredicate()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CircularBuffer<int>(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CircularBuffer<int>(-1));
            CircularBuffer<int> buffer = new(2);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = buffer[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer[0] = 1);
            Assert.Throws<ArgumentNullException>(() => buffer.FindIndex(null!));

            buffer.Add(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = buffer[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = buffer[1]);
        }

        [TestCase(0u, 0.0f)]
        [TestCase(1u, 0.5f)]
        [TestCase(2u, 0.25f)]
        [TestCase(3u, 0.75f)]
        [TestCase(4u, 0.125f)]
        public void VanDerCorput_ProducesExpectedBinarySequence(uint index, float expected)
        {
            Assert.That(MathHelpers.VanDerCorput(index), Is.EqualTo(expected));
        }

        [TestCase(0u)]
        [TestCase(1u)]
        public void VanDerCorput_RejectsNonProgressingBases(uint invalidBase)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MathHelpers.VanDerCorput(3, invalidBase));
        }

        [Test]
        public void VanDerCorput_SupportsAlternateBases()
        {
            Assert.That(MathHelpers.VanDerCorput(1, 3), Is.EqualTo(1.0f / 3.0f).Within(0.00001f));
            Assert.That(MathHelpers.VanDerCorput(2, 3), Is.EqualTo(2.0f / 3.0f).Within(0.00001f));
        }

        [TestCase(EightDirection.None, EightDirection.None, new float[] { 0, 0 }, new int[] { 0, 0 })]
        [TestCase(EightDirection.Left, EightDirection.Right, new float[] { -1, 0 }, new int[] { -1, 0 })]
        [TestCase(EightDirection.Right, EightDirection.Left, new float[] { 1, 0 }, new int[] { 1, 0 })]
        [TestCase(EightDirection.Up, EightDirection.Down, new float[] { 0, 1 }, new int[] { 0, 1 })]
        [TestCase(EightDirection.Down, EightDirection.Up, new float[] { 0, -1 }, new int[] { 0, -1 })]
        [TestCase(EightDirection.UpLeft, EightDirection.DownRight, new float[] { -1, 1 }, new int[] { -1, 1 })]
        [TestCase(EightDirection.UpRight, EightDirection.DownLeft, new float[] { 1, 1 }, new int[] { 1, 1 })]
        [TestCase(EightDirection.DownLeft, EightDirection.UpRight, new float[] { -1, -1 }, new int[] { -1, -1 })]
        [TestCase(EightDirection.DownRight, EightDirection.UpLeft, new float[] { 1, -1 }, new int[] { 1, -1 })]
        public void EightDirection_MapsEveryDirection(EightDirection direction, EightDirection opposite, float[] vector, int[] intVector)
        {
            Assert.That(direction.GetOpposite(), Is.EqualTo(opposite));
            Assert.That(direction.GetVector2(), Is.EqualTo(new Vector2(vector[0], vector[1])));
            Assert.That(direction.GetVector2Int(), Is.EqualTo(new Vector2Int(intVector[0], intVector[1])));
            Assert.That(direction.IsDiagonal(), Is.EqualTo(Math.Abs((int)direction) is 2 or 4));
        }

        [Test]
        public void EightDirection_ComponentExtractionAndAddCoverCardinalAndDiagonalCases()
        {
            EightDirection.UpLeft.GetComponentDirections(out EightDirection horizontal, out EightDirection vertical);
            Assert.That(horizontal, Is.EqualTo(EightDirection.Left));
            Assert.That(vertical, Is.EqualTo(EightDirection.Up));
            Assert.That(EightDirection.None.Add(EightDirection.Right), Is.EqualTo(EightDirection.Right));
            Assert.That(EightDirection.Left.Add(EightDirection.Right), Is.EqualTo(EightDirection.None));
            Assert.That(EightDirection.UpLeft.Add(EightDirection.Right), Is.EqualTo(EightDirection.Up));
            Assert.That(EightDirection.UpLeft.Add(EightDirection.Down), Is.EqualTo(EightDirection.Left));
            Assert.That(EightDirection.UpRight.Add(EightDirection.DownLeft), Is.EqualTo(EightDirection.None));
            Assert.That(EightDirection.UpLeft.ToCardinal().ToEightDirection(), Is.EqualTo(EightDirection.UpLeft));
        }

        [Test]
        public void EightDirection_ClosestDirectionHonorsCutoffAndIntegerOverload()
        {
            Assert.That(new Vector2(0.05f, 0).GetClosestEightDirection(), Is.EqualTo(EightDirection.None));
            Assert.That(new Vector2(0.051f, -0.051f).GetClosestEightDirection(), Is.EqualTo(EightDirection.DownRight));
            Assert.That(new Vector2Int(1, 1).GetClosestEightDirection(), Is.EqualTo(EightDirection.UpRight));
            Assert.Throws<NotImplementedException>(() => ((EightDirection)99).GetVector2());
            Assert.Throws<NotImplementedException>(() => ((EightDirection)99).GetVector2Int());
            Assert.Throws<NotImplementedException>(() => ((EightDirection)99).IsDiagonal());
        }

        [Test]
        public void GridPosition_DistanceAndInterpolationHandleEndpointsAndMidpoints()
        {
            GridPosition start = new(0, 0);
            GridPosition end = new(3, 4);

            Assert.That(start.DistanceTo(end, DistanceType.Manhattan), Is.EqualTo(7));
            Assert.That(start.DistanceTo(end, DistanceType.Chebyshev), Is.EqualTo(4));
            Assert.That(start.DistanceTo(end, DistanceType.Euclidean), Is.EqualTo(5));
            Assert.That(start.Interpolate(end, -1), Is.EqualTo(start));
            Assert.That(start.Interpolate(end, 0.5f), Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(start.Interpolate(end, 2), Is.EqualTo(end));
            Assert.That(start.Add(2, 3), Is.EqualTo(new GridPosition(2, 3)));
            Assert.That(start.GetAdjacentGridPositions(), Is.EqualTo(new[] { new GridPosition(-1, 0), new GridPosition(1, 0), new GridPosition(0, -1), new GridPosition(0, 1) }));
            Assert.Throws<NotImplementedException>(() => start.DistanceTo(end, (DistanceType)99));
        }

        [Test]
        public void GridPosition_CenterUsesIntegerDivisionAndRejectsEmptyInput()
        {
            Assert.That(new[] { new GridPosition(0, 0), new GridPosition(3, 3) }.CalculateCenterPosition(), Is.EqualTo(new GridPosition(1, 1)));
            Assert.Throws<DivideByZeroException>(() => Array.Empty<GridPosition>().CalculateCenterPosition());
        }

        [Test]
        public void VectorHelpersHandleZeroAndRoundMidpointsAwayFromZero()
        {
            Assert.That(Vector2.Zero.Normalized(), Is.EqualTo(Vector2.Zero));
            Assert.That(new Vector2(3, 4).Normalized(), Is.EqualTo(new Vector2(0.6f, 0.8f)));
            Assert.That(new Vector2(1.5f, -1.5f).RoundToInt(), Is.EqualTo(new Vector2Int(2, -2)));
            Assert.That(new Vector2Int(3, 4).Length(), Is.EqualTo(5));
            Assert.That(new Vector2Int(3, 4).LengthSquared(), Is.EqualTo(25));
            Assert.That((new Vector2Int(1, 2) + new Vector2Int(3, 4)), Is.EqualTo(new Vector2Int(4, 6)));
            Assert.That((new Vector2Int(3, 4) - new Vector2Int(1, 2)), Is.EqualTo(new Vector2Int(2, 2)));
            Assert.That(new Vector2Int(3, 4) * 2, Is.EqualTo(new Vector2Int(6, 8)));
            Assert.That(2 * new Vector2Int(3, 4), Is.EqualTo(new Vector2Int(6, 8)));
            Assert.That(new Vector2Int(3, 4) / 2, Is.EqualTo(new Vector2Int(1, 2)));
        }

        [Test]
        public void NumericPredicatesCoverComparisonsNestingAndEmptyComposites()
        {
            Assert.That(new NumericPredicate_LessThan(5).Evaluate(4), Is.True);
            Assert.That(new NumericPredicate_GreaterThan(5).Evaluate(6), Is.True);
            Assert.That(new NumericPredicate_LessThanOrEqualTo(5).Evaluate(5), Is.True);
            Assert.That(new NumericPredicate_GreaterThanOrEqualTo(5).Evaluate(5), Is.True);
            Assert.That(new NumericPredicate_Equal(5).Evaluate(5), Is.True);
            Assert.That(new NumericPredicate_Not(new NumericPredicate_Equal(5)).Evaluate(4), Is.True);
            Assert.That(new NumericPredicate_And(new(new NumericPredicate_GreaterThan(1), new NumericPredicate_LessThan(10))).Evaluate(5), Is.True);
            Assert.That(new NumericPredicate_And(ValueArray<NumericPredicate>.Empty).Evaluate(5), Is.True);
            Assert.That(new NumericPredicate_Or(new(new NumericPredicate_Equal(1), new NumericPredicate_Equal(5))).Evaluate(5), Is.True);
            Assert.That(new NumericPredicate_Or(ValueArray<NumericPredicate>.Empty).Evaluate(5), Is.False);
        }

        [Test]
        public void ValueAmountsEvaluateFixedPercentAndCombinedValues()
        {
            Assert.That(new ValueAmount_Percentage(0.25f).Evaluate(20), Is.EqualTo(5));
            Assert.That(new ValueAmount_Fixed(7).Evaluate(20), Is.EqualTo(7));
            Assert.That(new ValueAmount_PercentagePlusFixed(0.25f, 2).Evaluate(20), Is.EqualTo(7));
        }

        [Test]
        public void RestrictedRotationCoversAllValuesAndRejectsUnknownValue()
        {
            Assert.That(Rotation_Restricted_Four._0.GetEulerAngle(), Is.Zero);
            Assert.That(Rotation_Restricted_Four._90.GetEulerAngle(), Is.EqualTo(90));
            Assert.That(Rotation_Restricted_Four._180.GetEulerAngle(), Is.EqualTo(180));
            Assert.That(Rotation_Restricted_Four._270.GetEulerAngle(), Is.EqualTo(270));
            Assert.Throws<NotImplementedException>(() => ((Rotation_Restricted_Four)99).GetEulerAngle());
        }

        [Test]
        public void ReadOnlyListIndexOfFindsFirstMatchAndReturnsMinusOneWhenMissing()
        {
            IReadOnlyList<string?> list = new string?[] { "first", null, "last" };
            Assert.That(list.IndexOf(null), Is.EqualTo(1));
            Assert.That(list.IndexOf("last"), Is.EqualTo(2));
            Assert.That(list.IndexOf("missing"), Is.EqualTo(-1));
        }

        [Test]
        public void StateMachineHonorsConnectionDirectionDuplicateKeysAndLifecycle()
        {
            TestStateMachine machine = new();
            TestState a = new("a");
            TestState b = new("b");
            TestState c = new("c");
            machine.AddState("a", a);
            machine.AddState("a", c);
            machine.AddState("b", b);
            machine.AddConnection(a, b, StateConnectionType.OneWay);

            Assert.That(machine.GetState("a"), Is.SameAs(a));
            Assert.That(machine.TryChangeState("missing"), Is.False);
            Assert.That(machine.TryChangeState("a", "init"), Is.True);
            Assert.That(a.EnterCount, Is.EqualTo(1));
            Assert.That(machine.TryChangeState(a), Is.False);
            Assert.That(machine.TryChangeState(c), Is.False);
            Assert.That(machine.TryChangeState("b", "next"), Is.True);
            Assert.That(a.ExitCount, Is.EqualTo(1));
            Assert.That(machine.PreviousState, Is.SameAs(a));
            Assert.That(b.LastArguments, Is.EqualTo("next"));
            Assert.That(machine.TryChangeState(a), Is.False);
        }

        [Test]
        public void StateMachineCanAddReverseTransitionAndRejectUnknownConnectionTypes()
        {
            TestStateMachine machine = new();
            TestState a = new("a");
            TestState b = new("b");
            machine.AddState("a", a);
            machine.AddState("b", b);
            machine.AddConnection(a, b, StateConnectionType.IncludeReverse);
            Assert.That(machine.TryChangeState(a), Is.True);
            Assert.That(machine.TryChangeState(b), Is.True);
            Assert.That(machine.TryChangeState(a), Is.True);
            Assert.Throws<NotImplementedException>(() => machine.AddConnection(a, b, (StateConnectionType)99));
        }

        private sealed class TestStateMachine : StateMachine<TestState> { }

        private sealed class TestState(string id) : IState
        {
            public int EnterCount { get; private set; }
            public int ExitCount { get; private set; }
            public object? LastArguments { get; private set; }

            public void Enter(object? arguments)
            {
                EnterCount++;
                LastArguments = arguments;
            }

            public void Exit() => ExitCount++;

            public override bool Equals(object? obj) => obj is TestState state && stateId == state.stateId;
            private readonly string stateId = id;
            public override int GetHashCode() => stateId.GetHashCode(StringComparison.Ordinal);
        }
    }
}
