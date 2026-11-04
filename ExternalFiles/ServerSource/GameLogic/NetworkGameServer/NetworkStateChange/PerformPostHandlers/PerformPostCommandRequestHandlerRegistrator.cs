// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Server.Application.NetworkStateChange.PerformPostHandlers
{
    internal sealed class PerformPostCommandRequestHandlerRegistrator
    {
        private readonly Dictionary<Type, IPerformPostCommandRequestHandler> _handlerMap = new();

        public PerformPostCommandRequestHandlerRegistrator(ServerGameSession session, GameDatabase database)
        {
            IPerformPostCommandRequestHandler[] handlers =
            {
                new PerformPostCommandRequestHandler_Lobby_ToggleReady(session, database),
                new PerformPostCommandRequestHandler_Run_Combat(session, database),
                new PerformPostCommandRequestHandler_Run_Map_VotingChanged(session),
            };

            foreach (IPerformPostCommandRequestHandler handler in handlers)
            {
                _handlerMap.Add(handler.RequestType, handler);
            }
        }

        public bool TryGetHandler(NetworkGameCommandRequest request, [NotNullWhen(true)] out IPerformPostCommandRequestHandler? handler)
        {
            Type requestRuntimeType = request.GetType();
            if (_handlerMap.TryGetValue(requestRuntimeType, out handler))
                return true;

            foreach (KeyValuePair<Type, IPerformPostCommandRequestHandler> entry in _handlerMap)
            {
                if (!entry.Key.IsAssignableFrom(requestRuntimeType))
                    continue;
                handler = entry.Value;
                return true;
            }
            handler = null;
            return false;
        }
    }
}
