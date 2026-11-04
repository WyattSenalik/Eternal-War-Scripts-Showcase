// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using System;
using System.Collections.Generic;

namespace EternalWar.Server.Application.NetworkStateChange.AllowHandlers
{
    internal sealed class AllowChangeHandlerRegistrator
    {
        private static readonly IAllowChangeHandler[] _handlers = new IAllowChangeHandler[]
        {
            new AllowChangeHandler_Lobby_ToggleReady(),
            new AllowChangeHandler_Run_Combat_EndTurn(),
            new AllowChangeHandler_Run_Combat_ExecuteActionSequence(),
            new AllowChangeHandler_Run_Map_VoteForNode()
        };

        private readonly Dictionary<Type, IAllowChangeHandler> _handlerMap = new();

        public AllowChangeHandlerRegistrator()
        {
            foreach (IAllowChangeHandler handler in _handlers)
            {
                Type type = handler.ChangeType;
                _handlerMap.Add(type, handler);
            }
        }

        public IAllowChangeHandler GetHandler(NetworkGameCommandRequest stateChange)
        {
            Type stateChangeType = stateChange.GetType();
            return _handlerMap[stateChangeType];
        }
    }
}

