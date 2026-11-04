// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public sealed record VictoryRewardsInstance(VictoryRewardsInstance_Stats Stats, VictoryRewardsInstance_Artifacts? Artifacts);

    public sealed record VictoryRewardsInstance_Artifacts(ValueArray<VictoryArtifactChoiceForUnit> ArtifactChoicesForUnits);

    public sealed record VictoryRewardsInstance_Stats(ValueArray<VictoryStatChoicesForUnit> StatChoicesForUnits);

    public sealed record VictoryArtifactChoiceForUnit(CombatUnitUniqueIdentifier UnitIdentifier, ValueArray<string> ArtifactIdChoices);

    public sealed record VictoryStatChoicesForUnit(CombatUnitUniqueIdentifier UnitIdentifier, ValueArray<VictoryStatType> StatChoices);

    public static class VictoryRewardsInstanceExtensions
    {
        public static bool TryGetArtifactRewards(this VictoryRewardsInstance rewardsInstance, [NotNullWhen(true)] out VictoryRewardsInstance_Artifacts? artifactRewards)
        {
            artifactRewards = rewardsInstance.Artifacts;
            if (artifactRewards == null)
                return false;

            ValueArray<VictoryArtifactChoiceForUnit> artifactChoicesForUnit = artifactRewards.ArtifactChoicesForUnits;
            if (artifactChoicesForUnit.Count <= 0)
                return false;

            foreach (VictoryArtifactChoiceForUnit singleUnitChoice in artifactChoicesForUnit)
            {
                ValueArray<string> artifactIdChoices = singleUnitChoice.ArtifactIdChoices;
                if (artifactIdChoices.Count > 0)
                    return true;
            }
            return false;
        }
    }
}
