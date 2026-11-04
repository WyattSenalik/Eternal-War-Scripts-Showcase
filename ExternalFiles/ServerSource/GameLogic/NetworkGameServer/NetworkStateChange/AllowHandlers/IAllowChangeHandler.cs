// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using System;

namespace EternalWar.Server.Application.NetworkStateChange.AllowHandlers
{
    internal interface IAllowChangeHandler
    {
        Type ChangeType { get; }

        bool ShouldAllowChange(GameDatabase database, NetworkGameState networkGameState, Guid clientIdRequestingChange, NetworkGameCommandRequest changeRequested);
    }

    internal abstract class AllowChangeHandler<T> : IAllowChangeHandler
    {
        public Type ChangeType => typeof(T);

        public bool ShouldAllowChange(GameDatabase database, NetworkGameState networkGameState, Guid clientIdRequestingChange, NetworkGameCommandRequest changeRequested)
        {
            return changeRequested is T castedChange 
                ? ShouldAllowChange(database, networkGameState, clientIdRequestingChange, castedChange) 
                : throw new Exception($"{GetType()} for {ChangeType} cannot handle a change of {changeRequested}");
        }

        protected abstract bool ShouldAllowChange(GameDatabase database, NetworkGameState networkGameState, Guid clientIdRequestingChange, T changeRequested);
    }
}

