// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector
{
    internal sealed class AITargetSelectorHandler_ClosestSpecificUnit : AITargetSelectorHandler_Closest<AITargetSelectorInfo_ClosestSpecificUnit>
    {
        protected override bool DoesSatisfy(AITargetSelectionHandlerContext context, AITargetSelectorInfo_ClosestSpecificUnit targetSelectorInfo, UnitState candidateUnit)
        {
            string specificUnitId = targetSelectorInfo.UnitDefinitionId;
            CombatUnitUniqueIdentifier candidateIdentifier = candidateUnit.UniqueIdentifier;
            string candidateUnitId = candidateIdentifier.DefinitionId;
            return specificUnitId == candidateUnitId;
        }
    }
}