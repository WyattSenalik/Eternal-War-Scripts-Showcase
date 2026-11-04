// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    internal sealed class AIConditionHandlerRegistrator
    {
        private static readonly IAIConditionHandler[] _handlers = new IAIConditionHandler[]
        {
            new AIConditionHandler_And(),
            new AIConditionHandler_Or(),
            new AIConditionHandler_Not(),
            new AIConditionHandler_HasNonStackableEffect(),
            new AIConditionHandler_DistanceToTarget(),
            new AIConditionHandler_IsRelation(),
            new AIConditionHandler_CheckHealth(),
            new AIConditionHandler_HasPassive(),
            new AIConditionHandler_UnitFractionSatisfyingCondition()
        };

        private readonly Dictionary<Type, IAIConditionHandler> _handlersMap;

        public AIConditionHandlerRegistrator()
        {
            _handlersMap = new Dictionary<Type, IAIConditionHandler>(_handlers.Length);
            foreach (IAIConditionHandler handler in _handlers)
            {
                Type conditionType = handler.ConditionType;
                _handlersMap.Add(conditionType, handler);
            }
        }

        public IAIConditionHandler GetHandler(AIConditionInfo conditionInfo)
        {
            Type conditionType = conditionInfo.GetType();
            return _handlersMap[conditionType];
        }
    }
}
