// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal sealed record AIGoalTargetLocator(GameDatabase Database, CombatInstance CombatInstance, CombatState PreviousState, CombatUnitUniqueIdentifier UserIdentifier) : IAIGoalTargetLocator
    {
        public CombatUnitUniqueIdentifier? DetermineTarget(AITargetSelectorInfo targetSelectorInfo, CombatUnitUniqueIdentifier originalSubjectIdentifier)
        {
            AITargetSelectionHandlerContext targetSelectionContext = new(Database, CombatInstance, PreviousState, UserIdentifier, originalSubjectIdentifier);

            return targetSelectorInfo.GetTargetIdentifier(targetSelectionContext);
        }

        public GridPosition DetermineStartingPosition(CombatUnitUniqueIdentifier unitIdentifier)
        {
            return PreviousState.GetUnitGridPosition(unitIdentifier);
        }
    }
}