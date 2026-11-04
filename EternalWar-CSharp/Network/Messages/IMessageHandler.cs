// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages;

namespace EternalWar.Client.Application.Network.Messages
{
    internal interface IMessageHandler
    {
        void Handle(TypedMessage typedMessage);
    }

    internal abstract class MessageHandler<T> : IMessageHandler
    {
        public void Handle(TypedMessage typedMessage)
        {
            T data = typedMessage.ExtractData<T>();
            Handle(data);
        }

        protected abstract void Handle(T data);
    }
}
