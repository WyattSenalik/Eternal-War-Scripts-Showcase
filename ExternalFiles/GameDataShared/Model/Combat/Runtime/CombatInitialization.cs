// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using EternalWar.Core.Model.Run.Runtime;
using System;
using System.Linq;
using System.Numerics;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public static class CombatInitialization
    {
        public static CombatRuntime InitializeCombat(GameDatabase database, RunRuntime runRuntime, string combatId, Vector3 worldOffset, ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> playerUnitPlacements, out DeterministicRandomState randomStateResult) => InitializeCombat(database, runRuntime.Instance, runRuntime.State, combatId, worldOffset, playerUnitPlacements, out randomStateResult);
        public static CombatRuntime InitializeCombat(GameDatabase database, RunInstance runInstance, RunState runState, string combatId, Vector3 worldOffset, ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> playerUnitPlacements, out DeterministicRandomState randomStateResult)
        {
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.Builder unitBuildsMapBuilder = ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.CreateBuilder();
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder unitStateMapBuilder = ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.CreateBuilder();

            // Add the player units
            InitializePlayerUnits(database, runInstance, runState, playerUnitPlacements, unitBuildsMapBuilder, unitStateMapBuilder);
            // Add the NPC units
            InitializeNPCUnits(database, combatId, unitBuildsMapBuilder, unitStateMapBuilder);

            TileGridInstance tileGridInstance = new(worldOffset);
            CombatInstance combatInstance = new(combatId, tileGridInstance, unitBuildsMapBuilder.ToValueDictionary());
            CombatState combatState = new(combatId, unitStateMapBuilder.ToValueDictionary());

            CombatUnitUniqueIdentifier? initialTurnUnitIdentifier = CombatTurnHelpers.DeriveInitialTurnState(database, combatInstance).CurrentTurnUnitIdentifier;
            DeterministicRandomState randomState = runState.RandomState;
            if (initialTurnUnitIdentifier != null)
            {
                CombatSimulationHelpers.StartTurn(database, randomState, combatInstance, combatState, initialTurnUnitIdentifier, out combatState, out randomStateResult);
            }
            else
            {
                randomStateResult = randomState;
            }
            
            return new CombatRuntime(combatInstance, combatState);
        }

        private static void InitializePlayerUnits(GameDatabase database, RunInstance runInstance, RunState runState, ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> playerUnitPlacements, ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.Builder outUnitBuildsMapBuilder, ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder outUnitStateMapBuilder)
        {
            ValueArray<CombatUnitUniqueIdentifier> playerUnitIdentifiers = runInstance.PlayerUnitIdentifiers;

            foreach (CombatUnitUniqueIdentifier unitIdentifier in playerUnitIdentifiers)
            {
                (UnitBuild unitBuild, UnitState unitState) = InitializeUnit_Player(database, runState, playerUnitPlacements, unitIdentifier);

                outUnitBuildsMapBuilder.Add(unitIdentifier, unitBuild);
                outUnitStateMapBuilder.Add(unitIdentifier, unitState);
            }
        }

        private static void InitializeNPCUnits(GameDatabase database, string combatId, ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.Builder outUnitBuildsMapBuilder, ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder outUnitStateMapBuilder)
        {
            CombatDefinition combatDefinition = database.GetCombatDefinition(combatId);
            ValueArray<UnitPlacementInfo> nonPlayerUnitPlacements = combatDefinition.NonPlayerUnitPlacements;
            foreach (UnitPlacementInfo unitPlacement in nonPlayerUnitPlacements)
            {
                (UnitBuild unitBuild, UnitState unitState) = InitializeUnit_NPC(database, unitPlacement, outUnitBuildsMapBuilder);
                CombatUnitUniqueIdentifier unitIdentifier = unitState.UniqueIdentifier;

                outUnitBuildsMapBuilder.Add(unitIdentifier, unitBuild);
                outUnitStateMapBuilder.Add(unitIdentifier, unitState);
            }
        }

        private static (UnitBuild, UnitState) InitializeUnit_Player(GameDatabase database, RunState runState, ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> playerUnitPlacements, CombatUnitUniqueIdentifier unitIdentifier)
        {
            PlayerUnitUpgradesState upgrades = RunStateAccessors.GetUpgrades(runState, unitIdentifier);
            PlayerStatUpgradesState statUpgrades = upgrades.StatUpgrades;
            ValueArray<string> obtainedArtifactIds = upgrades.ObtainedArtifactIds;
            CombatStats statChanges = statUpgrades.CreateStatChanges();
            string classUnitDefinitionId = upgrades.GetCurrentClassUnitDefinitionId();

            UnitDefinition classUnitDefinition = database.GetUnitDefinition(classUnitDefinitionId);
            TeamAffiliation startingAffiliation = classUnitDefinition.StartingAffiliation;
            ValueArray<string> combatActionIds = classUnitDefinition.CombatActionIds;
            ValueArray<string> startingPassiveIdsFromUnit = classUnitDefinition.StartingPassiveIds;

            UnitPlacementOptionInfo placementOptionInfo = playerUnitPlacements[unitIdentifier];
            GridPosition position = placementOptionInfo.Position;
            EightDirection facingDirection = placementOptionInfo.FacingDirection;

            ValueArray<string> allStartingPassiveIds = DetermineAllStartingPassiveIds(database, startingPassiveIdsFromUnit, obtainedArtifactIds);
            ValueArray<PassiveState> passiveStates = CreateStartingPassives(allStartingPassiveIds);

            // TODO: Will probably need some way to add more actions.

            UnitBuild unitBuild = new(unitIdentifier, position, facingDirection, statChanges, combatActionIds, obtainedArtifactIds);

            ValueArray<TemporaryStatModifierState> temporaryStatMods = CreateStatModifiersFromArtifacts(database, classUnitDefinitionId, unitBuild, obtainedArtifactIds, out CombatStats resultingStats);
            UnitState unitState = new(unitIdentifier, startingAffiliation, resultingStats.MaxHealthInfo.Health, resultingStats.MaxActionPoints, HasTakenTurn: false, position, facingDirection, passiveStates, ActiveNonStackableEffectIds: ValueArray<string>.Empty, temporaryStatMods, UnitIdentifierWhoMostRecentlyDamagedMe: null);

            return (unitBuild, unitState);
        }

        private static (UnitBuild, UnitState) InitializeUnit_NPC(GameDatabase database, UnitPlacementInfo unitPlacement, ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.Builder outUnitBuildsMapBuilder)
        {
            string unitId = unitPlacement.UnitId;
            UnitPlacementOptionInfo placement = unitPlacement.Placement;
            GridPosition position = placement.Position;
            EightDirection facingDirection = placement.FacingDirection;

            UnitDefinition unitDefinition = database.GetUnitDefinition(unitId);
            HealthInfo baseHealthInfo = unitDefinition.BaseHealth;
            int baseHealth = baseHealthInfo.Health;
            int baseActionPoints = unitDefinition.BaseActionPoints;
            TeamAffiliation startingAffiliation = unitDefinition.StartingAffiliation;
            ValueArray<string> combatActionIds = unitDefinition.CombatActionIds;
            ValueArray<string> startingPassiveIds = unitDefinition.StartingPassiveIds;

            CombatUnitOwner_AI unitOwner = new();
            int existingUnitCountWithSameIdentifier = outUnitBuildsMapBuilder.Count(kvp => kvp.Key.Owner == unitOwner && kvp.Key.DefinitionId == unitId);
            CombatUnitUniqueIdentifier unitIdentifier = new(unitOwner, unitId, existingUnitCountWithSameIdentifier);

            ValueArray<PassiveState> passiveStates = CreateStartingPassives(startingPassiveIds);

            UnitBuild unitBuild = new(unitIdentifier, position, facingDirection, StatChanges: CombatStats.Empty, combatActionIds, ArtifactIds: ValueArray<string>.Empty);
            UnitState unitState = new(unitIdentifier, startingAffiliation, baseHealth, baseActionPoints, HasTakenTurn: false, position, facingDirection, passiveStates, ActiveNonStackableEffectIds: ValueArray<string>.Empty, TemporaryStatModifiers: ValueArray<TemporaryStatModifierState>.Empty, UnitIdentifierWhoMostRecentlyDamagedMe: null);

            return (unitBuild, unitState);
        }

        private static ValueArray<string> DetermineAllStartingPassiveIds(GameDatabase database, ValueArray<string> passiveIdsFromUnit, ValueArray<string> artifactIds)
        {
            using ValueArray<string>.Builder passiveIdsBuilder = new(passiveIdsFromUnit);

            foreach (string artifactId in artifactIds)
            {
                ArtifactDefinition artifactDefinition = database.GetArtifactDefinition(artifactId);
                passiveIdsBuilder.AddRange(artifactDefinition.PassivesToApply);
            }

            return passiveIdsBuilder.ToArray();
        }

        private static ValueArray<PassiveState> CreateStartingPassives(ValueArray<string> passiveIds)
        {
            using ValueArray<PassiveState>.Builder passiveStatesBuilder = new();
            foreach (string passiveId in passiveIds)
            {
                // TODO: Starting stacks should probably be specified in the unit definition.
                PassiveState passiveState = new(passiveId, Activations: 0, Stacks: 1, ExpirationProgresses: ValueArray<PassiveExpirationProgress>.Empty);
                passiveStatesBuilder.Add(passiveState);
            }

            return passiveStatesBuilder.ToValueArray();
        }

        private static ValueArray<TemporaryStatModifierState> CreateStatModifiersFromArtifacts(GameDatabase database, string unitId, UnitBuild unitBuild, ValueArray<string> artifactIds, out CombatStats resultingStats)
        {
            using ValueArray<TemporaryStatModifierState>.Builder statModStates = new();

            // Simulate the combat stats as they are being applied
            UnitDefinition unitDefinition = database.GetUnitDefinition(unitId);
            CombatStats baseStats = unitDefinition.GetBaseStats();
            CombatStats startingStatChanges = unitBuild.StatChanges;
            CombatStats buildStats = baseStats.Merge(startingStatChanges);

            resultingStats = buildStats;
            foreach (string artifactId in artifactIds)
            {
                ArtifactDefinition artifactDefinition = database.GetArtifactDefinition(artifactId);
                foreach (StatModificationInfo statModInfo in artifactDefinition.StatModifications)
                {
                    NCalcFunctionInfo function = statModInfo.NCalcFunction;
                    ValueFunctionContext valueFunctionContext = new(resultingStats);
                    float rawAmount = function.Evaluate(valueFunctionContext);
                    int amount = (int)MathF.Floor(rawAmount);

                    StatType statType = statModInfo.StatToModify;
                    TemporaryStatModifierState tempStatMod = new(statType, amount, ExpirationInfoGroup.Never, ExpirationProgress: 0);
                    statModStates.Add(tempStatMod);

                    // Simulate it increasing the stats so that it is updated for next stat mod.
                    resultingStats = resultingStats.Replace(statType, resultingStats.Get(statType) + amount);
                }
            }

            return statModStates.ToValueArray();
        }
    }
}
