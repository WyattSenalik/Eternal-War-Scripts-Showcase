using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Map.Definitions;
using EternalWar.Core.Model.Map.Runtime;
using EternalWar.Core.Model.Random;
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Network;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class CombatSimulationEventTests
    {
        private const string ACTION_ID = "event-coverage";
        private static readonly CombatUnitUniqueIdentifier USER_ID = new(new CombatUnitOwner_AI(), "User", 0);
        private static readonly CombatUnitUniqueIdentifier TARGET_ID = new(new CombatUnitOwner_AI(), "Target", 0);
        private static readonly GridPosition USER_START = new(1, 1);
        private static readonly GridPosition TARGET_START = new(4, 1);
        private static readonly GridPosition MOVE_DESTINATION = new(3, 1);
        private static readonly HitRulesInfo ALL_HIT_RULES = new(CanHitEnemies: true, CanHitAllies: true, CanHitNeutral: true, CanHitSelf: true);

        [Test]
        public void SimulateEmitsOrderedEventsForEveryCombatStepAndPassiveReaction()
        {
            CombatActionSequenceDefinition sequence = CreateFullCoverageSequence();
            TestWorld world = CreateWorld(sequence, targetHealth: 20, targetPassives: new ValueArray<PassiveState>(
                new PassiveState("decay-on-hit", Activations: 0, Stacks: 1, ValueArray<PassiveExpirationProgress>.Empty)));

            CombatSimulationResult result = Simulate(world, sequence.Id, new HitRollResult(DidHit: true, DidCrit: false));

            ActionSequenceTimingContext Timing(int step) => new(sequence.Id, step);
            TemporaryStatModifierState modifier = new(StatType.Strength, 2, ExpirationInfoGroup.Empty, ExpirationProgress: 0);
            PassiveState addedPassive = new("applied-passive", Activations: 0, Stacks: 2, ValueArray<PassiveExpirationProgress>.Empty);
            PassiveState activatedPassive = addedPassive with { Activations = 1 };
            PassiveState progressedPassive = activatedPassive with
            {
                ExpirationProgresses = new ValueArray<PassiveExpirationProgress>(new PassiveExpirationProgress(RuleIndex: 0, Occurrences: 1))
            };

            CombatEvent[] expectedEvents = new CombatEvent[]
            {
                CombatEvent_ActionStarted.Instance,
                new CombatEvent_ActionPointsSpent(USER_ID, ActionPointsSpent: 1, ResultingActionPoints: 2),

                new CombatEvent_StepStarted(USER_ID, Timing(0)),
                new CombatEvent_FacingChanged(USER_ID, Timing(0), USER_ID, EightDirection.Down, EightDirection.Left),
                new CombatEvent_Moved(USER_ID, Timing(0), USER_ID, USER_START, new GridPosition(2, 1), MovementType.Walk, IsFirstMove: true, IsLastMove: false),
                new CombatEvent_Moved(USER_ID, Timing(0), USER_ID, new GridPosition(2, 1), MOVE_DESTINATION, MovementType.Walk, IsFirstMove: false, IsLastMove: true),
                new CombatEvent_StepEnded(USER_ID, Timing(0)),

                new CombatEvent_StepStarted(USER_ID, Timing(1)),
                new CombatEvent_AttackSwung(USER_ID, Timing(1), new ValueArray<GridPosition>(TARGET_START)),
                new CombatEvent_AttackHit(USER_ID, Timing(1), TARGET_ID, WasCrit: false, WasFatal: false),
                new CombatEvent_DamageApplied(USER_ID, Timing(1), USER_ID, TARGET_ID, DamageType.True, DamageDealt: 4, ResultingHealth: 16, WasCrit: false),
                new CombatEvent_PassiveActivated(TARGET_ID, USER_ID, "decay-on-hit", PassiveActivateTime.OnHit,
                    new PassiveState("decay-on-hit", Activations: 1, Stacks: 1, ValueArray<PassiveExpirationProgress>.Empty)),
                new CombatEvent_PassiveStacksReduced(TARGET_ID, "decay-on-hit", StacksReduced: 1, ResultingPassive: null),
                new CombatEvent_PassiveRemoved(TARGET_ID, "decay-on-hit"),
                new CombatEvent_StepEnded(USER_ID, Timing(1)),

                new CombatEvent_StepStarted(USER_ID, Timing(2)),
                new CombatEvent_HealApplied(USER_ID, Timing(2), USER_ID, USER_ID, HealthRestored: 3, ResultingHealth: 13),
                new CombatEvent_StepEnded(USER_ID, Timing(2)),

                new CombatEvent_StepStarted(USER_ID, Timing(3)),
                new CombatEvent_StatModifierApplied(USER_ID, Timing(3), USER_ID, USER_ID, modifier),
                new CombatEvent_StepEnded(USER_ID, Timing(3)),

                new CombatEvent_StepStarted(USER_ID, Timing(4)),
                new CombatEvent_NonStackableEffectAdded(USER_ID, Timing(4), USER_ID, USER_ID, "test-effect"),
                new CombatEvent_StepEnded(USER_ID, Timing(4)),

                new CombatEvent_StepStarted(USER_ID, Timing(5)),
                new CombatEvent_NonStackableEffectRemoved(USER_ID, Timing(5), USER_ID, USER_ID, "test-effect"),
                new CombatEvent_StepEnded(USER_ID, Timing(5)),

                new CombatEvent_StepStarted(USER_ID, Timing(6)),
                new CombatEvent_PassiveAdded(USER_ID, Timing(6), USER_ID, USER_ID, "applied-passive", StacksAdded: 2, addedPassive),
                new CombatEvent_PassiveActivated(USER_ID, USER_ID, "applied-passive", PassiveActivateTime.OnApply, activatedPassive),
                new CombatEvent_PassiveExpirationProgressed(USER_ID, "applied-passive", progressedPassive),
                new CombatEvent_StepEnded(USER_ID, Timing(6)),

                new CombatEvent_StepStarted(USER_ID, Timing(7)),
                new CombatEvent_Moved(USER_ID, Timing(7), TARGET_ID, TARGET_START, TARGET_START, MovementType.Push, IsFirstMove: true, IsLastMove: false),
                new CombatEvent_Moved(USER_ID, Timing(7), TARGET_ID, TARGET_START, new GridPosition(5, 1), MovementType.Push, IsFirstMove: false, IsLastMove: true),
                new CombatEvent_StepEnded(USER_ID, Timing(7)),
                CombatEvent_ActionEnded.Instance
            };


            ValueArray<CombatEvent> resultEventLog = result.EventLog;
            int maxEventCount = Math.Max(resultEventLog.Count, expectedEvents.Length);
            for (int i = 0; i < maxEventCount; ++i)
            {
                Assert.That(i, Is.LessThan(resultEventLog.Count));
                Assert.That(i, Is.LessThan(expectedEvents.Length));

                CombatEvent currentResultEvent = resultEventLog[i];
                CombatEvent currentExpectedEvent = expectedEvents[i];
                Assert.That(currentResultEvent, Is.EqualTo(currentExpectedEvent), $"Index={i}\nActual Event: {currentResultEvent}\nExpected Event: {currentExpectedEvent}\n\nActual event log:\n{string.Join("\n", result.EventLog.Select(combatEvent => combatEvent.ToString()))}\n\nExpected event log:\n{string.Join("\n", expectedEvents.Select(combatEvent => combatEvent.ToString()))}");
            }
            Assert.That(
                result.EventLog,
                Is.EqualTo(new ValueArray<CombatEvent>(expectedEvents)),
                $"Actual event log:\n{string.Join("\n", result.EventLog.Select(combatEvent => combatEvent.ToString()))}\n\nExpected event log:\n{string.Join("\n", expectedEvents.Select(combatEvent => combatEvent.ToString()))}");
            Assert.That(result.FinalState.GetUnit(USER_ID).CurrentActionPoints, Is.EqualTo(2));
            Assert.That(result.FinalState.GetUnit(USER_ID).CurrentHealth, Is.EqualTo(13));
            Assert.That(result.FinalState.GetUnit(USER_ID).GridPosition, Is.EqualTo(MOVE_DESTINATION));
            Assert.That(result.FinalState.GetUnit(TARGET_ID).GridPosition, Is.EqualTo(new GridPosition(5, 1)));
        }

        [Test]
        public void SimulateEmitsOrderedEventsForHitUnitWhoHasCounterAndAsleep()
        {
            GridPosition playerPosition = new(0, 0);
            UnitPlacementOptionInfo playerUnitPlacementOption = new(playerPosition, EightDirection.Right);

            BlockInfo[] blocksForTestCombat = new BlockInfo[64];
            for (int x = 0; x < 8; ++x)
                for (int y = 0; y < 8; ++y)
                    blocksForTestCombat[y * 8 + x] = new BlockInfo("floor", new GridPosition(x, y), Rotation_Restricted_Four._0, 0);
            Dictionary<string, CombatDefinition> combats = new();
            combats["event-test-combat"] = new CombatDefinition(
                "event-test-combat",
                TileGrid: new TileGridInfo(GridSize: new Vector2Int(8, 8), TileSize: Vector2.One, TileSpacing: Vector2.Zero),
                GridBlocks: new GridBlocksInfo(blocksForTestCombat),
                PlayerUnitPlacementOptions: new ValueArray<UnitPlacementOptionInfo>(playerUnitPlacementOption),
                NonPlayerUnitPlacements: new ValueArray<UnitPlacementInfo>(new UnitPlacementInfo("test-enemy", new UnitPlacementOptionInfo(new GridPosition(0, 1), EightDirection.Right))),
                RewardsInfo: new CombatRewardsInfo(false, ValueArray<string>.Empty));

            Dictionary<string, UnitDefinition> units = new();
            units["test-ally"] = new UnitDefinition(
                "test-ally",
                new HealthInfo(false, 10),
                new AttributeStats(5, 5, 5, 5, 5, 0, 100),
                BaseMovement: 3,
                BaseActionPoints: 5,
                StartingAffiliation: TeamAffiliation.Ally,
                CanBeRevived: true,
                ShouldTakeATurn: true,
                ShouldBlockOccupiedTile: true,
                CanBeMoved: true,
                CanBeTargeted: true,
                MoveCombatActionId: "test-move",
                CombatActionIds: ValueArray<string>.Empty,
                StartingPassiveIds: new ValueArray<string>("test-counter-passive"));
            units["test-enemy"] = new UnitDefinition(
                "test-enemy",
                new HealthInfo(false, 10),
                new AttributeStats(5, 5, 5, 5, 5, 0, 100),
                BaseMovement: 3,
                BaseActionPoints: 5,
                StartingAffiliation: TeamAffiliation.Enemy,
                CanBeRevived: false,
                ShouldTakeATurn: true,
                ShouldBlockOccupiedTile: true,
                CanBeMoved: true,
                CanBeTargeted: true,
                MoveCombatActionId: "test-move",
                CombatActionIds: new ValueArray<string>("test-punch", "test-sleep"),
                StartingPassiveIds: ValueArray<string>.Empty);

            Dictionary<string, CombatActionSequenceDefinition> actionSequences = new();
            actionSequences["test-move"] = new CombatActionSequenceDefinition(
                "test-move", 
                ActionPointCost: 1, 
                Steps: ValueArray<CombatStepInfo>.Empty);
            actionSequences["test-sleep"] = new CombatActionSequenceDefinition(
                "test-sleep",
                ActionPointCost: 1,
                Steps: new ValueArray<CombatStepInfo>(
                    new CombatStepInfo_AddPassive(
                        ShouldUserFaceTarget: false,
                        Location: new TileLocationInfo(
                            TileLocation.MostRecentlySelected,
                            1,
                            new SelectionSpecificationsInfo(100, SelectionRangeType.Distance, 1, 1, SelectionType.HasUnit, new HitRulesInfo(true, true, true, false))),
                        PassiveId: "test-sleep-passive",
                        StacksToAddCalcFunction: new NCalcFunctionInfo("1", ValueArray<NCalcVariableInfo>.Empty),
                        HitRules: new HitRulesInfo(true, true, true, false))
                    ));
            actionSequences["test-apply-sleep-non-stackable-effect"] = new CombatActionSequenceDefinition(
                "test-apply-sleep-non-stackable-effect",
                ActionPointCost: 0,
                Steps: new ValueArray<CombatStepInfo>(
                    new CombatStepInfo_AddRemoveNonStackableEffect(
                        ShouldUserFaceTarget: false,
                        Location: new TileLocationInfo(TileLocation.Self, 0, new SelectionSpecificationsInfo(0, default, 0, 0, default, ALL_HIT_RULES)),
                        HitRules: ALL_HIT_RULES,
                        NonStackableEffectId: "test-sleep-non-stackable-effect",
                        AddRemoveType: AddRemoveType.Add)
                    ));
            actionSequences["test-remove-sleep-non-stackable-effect"] = new CombatActionSequenceDefinition(
                "test-remove-sleep-non-stackable-effect",
                ActionPointCost: 0,
                Steps: new ValueArray<CombatStepInfo>(
                    new CombatStepInfo_AddRemoveNonStackableEffect(
                        ShouldUserFaceTarget: false,
                        Location: new TileLocationInfo(TileLocation.Self, 0, new SelectionSpecificationsInfo(0, default, 0, 0, default, ALL_HIT_RULES)),
                        HitRules: ALL_HIT_RULES,
                        NonStackableEffectId: "test-sleep-non-stackable-effect",
                        AddRemoveType: AddRemoveType.Remove)
                    ));
            actionSequences["test-punch"] = new CombatActionSequenceDefinition(
                "test-punch",
                ActionPointCost: 1,
                Steps: new ValueArray<CombatStepInfo>(
                    new CombatStepInfo_Melee(
                        ShouldUserFaceTarget: false,
                        Location: new TileLocationInfo(
                            TileLocation.MostRecentlySelected,
                            1,
                            new SelectionSpecificationsInfo(100, SelectionRangeType.Distance, 1, 1, SelectionType.HasUnit, new HitRulesInfo(true, true, true, false))),
                        DamageInfo: new DamageInfo(
                            DamageType.Physical,
                            new HitInfo(new NCalcFunctionInfo("1", ValueArray<NCalcVariableInfo>.Empty),
                            HasMinimumValue: true,
                            MinimumValue: 1,
                            HitRules: new HitRulesInfo(true, true, true, false),
                            ShouldInvokeOnHitEvent: true)),
                        BaseAccuracy: 100)
                    ));

            Dictionary<string, PassiveDefinition> passives = new();
            passives["test-counter-passive"] = new PassiveDefinition(
                "test-counter-passive",
                CanStack: false,
                PassiveActivateTime.OnHit,
                CanActivateWhenDead: false,
                new PassiveConditionInfo_AttackedWithinDistance(1),
                HasActivationLimitPerEncounter: false,
                MaxActivationsPerEncounter: 0,
                new PassiveActionInfo_None(),
                StackReductionExpirationInfo: ExpirationInfoGroup.Empty,
                StackReductionNCalcFunction: new NCalcFunctionInfo("", ValueArray<NCalcVariableInfo>.Empty),
                SeparateExpirationActionInfo: PassiveActionInfo.Empty);
            passives["test-sleep-passive"] = new PassiveDefinition(
                "test-sleep-passive",
                CanStack: false,
                PassiveActivateTime.OnApply,
                CanActivateWhenDead: false,
                ConditionInfo: new PassiveConditionInfo_Always(),
                HasActivationLimitPerEncounter: false,
                MaxActivationsPerEncounter: 0,
                ActionInfo: new PassiveActionInfo_CombatAction("test-apply-sleep-non-stackable-effect"),
                StackReductionExpirationInfo: new ExpirationInfoGroup(new ValueArray<ExpirationInfo>(new ExpirationInfo(ExpirationTime.OnHit, 1))),
                StackReductionNCalcFunction: new NCalcFunctionInfo("1", ValueArray<NCalcVariableInfo>.Empty),
                SeparateExpirationActionInfo: new PassiveActionInfo_CombatAction("test-remove-sleep-non-stackable-effect"))
            {
                Priority = 1
            };

            Dictionary<string, NonStackableEffectDefinition> nonStackableEffects = new();
            nonStackableEffects["test-sleep-non-stackable-effect"] = new NonStackableEffectDefinition(
                "test-sleep-non-stackable-effect",
                new NonStackableEffectInfo_SkipTurn());

            Dictionary<string, BlockDefinition> blocks = new();
            blocks["floor"] = new BlockDefinition("floor", 0, true);

            GameDatabase database = new(combats, units, actionSequences, passives, nonStackableEffects, Artifacts: ValueDictionary<string, ArtifactDefinition>.Empty, blocks, FakeLocationMaps: ValueDictionary<string, FakeLocationMapDefinition>.Empty, LocationNodes: ValueDictionary<string, LocationNodeDefinition>.Empty);

            CombatUnitUniqueIdentifier playerUnitIdentifier = new(
                Owner: new CombatUnitOwner_Player(new ClientInfo(Guid.NewGuid(), 0)),
                DefinitionId: "test-ally",
                Index: 0);
            RunRuntime run = new(
                new RunInstance(new ValueArray<CombatUnitUniqueIdentifier>(playerUnitIdentifier)),
                new RunState(
                    new TEMP_NOTHING_RUN_PHASE(),
                    new ValueArray<PlayerUnitUpgradesState>(new PlayerUnitUpgradesState(playerUnitIdentifier, new PlayerStatUpgradesState(0, 0, 0, 0, 0, 0), ValueArray<string>.Empty, ValueArray<string>.Empty)),
                    new DeterministicRandomState(0L),
                    new LocationMapState(new LocationNodeInstance("", ValueArray<int>.Empty), 0, ValueDictionary<ClientInfo, int>.Empty)));
            Dictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> playerUnitPlacements = new();
            playerUnitPlacements[playerUnitIdentifier] = playerUnitPlacementOption;
            CombatRuntime initializedCombat = CombatInitialization.InitializeCombat(database, run, "event-test-combat", Vector3.Zero, playerUnitPlacements, out DeterministicRandomState randomState);

            CombatUnitUniqueIdentifier enemyIdentifier = initializedCombat.State.UnitMap.Keys.First(identifier => identifier != playerUnitIdentifier);
            ActionSelectionContext selectPlayerContext = new(new ValueArray<GridPositionGroup>(GridPositionGroup.CreateForSinglePosition(playerPosition)));
            ActionExecutionContext sleepActionContext = new("test-sleep", enemyIdentifier, selectPlayerContext);
            CombatSimulationResult sleepSimulationResult = CombatSimulator.Simulate(database, randomState, initializedCombat, sleepActionContext);

            CombatRuntime combatRuntime = initializedCombat with { State = sleepSimulationResult.FinalState };
            ActionExecutionContext punchActionContext = new("test-punch", enemyIdentifier, selectPlayerContext);
            CombatSimulationResult punchSimulationResult = CombatSimulator.Simulate(database, sleepSimulationResult.FinalRandomState, combatRuntime, punchActionContext, new SimulationOptions(new RandomResolutionResolver_AssumeSuccess()));

            ActionSequenceTimingContext punchTiming = new("test-punch", 0);
            ActionSequenceTimingContext sleepPassiveExpireTiming = new("test-remove-sleep-non-stackable-effect", 0);
            CombatEvent[] expectedEvents =
            [
                CombatEvent_ActionStarted.Instance,
                new CombatEvent_ActionPointsSpent(enemyIdentifier, ActionPointsSpent: 1, ResultingActionPoints: 3),
                new CombatEvent_StepStarted(enemyIdentifier, punchTiming),
                new CombatEvent_AttackSwung(enemyIdentifier, punchTiming, new ValueArray<GridPosition>(playerPosition)),
                new CombatEvent_AttackHit(enemyIdentifier, punchTiming, playerUnitIdentifier, WasCrit: false, WasFatal: false),
                new CombatEvent_DamageApplied(enemyIdentifier, punchTiming, enemyIdentifier, playerUnitIdentifier, DamageType.Physical, DamageDealt: 1, ResultingHealth: 9, WasCrit: false),
                new CombatEvent_PassiveStacksReduced(playerUnitIdentifier, "test-sleep-passive", 1, null),
                new CombatEvent_PassiveRemoved(playerUnitIdentifier, "test-sleep-passive"),
                new CombatEvent_ActionStarted(),
                new CombatEvent_StepStarted(playerUnitIdentifier, sleepPassiveExpireTiming),
                new CombatEvent_NonStackableEffectRemoved(playerUnitIdentifier, sleepPassiveExpireTiming, playerUnitIdentifier, playerUnitIdentifier, "test-sleep-non-stackable-effect"),
                new CombatEvent_StepEnded(playerUnitIdentifier, sleepPassiveExpireTiming),
                new CombatEvent_ActionEnded(),
                new CombatEvent_PassiveActivated(playerUnitIdentifier, enemyIdentifier, "test-counter-passive", PassiveActivateTime.OnHit, new PassiveState("test-counter-passive", Activations: 1, Stacks: 1, ValueArray<PassiveExpirationProgress>.Empty)),
                new CombatEvent_StepEnded(enemyIdentifier, punchTiming),
                CombatEvent_ActionEnded.Instance
            ];

            ValueArray<CombatEvent> resultEventLog = punchSimulationResult.EventLog;
            int maxEventCount = Math.Max(resultEventLog.Count, expectedEvents.Length);
            for (int i = 0; i < maxEventCount; ++i)
            {
                Assert.That(i, Is.LessThan(resultEventLog.Count));
                Assert.That(i, Is.LessThan(expectedEvents.Length));

                CombatEvent currentResultEvent = resultEventLog[i];
                CombatEvent currentExpectedEvent = expectedEvents[i];
                Assert.That(currentResultEvent, Is.EqualTo(currentExpectedEvent), $"Index={i}\nActual Event: {currentResultEvent}\nExpected Event: {currentExpectedEvent}\n\nActual event log:\n{string.Join("\n", resultEventLog.Select(combatEvent => combatEvent.ToString()))}\n\nExpected event log:\n{string.Join("\n", expectedEvents.Select(combatEvent => combatEvent.ToString()))}");
            }
            Assert.That(
                resultEventLog,
                Is.EqualTo(new ValueArray<CombatEvent>(expectedEvents)),
                $"Actual event log:\n{string.Join("\n", resultEventLog.Select(combatEvent => combatEvent.ToString()))}\n\nExpected event log:\n{string.Join("\n", expectedEvents.Select(combatEvent => combatEvent.ToString()))}");
        }

        [Test]
        public void SimulateMoveToAnInvalidGridPositionEmitsTeleportEvent()
        {
            const string actionId = "teleport-move";
            CombatActionSequenceDefinition sequence = new(actionId, ActionPointCost: 1, new ValueArray<CombatStepInfo>(new CombatStepInfo_Move(ShouldUserFaceTarget: false, Location(TileLocation.MostRecentlySelected))));
            TestWorld world = CreateWorld(sequence, targetHealth: 10);
            GridPosition destination = new(9, 1);
            ActionExecutionContext actionContext = new(actionId, USER_ID, new ActionSelectionContext(new ValueArray<GridPositionGroup>(GridPositionGroup.CreateForSinglePosition(destination))));

            SimulationOptions simulationOptions = new(new FixedRandomResolver(new HitRollResult(DidHit: true, DidCrit: false)));
            CombatSimulationResult result = CombatSimulator.Simulate(world.Database, world.RandomState, world.CombatInstance, world.CombatState, actionContext, simulationOptions);

            ActionSequenceTimingContext timing = new(actionId, 0);
            CombatEvent[] expectedEvents =
            {
                CombatEvent_ActionStarted.Instance,
                new CombatEvent_ActionPointsSpent(USER_ID, ActionPointsSpent: 1, ResultingActionPoints: 2),
                new CombatEvent_StepStarted(USER_ID, timing),
                new CombatEvent_Moved(USER_ID, timing, USER_ID, USER_START, destination, MovementType.Teleport, IsFirstMove: true, IsLastMove: true),
                new CombatEvent_StepEnded(USER_ID, timing),
                CombatEvent_ActionEnded.Instance
            };

            Assert.That(result.EventLog, Is.EqualTo(new ValueArray<CombatEvent>(expectedEvents)));
            Assert.That(result.FinalState.GetUnit(USER_ID).GridPosition, Is.EqualTo(destination));
        }

        [Test]
        public void SimulateAttackMissEmitsSwingAndMissWithoutDamage()
        {
            AssertAttackEvents(
                targetHealth: 10,
                new HitRollResult(DidHit: false, DidCrit: false),
                new CombatEvent_AttackMissed(USER_ID, new ActionSequenceTimingContext(ACTION_ID, 0), TARGET_ID));
        }

        [Test]
        public void SimulateCriticalAttackEmitsHitAndCriticalDamage()
        {
            AssertAttackEvents(
                targetHealth: 10,
                new HitRollResult(DidHit: true, DidCrit: true),
                new CombatEvent_AttackHit(USER_ID, new ActionSequenceTimingContext(ACTION_ID, 0), TARGET_ID, WasCrit: true, WasFatal: false),
                new CombatEvent_DamageApplied(USER_ID, new ActionSequenceTimingContext(ACTION_ID, 0), USER_ID, TARGET_ID, DamageType.True, DamageDealt: 6, ResultingHealth: 4, WasCrit: true));
        }

        [Test]
        public void SimulateFatalAttackEmitsDeathAfterDamage()
        {
            AssertAttackEvents(
                targetHealth: 4,
                new HitRollResult(DidHit: true, DidCrit: false),
                new CombatEvent_AttackHit(USER_ID, new ActionSequenceTimingContext(ACTION_ID, 0), TARGET_ID, WasCrit: false, WasFatal: true),
                new CombatEvent_DamageApplied(USER_ID, new ActionSequenceTimingContext(ACTION_ID, 0), USER_ID, TARGET_ID, DamageType.True, DamageDealt: 4, ResultingHealth: 0, WasCrit: false),
                new CombatEvent_UnitDied(USER_ID, new ActionSequenceTimingContext(ACTION_ID, 0), TARGET_ID));
        }

        private static void AssertAttackEvents(int targetHealth, HitRollResult rollResult, params CombatEvent[] outcomeEvents)
        {
            CombatActionSequenceDefinition sequence = CreateAttackSequence();
            TestWorld world = CreateWorld(sequence, targetHealth);
            CombatSimulationResult result = Simulate(world, sequence.Id, rollResult);
            ActionSequenceTimingContext timing = new(sequence.Id, 0);

            CombatEvent[] expectedEvents = (new CombatEvent[]
            {
                CombatEvent_ActionStarted.Instance,
                new CombatEvent_ActionPointsSpent(USER_ID, ActionPointsSpent: 1, ResultingActionPoints: 2),
                new CombatEvent_StepStarted(USER_ID, timing),
                new CombatEvent_AttackSwung(USER_ID, timing, new ValueArray<GridPosition>(TARGET_START)),
            }).Concat(outcomeEvents).Concat(new CombatEvent[]
            {
                new CombatEvent_StepEnded(USER_ID, timing),
                CombatEvent_ActionEnded.Instance
            }).ToArray();

            Assert.That(result.EventLog, Is.EqualTo(new ValueArray<CombatEvent>(expectedEvents)));
        }

        private static CombatActionSequenceDefinition CreateFullCoverageSequence()
        {
            TileLocationInfo selected = Location(TileLocation.MostRecentlySelected);
            TileLocationInfo self = Location(TileLocation.Self);
            HitInfo hitThree = Hit("3", ALL_HIT_RULES);
            HitInfo hitFour = Hit("4", ALL_HIT_RULES);

            CombatStepInfo[] steps =
            {
                new CombatStepInfo_Move(ShouldUserFaceTarget: true, selected),
                new CombatStepInfo_Melee(ShouldUserFaceTarget: false, selected, new DamageInfo(DamageType.True, hitFour), BaseAccuracy: 100),
                new CombatStepInfo_Heal(ShouldUserFaceTarget: false, self, hitThree),
                new CombatStepInfo_ModifyStat(ShouldUserFaceTarget: false, self, ActionInvolvedUnit.User, StatType.Strength, ExpirationInfoGroup.Empty, Hit("2", ALL_HIT_RULES)),
                new CombatStepInfo_AddRemoveNonStackableEffect(ShouldUserFaceTarget: false, self, ALL_HIT_RULES, "test-effect", AddRemoveType.Add),
                new CombatStepInfo_AddRemoveNonStackableEffect(ShouldUserFaceTarget: false, self, ALL_HIT_RULES, "test-effect", AddRemoveType.Remove),
                new CombatStepInfo_AddPassive(ShouldUserFaceTarget: false, self, "applied-passive", Function("2"), ALL_HIT_RULES),
                new CombatStepInfo_Push(ShouldUserFaceTarget: false, selected, ALL_HIT_RULES, PushDirection.AwayFromSource, TilesToPush: 2)
            };
            return new CombatActionSequenceDefinition("all-combat-steps", ActionPointCost: 1, new ValueArray<CombatStepInfo>(steps));
        }

        private static CombatActionSequenceDefinition CreateAttackSequence()
        {
            CombatStepInfo_Melee melee = new(
                ShouldUserFaceTarget: false,
                Location(TileLocation.MostRecentlySelected),
                new DamageInfo(DamageType.True, Hit("4", ALL_HIT_RULES)),
                BaseAccuracy: 100);
            return new CombatActionSequenceDefinition(ACTION_ID, ActionPointCost: 1, new ValueArray<CombatStepInfo>(melee));
        }

        private static TestWorld CreateWorld(CombatActionSequenceDefinition sequence, int targetHealth, ValueArray<PassiveState> targetPassives = default)
        {
            CombatStats baseStats = new(new HealthInfo(HasInfiniteHealth: false, Health: 20), new AttributeStats(Strength: 10, Defense: 0, Speed: 5, Magic: 0, Will: 0, Evasion: 0, Accuracy: 100), Movement: 5, MaxActionPoints: 3);
            UnitDefinition userDefinition = UnitDefinitionFor("User", TeamAffiliation.Ally, baseStats);
            UnitDefinition targetDefinition = UnitDefinitionFor("Target", TeamAffiliation.Enemy, baseStats);

            using ValueDictionary<string, CombatActionSequenceDefinition>.Builder actionSequences = ValueDictionary<string, CombatActionSequenceDefinition>.CreateBuilder();
            actionSequences.Add(sequence.Id, sequence);
            using ValueDictionary<string, UnitDefinition>.Builder unitDefinitions = ValueDictionary<string, UnitDefinition>.CreateBuilder();
            unitDefinitions.Add(userDefinition.Id, userDefinition);
            unitDefinitions.Add(targetDefinition.Id, targetDefinition);
            using ValueDictionary<string, CombatDefinition>.Builder combatDefinitions = ValueDictionary<string, CombatDefinition>.CreateBuilder();
            combatDefinitions.Add("event-test-combat", CreateCombatDefinition());
            using ValueDictionary<string, PassiveDefinition>.Builder passives = ValueDictionary<string, PassiveDefinition>.CreateBuilder();
            passives.Add("decay-on-hit", CreatePassive("decay-on-hit", PassiveActivateTime.OnHit, new ExpirationInfo(ExpirationTime.OnHit, OccurrencesBeforeExpiration: 1)));
            passives.Add("applied-passive", CreatePassive("applied-passive", PassiveActivateTime.OnApply, new ExpirationInfo(ExpirationTime.AfterActivate_PassiveSpecific, OccurrencesBeforeExpiration: 2)));
            using ValueDictionary<string, NonStackableEffectDefinition>.Builder effects = ValueDictionary<string, NonStackableEffectDefinition>.CreateBuilder();
            effects.Add("test-effect", new NonStackableEffectDefinition("test-effect", new NonStackableEffectInfo_SkipTurn()));
            using ValueDictionary<string, BlockDefinition>.Builder blocks = ValueDictionary<string, BlockDefinition>.CreateBuilder();
            blocks.Add("floor", new BlockDefinition("floor", ExtraStepCost: 0, CanStandOn: true));

            GameDatabase database = new(
                combatDefinitions.ToValueDictionary(),
                unitDefinitions.ToValueDictionary(),
                actionSequences.ToValueDictionary(),
                passives.ToValueDictionary(),
                effects.ToValueDictionary(),
                ValueDictionary<string, ArtifactDefinition>.Empty,
                blocks.ToValueDictionary(),
                ValueDictionary<string, FakeLocationMapDefinition>.Empty,
                ValueDictionary<string, LocationNodeDefinition>.Empty);

            UnitState userState = UnitStateFor(USER_ID, TeamAffiliation.Ally, USER_START, CurrentHealth: 10, CurrentActionPoints: 3, EightDirection.Down);
            UnitState targetState = UnitStateFor(TARGET_ID, TeamAffiliation.Enemy, TARGET_START, targetHealth, CurrentActionPoints: 3, EightDirection.Down, targetPassives);
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder unitStates = ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.CreateBuilder();
            unitStates.Add(USER_ID, userState);
            unitStates.Add(TARGET_ID, targetState);
            CombatState combatState = new("event-test-combat", unitStates.ToValueDictionary());

            using ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.Builder unitBuilds = ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.CreateBuilder();
            unitBuilds.Add(USER_ID, new UnitBuild(USER_ID, USER_START, EightDirection.Down, CombatStats.Empty, ValueArray<string>.Empty, ValueArray<string>.Empty));
            unitBuilds.Add(TARGET_ID, new UnitBuild(TARGET_ID, TARGET_START, EightDirection.Down, CombatStats.Empty, ValueArray<string>.Empty, ValueArray<string>.Empty));
            CombatInstance combatInstance = new("event-test-combat", new TileGridInstance(Vector3.Zero), unitBuilds.ToValueDictionary());

            DeterministicRandomState randomState = new(0);
            return new TestWorld(database, randomState, combatInstance, combatState);
        }

        private static CombatSimulationResult Simulate(TestWorld world, string actionId, HitRollResult rollResult)
        {
            GridPositionGroup[] selections = actionId == "all-combat-steps"
                ? new[]
                {
                    GridPositionGroup.CreateForSinglePosition(MOVE_DESTINATION),
                    GridPositionGroup.CreateForSinglePosition(TARGET_START),
                    GridPositionGroup.CreateForSinglePosition(TARGET_START)
                }
                : new[] { GridPositionGroup.CreateForSinglePosition(TARGET_START) };
            ActionExecutionContext actionContext = new(actionId, USER_ID, new ActionSelectionContext(new ValueArray<GridPositionGroup>(selections)));
            SimulationOptions simulationOptions = new(new FixedRandomResolver(rollResult));
            return CombatSimulator.Simulate(world.Database, world.RandomState, world.CombatInstance, world.CombatState, actionContext, simulationOptions);
        }

        private static CombatDefinition CreateCombatDefinition()
        {
            List<BlockInfo> floorTiles = new();
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 8; x++)
                    floorTiles.Add(new BlockInfo("floor", new GridPosition(x, y), Rotation_Restricted_Four._0, Height: 0));
            }
            return new CombatDefinition(
                "event-test-combat",
                new TileGridInfo(new Vector2Int(8, 4), Vector2.One, Vector2.Zero),
                new GridBlocksInfo(new ValueArray<BlockInfo>(floorTiles)),
                ValueArray<UnitPlacementOptionInfo>.Empty,
                ValueArray<UnitPlacementInfo>.Empty,
                new CombatRewardsInfo(HasArtifactRewards: false, ValueArray<string>.Empty));
        }

        private static UnitDefinition UnitDefinitionFor(string id, TeamAffiliation affiliation, CombatStats stats) => new(
            id,
            stats.MaxHealthInfo,
            stats.AttributeStats,
            stats.Movement,
            stats.MaxActionPoints,
            affiliation,
            CanBeRevived: false,
            ShouldTakeATurn: true,
            ShouldBlockOccupiedTile: true,
            CanBeMoved: true,
            CanBeTargeted: true,
            MoveCombatActionId: string.Empty,
            ValueArray<string>.Empty,
            ValueArray<string>.Empty);

        private static UnitState UnitStateFor(
            CombatUnitUniqueIdentifier identifier,
            TeamAffiliation affiliation,
            GridPosition position,
            int CurrentHealth,
            int CurrentActionPoints,
            EightDirection facing,
            ValueArray<PassiveState> passives = default) => new(
                identifier,
                affiliation,
                CurrentHealth,
                CurrentActionPoints,
                HasTakenTurn: false,
                position,
                facing,
                passives,
                ValueArray<string>.Empty,
                ValueArray<TemporaryStatModifierState>.Empty,
                UnitIdentifierWhoMostRecentlyDamagedMe: null);

        private static PassiveDefinition CreatePassive(string id, PassiveActivateTime activateTime, ExpirationInfo expirationInfo) => new(
            id,
            CanStack: true,
            activateTime,
            CanActivateWhenDead: false,
            new PassiveConditionInfo_Always(),
            HasActivationLimitPerEncounter: false,
            MaxActivationsPerEncounter: 0,
            PassiveActionInfo_None.Empty,
            new ExpirationInfoGroup(new ValueArray<ExpirationInfo>(expirationInfo)),
            Function("1"),
            PassiveActionInfo_None.Empty);

        private static TileLocationInfo Location(TileLocation tileLocation) => new(tileLocation, Value: 0, SelectionSpecificationsInfo.Empty);

        private static HitInfo Hit(string function, HitRulesInfo hitRules) => new(Function(function), HasMinimumValue: false, MinimumValue: 0, hitRules, ShouldInvokeOnHitEvent: false);

        private static NCalcFunctionInfo Function(string function) => new(function, ValueArray<NCalcVariableInfo>.Empty);

        private sealed record TestWorld(GameDatabase Database, DeterministicRandomState RandomState, CombatInstance CombatInstance, CombatState CombatState);

        private sealed class FixedRandomResolver(HitRollResult result) : IRandomResolutionResolver
        {
            public HitRollResult RollHitChance(int chance) => result;
            public float RollWeight(float totalWeight) => 0;
            public int RollRange(int exclusiveMax) => 0;
        }
    }
}
