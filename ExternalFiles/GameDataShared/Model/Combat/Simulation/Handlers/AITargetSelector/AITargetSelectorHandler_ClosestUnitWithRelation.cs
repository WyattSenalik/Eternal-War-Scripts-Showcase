// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector
{
    internal sealed class AITargetSelectorHandler_ClosestUnitWithRelation : AITargetSelectorHandler_Closest<AITargetSelectorInfo_ClosestUnitWithRelation>
    {
        protected override bool DoesSatisfy(AITargetSelectionHandlerContext context, AITargetSelectorInfo_ClosestUnitWithRelation targetSelectorInfo, UnitState candidateUnit)
        {
            CombatState combatState = context.PreviousState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            TeamRelation requiredRelation = targetSelectorInfo.Relation;

            CombatUnitUniqueIdentifier candidateIdentifier = candidateUnit.UniqueIdentifier;
            TeamRelation actualRelation = combatState.DetermineRelationBetween(userIdentifier, candidateIdentifier);

            return actualRelation == requiredRelation;
        }
    }
}