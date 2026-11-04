// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages;
using System;

namespace EternalWar.Server.Application.Messages
{
    internal interface IMessageHandler
    {
        void Handle(TypedMessageWithClient typedMessageWithClient);
    }

    internal abstract class MessageHandler<T> : IMessageHandler
    {
        public void Handle(TypedMessageWithClient typedMessageWithClient)
        {
            TypedMessage typedMessage = typedMessageWithClient.TypedMessage;
            Guid clientGuid = typedMessageWithClient.ClientId;
            T data = typedMessage.ExtractData<T>();
            Handle(clientGuid, data);
        }

        protected abstract void Handle(Guid clientGuid, T messageData);
    }
}