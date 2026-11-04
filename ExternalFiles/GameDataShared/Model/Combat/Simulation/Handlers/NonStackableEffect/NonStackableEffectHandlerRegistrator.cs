// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.NonStackableEffect
{
    internal sealed class NonStackableEffectHandlerRegistrator
    {
        private static readonly INonStackableEffectHandler[] _handlers = new INonStackableEffectHandler[]
        {
            new NonStackableEffectHandler_SkipTurn(),
            new NonStackableEffectHandler_DoubleTurn(),
            new NonStackableEffectHandler_Invincible(),
            new NonStackableEffectHandler_ImmuneToSpecificPassive()
        };

        private readonly Dictionary<Type, INonStackableEffectHandler> _handlersMap;

        public NonStackableEffectHandlerRegistrator()
        {
            _handlersMap = new Dictionary<Type, INonStackableEffectHandler>(_handlers.Length);
            foreach (INonStackableEffectHandler handler in _handlers)
            {
                Type infoType = handler.InfoType;
                _handlersMap.Add(infoType, handler);
            }
        }

        public INonStackableEffectHandler GetHandler(NonStackableEffectInfo info)
        {
            Type infoType = info.GetType();
            return _handlersMap[infoType];
        }
    }
}
