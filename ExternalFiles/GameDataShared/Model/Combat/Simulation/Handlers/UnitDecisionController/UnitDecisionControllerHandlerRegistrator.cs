// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.UnitDecisionController
{
    internal sealed class UnitDecisionControllerHandlerRegistrator
    {
        private static readonly IUnitDecisionControllerHandler[] _handlers = new IUnitDecisionControllerHandler[]
        {
            new UnitDecisionControllerHandler_GoalOriented(),
            new UnitDecisionControllerHandler_None()
        };

        private readonly Dictionary<Type, IUnitDecisionControllerHandler> _handlersMap = new();

        public UnitDecisionControllerHandlerRegistrator()
        {
            _handlersMap = new Dictionary<Type, IUnitDecisionControllerHandler>(_handlers.Length);
            foreach (IUnitDecisionControllerHandler handler in _handlers)
            {
                Type controllerType = handler.ControllerType;
                _handlersMap.Add(controllerType, handler);
            }
        }

        public IUnitDecisionControllerHandler GetHandler(UnitDecisionControllerInfo decisionControllerInfo)
        {
            Type controllerType = decisionControllerInfo.GetType();
            return _handlersMap[controllerType];
        }
    }
}