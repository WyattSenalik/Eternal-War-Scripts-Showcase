// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Map.Runtime;

namespace EternalWar.Core.Model.Run.Runtime
{
    public static class RunInitialization
    {
        public static RunRuntime InitializeRun(ValueArray<CombatUnitUniqueIdentifier> playerChosenUnitIdentifiers, ulong RandomnessSeed)
        {
            RunInstance runInstance = new(playerChosenUnitIdentifiers);

            using ValueArray<PlayerUnitUpgradesState>.Builder initialPlayerUpgrades = new();
            foreach (CombatUnitUniqueIdentifier unitIdentifier in playerChosenUnitIdentifiers)
            {
                PlayerUnitUpgradesState upgradesState = InitializePlayerUpgrades(unitIdentifier);
                initialPlayerUpgrades.Add(upgradesState);
            }
            RunPhase runStartingPhase = new TEMP_NOTHING_RUN_PHASE(); // TODO: Make this first combat probably

            DeterministicRandomState randomState = new(RandomnessSeed);
            LocationMapState initialMapState = LocationMapState.CreateInitial(null!); // TODO: Need to generate the map and then give the first node here
            RunState runState = new(runStartingPhase, initialPlayerUpgrades.ToValueArray(), randomState, initialMapState);
            return new RunRuntime(runInstance, runState);
        }

        private static PlayerUnitUpgradesState InitializePlayerUpgrades(CombatUnitUniqueIdentifier unitIdentifier)
        {
            return new PlayerUnitUpgradesState(unitIdentifier, PlayerStatUpgradesState.Empty, ObtainedArtifactIds: ValueArray<string>.Empty, ClassPrestigeChoices: ValueArray<string>.Empty);
        }
    }
}