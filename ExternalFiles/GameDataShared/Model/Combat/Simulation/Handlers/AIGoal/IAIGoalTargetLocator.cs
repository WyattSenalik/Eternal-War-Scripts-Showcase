// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal interface IAIGoalTargetLocator
    {
        CombatUnitUniqueIdentifier? DetermineTarget(AITargetSelectorInfo targetSelectorInfo, CombatUnitUniqueIdentifier originalSubjectIdentifier);

        /// <summary>Position the unit occupied before the candidate simulation ran.</summary>
        GridPosition DetermineStartingPosition(CombatUnitUniqueIdentifier unitIdentifier);
    }
}