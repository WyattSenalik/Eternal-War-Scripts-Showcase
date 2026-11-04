// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Map.Runtime;
using System;

namespace EternalWar.Core.Model.Run.Runtime
{
    public static class RunStateTransforms
    {
        public static RunState ReplacePhase(this RunState runState, RunPhase newPhase)
        {
            return runState with
            {
                Phase = newPhase
            };
        }

        public static RunState TransformCombatState(this RunState runState, Func<CombatRuntime, CombatState> transformFunc)
        {
            var combatRuntime = RunStateAccessors.GetRunPhase<CombatRuntime>(runState);
            CombatState newCombatState = transformFunc.Invoke(combatRuntime);
            return ReplaceCombatState(runState, newCombatState);
        }

        public static RunState TransformMapState(this RunState runState, Func<LocationMapState, LocationMapState> transformFunc)
        {
            LocationMapState locationMapState = RunStateAccessors.GetLocationMapState(runState);
            LocationMapState newLocationMapState = transformFunc.Invoke(locationMapState);
            return runState with
            {
                CurrentMap = newLocationMapState
            };

        }

        private static RunState ReplaceCombatState(this RunState runState, CombatState newCombatState)
        {
            CombatRuntime oldCombatRuntime = RunStateAccessors.GetRunPhase<CombatRuntime>(runState);

            CombatRuntime newCombatRuntime = oldCombatRuntime with
            {
                State = newCombatState
            };
            return runState with
            {
                Phase = newCombatRuntime
            };
        }
    }

    public static class RunRuntimeTransforms
    {
        public static RunRuntime ReplacePhase(this RunRuntime runRuntime, RunPhase newPhase)
        {
            RunState oldRunState = runRuntime.State;
            return runRuntime with
            {
                State = oldRunState.ReplacePhase(newPhase)
            };
        }

        public static RunRuntime TransformCombatState(this RunRuntime runRuntime, Func<CombatRuntime, CombatState> transformFunc)
        {
            RunState oldRunState = runRuntime.State;
            return runRuntime with
            {
                State = oldRunState.TransformCombatState(transformFunc)
            };
        }
    }
}