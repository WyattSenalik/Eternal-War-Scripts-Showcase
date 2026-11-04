// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Run.Runtime;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public sealed record CombatRuntime(CombatInstance Instance, CombatState State) : RunPhase
    {
        public static implicit operator CombatInstance(CombatRuntime runtime) => runtime.Instance;
        public static implicit operator CombatState(CombatRuntime runtime) => runtime.State;
    }
}
