// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector
{
    internal sealed class AITargetSelectorHandler_User : AITargetSelectorHandler<AITargetSelectorInfo_User>
    {
        protected override CombatUnitUniqueIdentifier? DetermineTarget(AITargetSelectionHandlerContext context, AITargetSelectorInfo_User targetSelectorInfo)
        {
            return context.UserIdentifier;
        }
    }
}