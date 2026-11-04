// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.Passive.Condition
{
    internal sealed class PassiveConditionHandlerRegistrator
    {
        private static readonly IPassiveConditionHandler[] _handlers = new IPassiveConditionHandler[]
        {
            new PassiveConditionHandler_AttackedWithinDistance(),
            new PassiveConditionHandler_HealthReducedBelow(),
            new PassiveConditionHandler_Always()
        };

        private readonly Dictionary<Type, IPassiveConditionHandler> _handlersMap;

        public PassiveConditionHandlerRegistrator()
        {
            _handlersMap = new Dictionary<Type, IPassiveConditionHandler>(_handlers.Length);
            foreach (IPassiveConditionHandler handler in _handlers)
            {
                Type conditionInfoType = handler.ConditionInfoType;
                _handlersMap.Add(conditionInfoType, handler);
            }
        }

        public IPassiveConditionHandler GetHandler(PassiveConditionInfo conditionInfo)
        {
            Type conditionInfoType = conditionInfo.GetType();
            return _handlersMap[conditionInfoType];
        }
    }
}
