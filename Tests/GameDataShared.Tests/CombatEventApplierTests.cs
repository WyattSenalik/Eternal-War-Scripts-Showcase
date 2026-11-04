// User-facing behavior is exercised through command application; observer events are recorded separately.
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Combat.Simulation.Events;
using NUnit.Framework;
using System;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class CombatCommandApplierTests
    {
        private readonly CombatUnitUniqueIdentifier _unitId = new(new CombatUnitOwner_AI(), "Unit", 0);
        private ActionSequenceTimingContext _timing = null!;
        private CombatState _initialState = null!;

        [SetUp]
        public void SetUp()
        {
            _timing = new ActionSequenceTimingContext("action", 0);
            UnitState unit = new(_unitId, TeamAffiliation.Ally, 10, 3, false, new GridPosition(1, 1), EightDirection.Down, ValueArray<PassiveState>.Empty, ValueArray<string>.Empty, ValueArray<TemporaryStatModifierState>.Empty, null);
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder units = ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.CreateBuilder();
            units.Add(_unitId, unit);
            _initialState = new CombatState("combat", units.ToValueDictionary());
        }

        [Test]
        public void StructuralCommandsPreserveTheExistingState()
        {
            CombatCommand[] commands =
            {
                CombatCommand_CompleteAction.Instance,
                new CombatCommand_BeginActionStep(_unitId, _timing),
                new CombatCommand_CompleteActionStep(_unitId, _timing),
                new CombatCommand_PerformAttack(_unitId, _timing, ValueArray<GridPosition>.Empty),
                new CombatCommand_ResolveAttackMiss(_unitId, _timing, _unitId),
                new CombatCommand_NotifyUnitDied(_unitId, _timing, _unitId)
            };

            foreach (CombatCommand command in commands)
            {
                Assert.That(_initialState.ApplyCommand(command), Is.SameAs(_initialState));
            }
        }

        [Test]
        public void ScalarCommandsUpdateActionPointsPositionFacingAndHealth()
        {
            CombatState state = _initialState.ApplyCommand(new CombatCommand_BeginAction(_unitId, 1, 2));
            state = state.ApplyCommand(new CombatCommand_MoveUnit(_unitId, _timing, _unitId, new GridPosition(1, 1), new GridPosition(2, 3), MovementType.Walk, true, true));
            state = state.ApplyCommand(new CombatCommand_ChangeFacing(_unitId, _timing, _unitId, EightDirection.Down, EightDirection.Left));
            state = state.ApplyCommand(new CombatCommand_ResolveAttackHit(_unitId, _timing, _unitId, WasCrit: false, WasFatal: false, DamageType.True, DamageDealt: 4, ResultingHealth: 6));
            state = state.ApplyCommand(new CombatCommand_ApplyHealing(_unitId, _timing, _unitId, _unitId, 3, 9));

            UnitState updated = state.GetUnit(_unitId);
            Assert.That(updated.CurrentActionPoints, Is.EqualTo(2));
            Assert.That(updated.GridPosition, Is.EqualTo(new GridPosition(2, 3)));
            Assert.That(updated.FacingDirection, Is.EqualTo(EightDirection.Left));
            Assert.That(updated.CurrentHealth, Is.EqualTo(9));
        }

        [Test]
        public void ModifierAndPassiveCommandsAddReplaceRemoveAndIgnoreUnknownRemovals()
        {
            TemporaryStatModifierState firstModifier = new(StatType.Strength, 2, ExpirationInfoGroup.Empty, 0);
            TemporaryStatModifierState secondModifier = new(StatType.Strength, 5, ExpirationInfoGroup.Empty, 1);
            PassiveState firstPassive = new("passive", 1, 1, ValueArray<PassiveExpirationProgress>.Empty);
            PassiveState stackedPassive = new("passive", 2, 3, ValueArray<PassiveExpirationProgress>.Empty);
            CombatState state = _initialState
                .ApplyCommand(new CombatCommand_ApplyStatModifier(_unitId, _timing, _unitId, _unitId, firstModifier))
                .ApplyCommand(new CombatCommand_RemoveStatModifier(_unitId, _timing, _unitId, secondModifier))
                .ApplyCommand(new CombatCommand_RemoveStatModifier(_unitId, _timing, _unitId, firstModifier))
                .ApplyCommand(new CombatCommand_AddPassive(_unitId, _timing, _unitId, _unitId, "passive", 1, firstPassive))
                .ApplyCommand(new CombatCommand_AddPassive(_unitId, _timing, _unitId, _unitId, "passive", 2, stackedPassive));

            UnitState afterAdds = state.GetUnit(_unitId);
            Assert.That(afterAdds.TemporaryStatModifiers, Is.Empty);
            Assert.That(afterAdds.ActivePassives, Is.EqualTo(new ValueArray<PassiveState>(stackedPassive)));
            Assert.That(afterAdds.HasPassive("passive"), Is.True);

            state = state.ApplyCommand(new CombatCommand_ReducePassiveStacks(_unitId, "passive", 10, null));
            Assert.That(state.GetUnit(_unitId).ActivePassives, Is.Empty);
        }

        [Test]
        public void EffectCommandsIgnoreDuplicatesAndMissingRemovals()
        {
            CombatState state = _initialState
                .ApplyCommand(new CombatCommand_AddNonStackableEffect(_unitId, _timing, _unitId, _unitId, "effect"))
                .ApplyCommand(new CombatCommand_AddNonStackableEffect(_unitId, _timing, _unitId, _unitId, "effect"))
                .ApplyCommand(new CombatCommand_RemoveNonStackableEffect(_unitId, _timing, _unitId, _unitId, "missing"));
            Assert.That(state.GetUnit(_unitId).ActiveNonStackableEffectIds, Is.EqualTo(new ValueArray<string>("effect")));
            Assert.That(state.GetUnit(_unitId).HasNonStackableEffect("effect"), Is.True);

            state = state.ApplyCommand(new CombatCommand_RemoveNonStackableEffect(_unitId, _timing, _unitId, _unitId, "effect"));
            Assert.That(state.GetUnit(_unitId).ActiveNonStackableEffectIds, Is.Empty);
            Assert.That(state.GetUnit(_unitId).IsDead(), Is.False);
        }

        [Test]
        public void EventRecorderOnlyRecordsObserverEventsAndRejectsUseAfterDispose()
        {
            CombatEventRecorder recorder = new();
            CombatEvent spent = new CombatCommand_BeginAction(_unitId, 1, 2).CreateEvents()[1];
            recorder.Record(spent);
            Assert.That(recorder.BuildLog(), Is.EqualTo(new ValueArray<CombatEvent>(spent)));

            ((IDisposable)recorder).Dispose();
            ((IDisposable)recorder).Dispose();
            Assert.Throws<ObjectDisposedException>(() => recorder.Record(spent));
            Assert.Throws<ObjectDisposedException>(() => recorder.BuildLog());
        }

        [Test]
        public void UnknownCommandFailsExplicitly()
        {
            Assert.Throws<NotImplementedException>(() => _initialState.ApplyCommand(new UnknownCombatCommand()));
        }

        private sealed record UnknownCombatCommand : CombatCommand;
    }
}
