// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector
{
    public static class AITargetSelectorInfoExtensions
    {
        public static CombatUnitUniqueIdentifier? GetTargetIdentifier(this AITargetSelectorInfo targetSelectorInfo, AITargetSelectionHandlerContext context)
        {
            GameDatabase database = context.Database;
            CombatInstance combatInstance = context.CombatInstance;
            CombatState combatState = context.PreviousState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            CombatUnitUniqueIdentifier subjectIdentifier = context.SubjectIdentifier;

            IAITargetSelectorHandler targetSelectorHandler = CombatHandlersRegistration.GetHandler(targetSelectorInfo);
            AITargetSelectionHandlerContext targetSelectionContext = new(database, combatInstance, combatState, userIdentifier, subjectIdentifier);
            return targetSelectorHandler.DetermineTarget(targetSelectionContext, targetSelectorInfo);
        }
    }
}