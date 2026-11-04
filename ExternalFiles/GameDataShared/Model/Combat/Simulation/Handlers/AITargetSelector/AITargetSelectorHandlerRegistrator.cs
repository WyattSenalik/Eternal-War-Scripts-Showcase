// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector
{
    internal sealed class AITargetSelectorHandlerRegistrator
    {
        private static readonly IAITargetSelectorHandler[] _handlers = new IAITargetSelectorHandler[]
        {
            new AITargetSelectorHandler_User(),
            new AITargetSelectorHandler_CurrentSubject(),
            new AITargetSelectorHandler_ClosestSpecificUnit(),
            new AITargetSelectorHandler_ClosestUnitWithRelation()
        };

        private readonly Dictionary<Type, IAITargetSelectorHandler> _handlersMap = new();

        public AITargetSelectorHandlerRegistrator()
        {
            _handlersMap = new Dictionary<Type, IAITargetSelectorHandler>(_handlers.Length);
            foreach (IAITargetSelectorHandler handler in _handlers)
            {
                Type targetSelectorType = handler.TargetSelectorType;
                _handlersMap.Add(targetSelectorType, handler);
            }
        }

        public IAITargetSelectorHandler GetHandler(AITargetSelectorInfo targetSelectorInfo)
        {
            Type targetSelectorType = targetSelectorInfo.GetType();
            return _handlersMap[targetSelectorType];
        }
    }
}