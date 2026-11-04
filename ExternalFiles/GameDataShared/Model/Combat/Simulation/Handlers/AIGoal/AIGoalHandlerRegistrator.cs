// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal sealed class AIGoalHandlerRegistrator
    {
        private static readonly IAIGoalHandler[] _handlers = new IAIGoalHandler[]
        {
            new AIGoalHandler_Kill(),
            new AIGoalHandler_DealDamage(),
            new AIGoalHandler_AddNonStackableEffect(),
            new AIGoalHandler_MoveCloserToTarget(),
            new AIGoalHandler_AddPassive()
        };

        private readonly Dictionary<Type, IAIGoalHandler> _handlersMap = new();

        public AIGoalHandlerRegistrator()
        {
            _handlersMap = new Dictionary<Type, IAIGoalHandler>(_handlers.Length);
            foreach (IAIGoalHandler handler in _handlers)
            {
                Type stepInfoType = handler.GoalInfoType;
                _handlersMap.Add(stepInfoType, handler);
            }
        }

        public IAIGoalHandler GetHandler(AIGoalInfo goalInfo)
        {
            Type goalInfoType = goalInfo.GetType();

            return _handlersMap[goalInfoType];
        }
    }
}