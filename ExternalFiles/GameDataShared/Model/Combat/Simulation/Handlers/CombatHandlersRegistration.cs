// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector;
using EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep;
using EternalWar.Core.Model.Combat.Simulation.Handlers.NonStackableEffect;
using EternalWar.Core.Model.Combat.Simulation.Handlers.Passive.Action;
using EternalWar.Core.Model.Combat.Simulation.Handlers.Passive.Condition;
using EternalWar.Core.Model.Combat.Simulation.Handlers.PushDirection;
using EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionRangeType;
using EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionType;
using EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation;
using EternalWar.Core.Model.Combat.Simulation.Handlers.UnitDecisionController;
using Direction = EternalWar.Core.Model.Combat.Definitions.PushDirection;
using Location = EternalWar.Core.Model.Combat.Definitions.TileLocation;
using RangeType = EternalWar.Core.Model.Combat.Definitions.SelectionRangeType;
using TargetSelectionType = EternalWar.Core.Model.Combat.Definitions.SelectionType;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers
{
    internal static class CombatHandlersRegistration
    {
        private static readonly CombatStepHandlerRegistrator _combatStepHandlerRegistrator = new();
        private static readonly HandlerRegistrator<Location, ITileLocationHandler> _tileLocationHandlerRegistrator = new(TileLocationHandlerFactory.CreateHandler);
        private static readonly HandlerRegistrator<Direction, IPushDirectionHandler> _pushDirectionHandlerRegistrator = new(PushDirectionHandlerFactory.CreateHandler);
        private static readonly NonStackableEffectHandlerRegistrator _nonStackableEffectHandlerRegistrator = new();
        private static readonly PassiveActionHandlerRegistrator _passiveActionHandlerRegistrator = new();
        private static readonly PassiveConditionHandlerRegistrator _passiveConditionHandlerRegistrator = new();
        private static readonly UnitDecisionControllerHandlerRegistrator _unitDecisionControllerHandlerRegistrator = new();
        private static readonly HandlerRegistrator<TargetSelectionType, ISelectionTypeHandler> _selectionTypeHandlerRegistrator = new(SelectionTypeHandlerFactory.CreateHandler);
        private static readonly HandlerRegistrator<RangeType, ISelectionRangeTypeHandler> _selectionRangeTypeHandlerRegistrator = new(SelectionRangeTypeHandlerFactory.CreateHandler);
        private static readonly AIGoalHandlerRegistrator _aiGoalHandlerRegistrator = new();
        private static readonly AIConditionHandlerRegistrator _aiConditionHandlerRegistrator = new();
        private static readonly AITargetSelectorHandlerRegistrator _aiTargetSelectorHandlerRegistrator = new();

        public static ICombatStepHandler GetHandler(CombatStepInfo stepInfo) => _combatStepHandlerRegistrator.GetHandler(stepInfo);
        public static ITileLocationHandler GetHandler(Location tileLocation) => _tileLocationHandlerRegistrator.GetHandler(tileLocation);
        public static IPushDirectionHandler GetHandler(Direction pushDirection) => _pushDirectionHandlerRegistrator.GetHandler(pushDirection);
        public static INonStackableEffectHandler GetHandler(NonStackableEffectInfo info) => _nonStackableEffectHandlerRegistrator.GetHandler(info);
        public static IPassiveActionHandler GetHandler(PassiveActionInfo actionInfo) => _passiveActionHandlerRegistrator.GetHandler(actionInfo);
        public static IPassiveConditionHandler GetHandler(PassiveConditionInfo conditionInfo) => _passiveConditionHandlerRegistrator.GetHandler(conditionInfo);
        public static IUnitDecisionControllerHandler GetHandler(UnitDecisionControllerInfo decisionControllerInfo) => _unitDecisionControllerHandlerRegistrator.GetHandler(decisionControllerInfo);
        public static ISelectionTypeHandler GetHandler(TargetSelectionType selectionType) => _selectionTypeHandlerRegistrator.GetHandler(selectionType);
        public static ISelectionRangeTypeHandler GetHandler(RangeType rangeType) => _selectionRangeTypeHandlerRegistrator.GetHandler(rangeType);
        public static IAIGoalHandler GetHandler(AIGoalInfo goalInfo) => _aiGoalHandlerRegistrator.GetHandler(goalInfo);
        public static IAIConditionHandler GetHandler(AIConditionInfo conditionInfo) => _aiConditionHandlerRegistrator.GetHandler(conditionInfo);
        public static IAITargetSelectorHandler GetHandler(AITargetSelectorInfo targetSelectorInfo) => _aiTargetSelectorHandlerRegistrator.GetHandler(targetSelectorInfo);
    }
}
