// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep
{
    internal sealed class CombatStepHandlerRegistrator
    {
        private static readonly ICombatStepHandler[] _handlers = new ICombatStepHandler[]
        {
            new CombatStepHandler_Move(),
            new CombatStepHandler_Melee(),
            new CombatStepHandler_Heal(),
            new CombatStepHandler_Push(),
            new CombatStepHandler_ModifyStat(),
            new CombatStepHandler_AddRemoveNonStackableEffect(),
            new CombatStepHandler_AddPassive(),
        };

        private readonly Dictionary<Type, ICombatStepHandler> _handlersMap = new();

        public CombatStepHandlerRegistrator()
        {
            _handlersMap = new Dictionary<Type, ICombatStepHandler>(_handlers.Length);
            foreach (ICombatStepHandler handler in _handlers)
            {
                Type stepInfoType = handler.StepInfoType;
                _handlersMap.Add(stepInfoType, handler);
            }
        }

        public ICombatStepHandler GetHandler(CombatStepInfo stepInfo)
        {
            Type stepInfoType = stepInfo.GetType();
            return _handlersMap[stepInfoType];
        }
    } 
}
