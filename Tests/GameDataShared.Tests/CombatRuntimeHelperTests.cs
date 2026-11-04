using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using NUnit.Framework;
using System;
using System.Linq;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class CombatRuntimeHelperTests
    {
        private static readonly CombatUnitUniqueIdentifier AllyId = new(new CombatUnitOwner_AI(), "Ally", 0);
        private static readonly CombatUnitUniqueIdentifier EnemyId = new(new CombatUnitOwner_AI(), "Enemy", 0);
        private static readonly CombatUnitUniqueIdentifier NeutralId = new(new CombatUnitOwner_AI(), "Neutral", 0);

        [Test]
        public void CombatStateFindsUnitsPositionsDistancesAndRelations()
        {
            GridPosition sharedPosition = new(2, 3);
            UnitState ally = CreateUnit(AllyId, TeamAffiliation.Ally, 10, sharedPosition, new ValueArray<PassiveState>(new PassiveState("guard", 1, 1, ValueArray<PassiveExpirationProgress>.Empty)), new ValueArray<string>("ward"));
            UnitState enemy = CreateUnit(EnemyId, TeamAffiliation.Enemy, 0, sharedPosition);
            UnitState neutral = CreateUnit(NeutralId, TeamAffiliation.Neutral, 5, new GridPosition(-1, 4));
            CombatState state = CreateState(ally, enemy, neutral);

            Assert.That(state.GetUnit(AllyId), Is.EqualTo(ally));
            Assert.That(state.TryGetUnit(EnemyId, out UnitState? found), Is.True);
            Assert.That(found, Is.EqualTo(enemy));
            Assert.That(state.TryGetUnit(new CombatUnitUniqueIdentifier(new CombatUnitOwner_AI(), "missing", 0), out _), Is.False);
            Assert.That(() => state.GetUnitGridPosition(new CombatUnitUniqueIdentifier(new CombatUnitOwner_AI(), "missing", 0)), Throws.TypeOf<System.Collections.Generic.KeyNotFoundException>());
            Assert.That(state.GetUnitAtPosition(sharedPosition), Is.AnyOf(ally, enemy));
            Assert.That(state.GetAllUnitsAtPosition(sharedPosition).ToArray(), Has.Length.EqualTo(2));
            Assert.That(state.GetAllUnitsAtPosition(new GridPosition(99, 99)).ToArray(), Is.Empty);
            Assert.That(state.DistanceTo(AllyId, NeutralId, DistanceType.Manhattan), Is.EqualTo(4));
            Assert.That(state.DetermineRelationBetween(AllyId, AllyId), Is.EqualTo(TeamRelation.Friendly));
            Assert.That(state.DetermineRelationBetween(AllyId, EnemyId), Is.EqualTo(TeamRelation.Hostile));
            Assert.That(state.DetermineRelationBetween(AllyId, NeutralId), Is.EqualTo(TeamRelation.Neutral));
            Assert.That(state.GetUnit(AllyId).HasPassive("guard"), Is.True);
            Assert.That(state.GetUnit(AllyId).HasPassive("missing"), Is.False);
            Assert.That(state.GetUnit(AllyId).HasNonStackableEffect("ward"), Is.True);
            Assert.That(state.GetUnit(AllyId).HasNonStackableEffect("missing"), Is.False);
            Assert.That(ally.IsDead(), Is.False);
            Assert.That(enemy.IsDead(), Is.True);
            Assert.That(ally.ActivePassives.FindIndexOfPassive("guard"), Is.Zero);
            Assert.That(ally.ActivePassives.FindIndexOfPassive("missing"), Is.EqualTo(-1));
        }

        [Test]
        public void CombatStatusCoversActiveVictoryDefeatAndNoLivingTeams()
        {
            UnitState allyAlive = CreateUnit(AllyId, TeamAffiliation.Ally, 1, new GridPosition(0, 0));
            UnitState enemyAlive = CreateUnit(EnemyId, TeamAffiliation.Enemy, 1, new GridPosition(1, 0));
            UnitState allyDead = CreateUnit(AllyId, TeamAffiliation.Ally, 0, new GridPosition(0, 0));
            UnitState enemyDead = CreateUnit(EnemyId, TeamAffiliation.Enemy, 0, new GridPosition(1, 0));

            Assert.That(CreateState(allyAlive, enemyAlive).DeriveStatus(), Is.TypeOf<CombatStatus_Active>());
            Assert.That(CreateState(allyAlive, enemyDead).DeriveStatus(), Is.TypeOf<CombatStatus_Victory>());
            Assert.That(CreateState(allyDead, enemyAlive).DeriveStatus(), Is.TypeOf<CombatStatus_Defeat>());
            Assert.Throws<NotImplementedException>(() => CreateState(allyDead, enemyDead).DeriveStatus());
        }

        [Test]
        public void UnitStateDeadCheckIncludesZeroAndNegativeHealth()
        {
            Assert.That(CreateUnit(AllyId, TeamAffiliation.Ally, 0, default).IsDead(), Is.True);
            Assert.That(CreateUnit(AllyId, TeamAffiliation.Ally, -1, default).IsDead(), Is.True);
            Assert.That(CreateUnit(AllyId, TeamAffiliation.Ally, 1, default).IsDead(), Is.False);
        }

        private static UnitState CreateUnit(CombatUnitUniqueIdentifier id, TeamAffiliation affiliation, int health, GridPosition position, ValueArray<PassiveState> passives = default, ValueArray<string> effects = default)
        {
            return new UnitState(id, affiliation, health, 2, false, position, EightDirection.Down, passives, effects, ValueArray<TemporaryStatModifierState>.Empty, null);
        }

        private static CombatState CreateState(params UnitState[] units)
        {
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder builder = ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.CreateBuilder();
            foreach (UnitState unit in units)
                builder.Add(unit.UniqueIdentifier, unit);
            return new CombatState("combat", builder.ToValueDictionary());
        }
    }
}
