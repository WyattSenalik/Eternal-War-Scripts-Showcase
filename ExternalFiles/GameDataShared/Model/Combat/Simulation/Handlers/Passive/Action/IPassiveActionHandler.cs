// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.Passive.Action
{
    public sealed record PassiveActionHandlerContext(CombatCommandProcessor CommandProcessor, CombatUnitUniqueIdentifier OwnerIdentifier, CombatUnitUniqueIdentifier SourceIdentifier, int? PassiveStacks = null);

    public interface IPassiveActionHandler
    {
        Type ActionInfoType { get; }

        void Execute(PassiveActionHandlerContext context, PassiveActionInfo actionInfo);
    }

    public abstract class PassiveActionHandler<TActionInfo> : IPassiveActionHandler where TActionInfo : PassiveActionInfo
    {
        public Type ActionInfoType => typeof(TActionInfo);

        public void Execute(PassiveActionHandlerContext context, PassiveActionInfo actionInfo)
        {
            if (actionInfo is not TActionInfo typedActionInfo)
                throw new ArgumentException($"{GetType().Name} cannot execute a {actionInfo.GetType().Name}");

            Execute(context, typedActionInfo);
        }

        protected abstract void Execute(PassiveActionHandlerContext context, TActionInfo actionInfo);
    }

    public sealed class PassiveActionHandler_CombatAction : PassiveActionHandler<PassiveActionInfo_CombatAction>
    {
        protected override void Execute(PassiveActionHandlerContext context, PassiveActionInfo_CombatAction actionInfo)
        {
            CombatCommandProcessor commandProcessor = context.CommandProcessor;
            GameDatabase database = commandProcessor.Database;
            CombatState combatState = commandProcessor.CurrentState;
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;
            CombatUnitUniqueIdentifier sourceIdentifier = context.SourceIdentifier;
            CombatUnitUniqueIdentifier ownerIdentifier = context.OwnerIdentifier;
            string combatActionId = actionInfo.CombatActionId;

            CombatActionSequenceDefinition? actionSequenceDefinition = database.GetActionSequenceDefinition(combatActionId);

            CombatUnitUniqueIdentifier selectionTargetIdentifier = unitMap.ContainsKey(sourceIdentifier) ? sourceIdentifier : ownerIdentifier;
            GridPosition selectionTargetPosition = combatState.GetUnit(selectionTargetIdentifier).GridPosition;

            using ValueArray<GridPositionGroup>.Builder selectionGroups = new();
            foreach (CombatStepInfo stepInfo in actionSequenceDefinition.Steps)
            {
                if (!stepInfo.DoesUseSelection())
                    continue;
                selectionGroups.Add(GridPositionGroup.CreateForSinglePosition(selectionTargetPosition));
            }
            if (selectionGroups.Count <= 0)
                selectionGroups.Add(GridPositionGroup.EMPTY);

            ActionSelectionContext selectionContext = new(selectionGroups.ToValueArray());
            ActionExecutionContext actionContext = new(combatActionId, ownerIdentifier, selectionContext);

            CombatSimulator.SimulatePassiveAction(commandProcessor, actionContext, actionSequenceDefinition, context.PassiveStacks);
        }
    }

    public sealed class PassiveActionHandler_None : PassiveActionHandler<PassiveActionInfo_None>
    {
        protected override void Execute(PassiveActionHandlerContext context, PassiveActionInfo_None actionInfo) { }
    }
}

