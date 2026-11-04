// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Map.Runtime;
using EternalWar.Core.Model.Random;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Run.Runtime
{
    public sealed record TEMP_NOTHING_RUN_PHASE : RunPhase;

    [JsonPolymorphic]
    [JsonDerivedType(typeof(TEMP_NOTHING_RUN_PHASE), "TEMP")]
    [JsonDerivedType(typeof(CombatRuntime), "Combat")]
    public abstract record RunPhase;

    // TODO: Make victory rewards held in like new RunPhase? Like VictoryRewardsRuntime
    public sealed record RunState(RunPhase Phase, ValueArray<PlayerUnitUpgradesState> PlayerUpgrades, DeterministicRandomState RandomState, LocationMapState CurrentMap);

    public sealed record DeterministicRandomState(ulong Seed, ulong Position = 0);

    public static class DeterministicRandomStateExtensions
    {
        public static RandomResolutionResolver_Deterministic CreateResolver(this DeterministicRandomState randomState)
        {
            return new RandomResolutionResolver_Deterministic(randomState.Seed, randomState.Position);
        }
    }
}