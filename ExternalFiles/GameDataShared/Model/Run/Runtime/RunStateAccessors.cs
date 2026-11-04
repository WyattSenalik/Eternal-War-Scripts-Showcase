// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Map.Runtime;
using System;
using System.Linq;

namespace EternalWar.Core.Model.Run.Runtime
{
    public static class RunStateAccessors
    {
        public static T GetRunPhase<T>(RunState runState) where T : RunPhase
        {
            RunPhase runPhase = runState.Phase;
            return runPhase is T castedRunPhase ? castedRunPhase : throw new Exception($"RunState is not in the {nameof(T)} phase");
        }

        public static CombatState GetCombatState(RunState runState)
        {
            CombatRuntime combatRuntime = GetRunPhase<CombatRuntime>(runState);
            return combatRuntime.State;
        }

        public static LocationMapState GetLocationMapState(RunState runState)
        {
            LocationMapRuntime locationMapRuntime = GetRunPhase<LocationMapRuntime>(runState);
            return locationMapRuntime.State;
        }

        public static LocationMapInstance GetLocationMapInstance(RunState runState)
        {
            LocationMapRuntime locationMapRuntime = GetRunPhase<LocationMapRuntime>(runState);
            return locationMapRuntime.Instance;
        }

        public static PlayerUnitUpgradesState GetUpgrades(RunState runState, CombatUnitUniqueIdentifier playerUnitIdentifier)
        {
            ValueArray<PlayerUnitUpgradesState> playerUpgrades = runState.PlayerUpgrades;
            return playerUpgrades.First(upgrade => upgrade.PlayerUnitIdentifier == playerUnitIdentifier);
        }
    }
}