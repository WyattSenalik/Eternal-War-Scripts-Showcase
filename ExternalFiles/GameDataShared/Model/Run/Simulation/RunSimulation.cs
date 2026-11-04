// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System.Numerics;

namespace EternalWar.Core.Model.Run.Simulation
{
    public static class RunSimulation
    {
        public static RunRuntime StartCombat(RunRuntime runRuntime, GameDatabase database, string combatId, Vector3 worldOffset, ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> playerUnitPlacements, out CombatRuntime newCombatRuntime)
        {
            RunInstance oldRunInstance = runRuntime.Instance;
            RunState oldRunState = runRuntime.State;

            newCombatRuntime = CombatInitialization.InitializeCombat(database, oldRunInstance, oldRunState, combatId, worldOffset, playerUnitPlacements, out DeterministicRandomState randomStateResult);

            RunState newRunState = oldRunState with
            {
                Phase = newCombatRuntime,
                RandomState = randomStateResult
            };
            return runRuntime with
            {
                State = newRunState
            };
        }
    }
}