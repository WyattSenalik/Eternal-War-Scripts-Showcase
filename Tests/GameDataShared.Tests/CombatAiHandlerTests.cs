using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Handlers;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector;
using EternalWar.Core.Model.Run.Runtime;
using NUnit.Framework;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class CombatAiHandlerTests
    {
        private static readonly CombatUnitUniqueIdentifier _userId = new(new CombatUnitOwner_AI(), "user", 0);
        private static readonly CombatUnitUniqueIdentifier _allyId = new(new CombatUnitOwner_AI(), "ally", 0);
        private static readonly CombatUnitUniqueIdentifier _enemyNearId = new(new CombatUnitOwner_AI(), "enemy-near", 0);
        private static readonly CombatUnitUniqueIdentifier _enemyFarId = new(new CombatUnitOwner_AI(), "enemy-far", 0);
        private static readonly CombatUnitUniqueIdentifier _enemyDeadId = new(new CombatUnitOwner_AI(), "enemy-dead", 0);

        [Test]
        public void TargetSelectorsReturnExplicitTargetsAndNearestEligibleLiveUnit()
        {
            UnitState user = Unit(_userId, TeamAffiliation.Ally, 10, 0, 0);
            UnitState ally = Unit(_allyId, TeamAffiliation.Ally, 10, 1, 0);
            UnitState nearEnemy = Unit(_enemyNearId, TeamAffiliation.Enemy, 10, 2, 0);
            UnitState farEnemy = Unit(_enemyFarId, TeamAffiliation.Enemy, 10, 5, 0);
            UnitState deadEnemy = Unit(_enemyDeadId, TeamAffiliation.Enemy, 0, 1, 0);
            CombatState state = State(user, ally, nearEnemy, farEnemy, deadEnemy);
            AITargetSelectionHandlerContext context = TargetContext(state, _userId, _allyId);

            Assert.That(new AITargetSelectorInfo_User().GetTargetIdentifier(context), Is.EqualTo(_userId));
            Assert.That(new AITargetSelectorInfo_CurrentSubject().GetTargetIdentifier(context), Is.EqualTo(_allyId));
            Assert.That(new AITargetSelectorInfo_ClosestSpecificUnit("enemy-far", new AITargetSelectorInfo_User()).GetTargetIdentifier(context), Is.EqualTo(_enemyFarId));
            Assert.That(new AITargetSelectorInfo_ClosestUnitWithRelation(TeamRelation.Hostile, new AITargetSelectorInfo_User()).GetTargetIdentifier(context), Is.EqualTo(_enemyNearId));
            Assert.That(new AITargetSelectorInfo_ClosestUnitWithRelation(TeamRelation.Friendly, new AITargetSelectorInfo_User()).GetTargetIdentifier(context), Is.EqualTo(_userId));
            Assert.That(new AITargetSelectorInfo_ClosestSpecificUnit("missing", new AITargetSelectorInfo_User()).GetTargetIdentifier(context), Is.Null);
        }

        [Test]
        public void ClosestTargetUsesRequestedDistanceAndReturnsNullWhenRelativeTargetIsMissing()
        {
            UnitState user = Unit(_userId, TeamAffiliation.Ally, 10, 0, 0);
            UnitState diagonal = Unit(_allyId, TeamAffiliation.Enemy, 10, 2, 2);
            UnitState straight = Unit(_enemyNearId, TeamAffiliation.Enemy, 10, 3, 0);
            CombatState state = State(user, diagonal, straight);
            AITargetSelectionHandlerContext context = TargetContext(state, _userId, diagonal.UniqueIdentifier);

            AITargetSelectorInfo_ClosestUnitWithRelation manhattan = new(TeamRelation.Hostile, new AITargetSelectorInfo_User());
            AITargetSelectorInfo_ClosestUnitWithRelation euclidean = manhattan with { DistanceComparisonType = DistanceType.Euclidean };
            Assert.That(manhattan.GetTargetIdentifier(context), Is.EqualTo(_enemyNearId));
            Assert.That(euclidean.GetTargetIdentifier(context), Is.EqualTo(_allyId));

            AITargetSelectorInfo_ClosestSpecificUnit nestedMissing = new("absent", new AITargetSelectorInfo_ClosestSpecificUnit("also-absent", new AITargetSelectorInfo_User()));
            Assert.That(nestedMissing.GetTargetIdentifier(context), Is.Null);
            Assert.Throws<System.InvalidOperationException>(() => new AITargetSelectorInfo_ClosestSpecificUnit("anything", null!).GetTargetIdentifier(context));
        }

        [Test]
        public void BooleanAiConditionsHandleEmptyListsShortCircuitAndNestedNegation()
        {
            CombatState state = State(
                Unit(_userId, TeamAffiliation.Ally, 10, 0, 0, new ValueArray<PassiveState>(new PassiveState("guard", 1, 1, ValueArray<PassiveExpirationProgress>.Empty)), new ValueArray<string>("shield")),
                Unit(_enemyNearId, TeamAffiliation.Enemy, 10, 2, 0, new ValueArray<PassiveState>(new PassiveState("guard", 1, 1, ValueArray<PassiveExpirationProgress>.Empty)), new ValueArray<string>("shield")));
            DeterministicRandomState randomState = new(0);
            AIConditionHandlerContext context = ConditionContext(state, randomState, _userId, _enemyNearId);

            AIConditionInfo hasPassive = new AIConditionInfo_HasPassive("guard");
            AIConditionInfo hasEffect = new AIConditionInfo_HasNonStackableEffect("shield");
            AIConditionInfo missingPassive = new AIConditionInfo_HasPassive("missing");
            AssertCondition(context, hasPassive, true);
            AssertCondition(context, hasEffect, true);
            AssertCondition(context, missingPassive, false);
            AssertCondition(context, new AIConditionInfo_IsRelation(TeamRelation.Hostile), true);
            AssertCondition(context, new AIConditionInfo_IsRelation(TeamRelation.Friendly), false);

            AssertCondition(context, new AIConditionInfo_And(ValueArray<AIConditionInfo>.Empty), true);
            AssertCondition(context, new AIConditionInfo_And(new ValueArray<AIConditionInfo>(hasPassive, hasEffect)), true);
            AssertCondition(context, new AIConditionInfo_And(new ValueArray<AIConditionInfo>(hasPassive, missingPassive)), false);
            AssertCondition(context, new AIConditionInfo_Or(ValueArray<AIConditionInfo>.Empty), false);
            AssertCondition(context, new AIConditionInfo_Or(new ValueArray<AIConditionInfo>(missingPassive, hasEffect)), true);
            AssertCondition(context, new AIConditionInfo_Not(missingPassive), true);
            AssertCondition(context, new AIConditionInfo_Not(new AIConditionInfo_Not(hasEffect)), true);
        }

        [Test]
        public void DistanceConditionEvaluatesDistanceAndHandlesMissingSelectedTarget()
        {
            CombatState state = State(Unit(_userId, TeamAffiliation.Ally, 10, 0, 0), Unit(_enemyNearId, TeamAffiliation.Enemy, 10, 2, 1));
            DeterministicRandomState randomState = new(0);
            AIConditionHandlerContext context = ConditionContext(state, randomState, _userId, _enemyNearId);
            AIConditionInfo_DistanceToTarget condition = new(new AITargetSelectorInfo_User(), new NumericPredicate_Equal(3))
            {
                DistanceType = DistanceType.Manhattan
            };
            AssertCondition(context, condition, true);
            AssertCondition(context, condition with { DistanceType = DistanceType.Euclidean }, false);

            AIConditionInfo_DistanceToTarget missing = condition with
            {
                Other = new AITargetSelectorInfo_ClosestSpecificUnit("absent", new AITargetSelectorInfo_User())
            };
            AssertCondition(context, missing, false);
        }

        private static void AssertCondition(AIConditionHandlerContext context, AIConditionInfo condition, bool expected)
        {
            IAIConditionHandler handler = CombatHandlersRegistration.GetHandler(condition);
            Assert.That(handler.DoesSatisfy(context, condition), Is.EqualTo(expected), condition.GetType().Name);
        }

        private static AIConditionHandlerContext ConditionContext(CombatState state, DeterministicRandomState randomState, CombatUnitUniqueIdentifier userId, CombatUnitUniqueIdentifier subjectId)
        {
            return new AIConditionHandlerContext(null!, null!, state, new CombatSimulationResult(state, randomState, ValueArray<CombatCommand>.Empty, ValueArray<CombatEvent>.Empty), userId, subjectId);
        }

        private static AITargetSelectionHandlerContext TargetContext(CombatState state, CombatUnitUniqueIdentifier userId, CombatUnitUniqueIdentifier subjectId)
        {
            return new AITargetSelectionHandlerContext(null!, null!, state, userId, subjectId);
        }

        private static CombatState State(params UnitState[] units)
        {
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder builder = ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.CreateBuilder();
            foreach (UnitState unit in units)
                builder.Add(unit.UniqueIdentifier, unit);
            return new CombatState("ai-test", builder.ToValueDictionary());
        }

        private static UnitState Unit(CombatUnitUniqueIdentifier id, TeamAffiliation affiliation, int health, int x, int y, ValueArray<PassiveState> passives = default, ValueArray<string> effects = default)
        {
            return new UnitState(id, affiliation, health, 2, false, new GridPosition(x, y), EightDirection.Down, passives, effects, ValueArray<TemporaryStatModifierState>.Empty, null);
        }
    }
}
