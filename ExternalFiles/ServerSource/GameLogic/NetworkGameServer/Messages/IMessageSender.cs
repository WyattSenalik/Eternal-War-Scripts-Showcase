// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Server.Application.Messages
{
    internal interface IMessageSender
    {
        void SendMessageToClient(Guid clientId, string message);
    }
}