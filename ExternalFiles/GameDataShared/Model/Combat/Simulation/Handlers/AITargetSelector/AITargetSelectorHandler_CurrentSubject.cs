// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector
{
    internal sealed class AITargetSelectorHandler_CurrentSubject : AITargetSelectorHandler<AITargetSelectorInfo_CurrentSubject>
    {
        protected override CombatUnitUniqueIdentifier? DetermineTarget(AITargetSelectionHandlerContext context, AITargetSelectorInfo_CurrentSubject targetSelectorInfo)
        {
            return context.SubjectIdentifier;
        }
    }
}