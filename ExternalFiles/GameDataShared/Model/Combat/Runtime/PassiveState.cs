// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public sealed record PassiveState(string PassiveDefinitionId, int Activations, int Stacks, ValueArray<PassiveExpirationProgress> ExpirationProgresses);

    public sealed record PassiveExpirationProgress(int RuleIndex, int Occurrences);

    public static class PassiveStateExtensions
    {
        public static int FindIndexOfPassive(this IReadOnlyList<PassiveState> passives, string passiveId)
        {
            for (int i = 0; i < passives.Count; ++i)
            {
                PassiveState passiveState = passives[i];
                if (passiveState.PassiveDefinitionId == passiveId)
                    return i;
            }
            return -1;
        }

        public static PassiveState FindPassive(this IReadOnlyList<PassiveState> passives, string passiveId)
        {
            for (int i = 0; i < passives.Count; ++i)
            {
                PassiveState passiveState = passives[i];
                if (passiveState.PassiveDefinitionId == passiveId)
                    return passiveState;
            }
            throw new Exception($"Failed to find passive with id ({passiveId}) in [{string.Join(", ", passives)}]");
        }
    }
}
