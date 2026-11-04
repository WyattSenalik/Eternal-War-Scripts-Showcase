// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.Passive.Condition
{
    public sealed record PassiveConditionHandlerContext(PassiveConditionInfo Info, CombatState CombatState, CombatUnitUniqueIdentifier PassiveOwnerIdentifier, CombatEvent TriggeringEvent, CombatUnitUniqueIdentifier SourceIdentifier);

    public interface IPassiveConditionHandler
    {
        Type ConditionInfoType { get; }

        bool DoesSatisfy(PassiveConditionHandlerContext context);
    }

    public abstract class PassiveConditionHandler<TConditionInfo> : IPassiveConditionHandler where TConditionInfo : PassiveConditionInfo
    {
        public Type ConditionInfoType => typeof(TConditionInfo);

        public bool DoesSatisfy(PassiveConditionHandlerContext context)
        {
            return context.Info is TConditionInfo castedConditionInfo
                ? DoesSatisfy(context, castedConditionInfo)
                : throw new ArgumentException($"{GetType().Name} cannot evaluate a {context.Info.GetType().Name}");
        }

        protected abstract bool DoesSatisfy(PassiveConditionHandlerContext context, TConditionInfo conditionInfo);
    }

    public sealed class PassiveConditionHandler_AttackedWithinDistance : PassiveConditionHandler<PassiveConditionInfo_AttackedWithinDistance>
    {
        protected override bool DoesSatisfy(PassiveConditionHandlerContext context, PassiveConditionInfo_AttackedWithinDistance conditionInfo)
        {
            CombatUnitUniqueIdentifier sourceIdentifier = context.SourceIdentifier;
            CombatUnitUniqueIdentifier pasiveOwnerIdentifier = context.PassiveOwnerIdentifier;

            if (context.TriggeringEvent is not CombatEvent_DamageApplied damageEvent)
                return false;
            CombatUnitUniqueIdentifier targetIdentifier = damageEvent.TargetIdentifier;
            if (targetIdentifier != pasiveOwnerIdentifier)
                return false;

            CombatState combatState = context.CombatState;
            UnitState passiveOwner = combatState.GetUnit(pasiveOwnerIdentifier);
            UnitState source = combatState.GetUnit(sourceIdentifier);
            GridPosition passiveOwnerPosition = passiveOwner.GridPosition;
            GridPosition sourcePosition = source.GridPosition;

            float distance = passiveOwnerPosition.DistanceTo(sourcePosition, DistanceType.Manhattan);
            return distance <= conditionInfo.TileDistance;
        }
    }

    public sealed class PassiveConditionHandler_HealthReducedBelow : PassiveConditionHandler<PassiveConditionInfo_HealthReducedBelow>
    {
        protected override bool DoesSatisfy(PassiveConditionHandlerContext context, PassiveConditionInfo_HealthReducedBelow conditionInfo)
        {
            if (context.TriggeringEvent is not CombatEvent_DamageApplied damageEvent ||
                damageEvent.TargetIdentifier != context.PassiveOwnerIdentifier)
                return false;

            int resultingHealth = damageEvent.ResultingHealth;
            int previousHealth = resultingHealth + damageEvent.DamageDealt;
            int healthThreshold = conditionInfo.HealthThreshold;
            return previousHealth > healthThreshold && resultingHealth <= healthThreshold;
        }
    }

    public sealed class PassiveConditionHandler_Always : PassiveConditionHandler<PassiveConditionInfo_Always>
    {
        protected override bool DoesSatisfy(PassiveConditionHandlerContext context, PassiveConditionInfo_Always conditionInfo) => true;
    }
}
