// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Server.Application.Actions
{
    internal interface IHighLevelNetworkActions
    {
        void ClientConnected(Guid clientGuid);
        void ClientDisconnected(Guid disconnectedClientGuid);
    }
}