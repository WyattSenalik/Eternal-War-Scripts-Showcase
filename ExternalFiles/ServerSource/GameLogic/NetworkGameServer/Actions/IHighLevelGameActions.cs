// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using System;

namespace EternalWar.Server.Application.Actions
{
    internal interface IHighLevelGameActions
    {
        void ClientStateDiverged(Guid clientGuid);
        void ClientRequestCommand(Guid requestingClientId, ChangeRequest requestedCommand);
    }
}
