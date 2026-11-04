// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.Passive.Action
{
    internal sealed class PassiveActionHandlerRegistrator
    {
        private static readonly IPassiveActionHandler[] _handlers = new IPassiveActionHandler[]
        {
            new PassiveActionHandler_CombatAction(),
            new PassiveActionHandler_None()
        };

        private readonly Dictionary<Type, IPassiveActionHandler> _handlersMap;

        public PassiveActionHandlerRegistrator()
        {
            _handlersMap = new Dictionary<Type, IPassiveActionHandler>(_handlers.Length);
            foreach (IPassiveActionHandler handler in _handlers)
            {
                Type actionInfoType = handler.ActionInfoType;
                _handlersMap.Add(actionInfoType, handler);
            }
        }

        public IPassiveActionHandler GetHandler(PassiveActionInfo actionInfo)
        {
            Type actionInfoType = actionInfo.GetType();
            return _handlersMap[actionInfoType];
        }
    }
}
