// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector
{
    public sealed record AITargetSelectionHandlerContext(GameDatabase Database, CombatInstance CombatInstance, CombatState PreviousState, CombatUnitUniqueIdentifier UserIdentifier, CombatUnitUniqueIdentifier SubjectIdentifier);

    internal interface IAITargetSelectorHandler
    {
        Type TargetSelectorType { get; }

        CombatUnitUniqueIdentifier? DetermineTarget(AITargetSelectionHandlerContext context, AITargetSelectorInfo targetSelectorInfo);
    }

    public abstract class AITargetSelectorHandler<TTargetSelectorInfo> : IAITargetSelectorHandler where TTargetSelectorInfo : AITargetSelectorInfo
    {
        public Type TargetSelectorType => typeof(TTargetSelectorInfo);

        public CombatUnitUniqueIdentifier? DetermineTarget(AITargetSelectionHandlerContext context, AITargetSelectorInfo targetSelectorInfo)
        {
            return targetSelectorInfo is TTargetSelectorInfo castedTargetSelectorInfo
                ? DetermineTarget(context, castedTargetSelectorInfo)
                : throw new ArgumentException($"{GetType().Name} cannot evaluate a {targetSelectorInfo.GetType().Name}");
        }

        protected abstract CombatUnitUniqueIdentifier? DetermineTarget(AITargetSelectionHandlerContext context, TTargetSelectorInfo targetSelectorInfo);
    }

    public abstract class AITargetSelectorHandler_Closest<TTargetSelectorInfo> : AITargetSelectorHandler<TTargetSelectorInfo> where TTargetSelectorInfo : AITargetSelectorInfo_Closest
    {
        protected sealed override CombatUnitUniqueIdentifier? DetermineTarget(AITargetSelectionHandlerContext context, TTargetSelectorInfo targetSelectorInfo)
        {
            CombatState combatState = context.PreviousState;
            AITargetSelectorInfo relativeToSelectorInfo = targetSelectorInfo.RelativeTo ?? throw new InvalidOperationException($"{targetSelectorInfo.GetType().Name} has a null RelativeTo.");
            DistanceType distanceType = targetSelectorInfo.DistanceComparisonType;

            CombatUnitUniqueIdentifier? relativeToUnitIdentifier = relativeToSelectorInfo.GetTargetIdentifier(context);
            if (relativeToUnitIdentifier == null)
                return null;

            GridPosition relativeToPosition = combatState.GetUnitGridPosition(relativeToUnitIdentifier);
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;

            float closestDistance = float.PositiveInfinity;
            CombatUnitUniqueIdentifier? closestUnitIdentifier = null;
            foreach (UnitState candidateUnit in unitMap.Values)
            {
                // Ignore dead units (TODO: Maybe don't? For reviving friendlies later?)
                if (candidateUnit.IsDead())
                    continue;

                if (!DoesSatisfy(context, targetSelectorInfo, candidateUnit))
                    continue;

                GridPosition candidatePosition = candidateUnit.GridPosition;
                float distance = relativeToPosition.DistanceTo(candidatePosition, distanceType);
                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
                closestUnitIdentifier = candidateUnit.UniqueIdentifier;
            }

            return closestUnitIdentifier;
        }

        protected abstract bool DoesSatisfy(AITargetSelectionHandlerContext context, TTargetSelectorInfo targetSelectorInfo, UnitState candidateUnit);
    }
}