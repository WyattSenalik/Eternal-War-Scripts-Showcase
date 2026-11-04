// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages;
using EternalWar.Server.Application.Networking;

namespace EternalWar.Server.Application.Messages
{
    internal sealed class MessageListener
    {
        public void HandleMessage(NetworkGameServerTransport.ReceivedMessage messageData)
        {
            TypedMessage? typedMessage = messageData.Message.DeserializeIntoTypedMessage();
            if (typedMessage == null)
            {
                EternalWar.Core.Helpers.Debug.PrintError($"Failed to deserialize network message ({messageData}) into {nameof(TypedMessage)}");
                return;
            }
            IMessageHandler? handler = MessageHandlerRegistration.GetHandler(typedMessage.Type);
            if (handler == null)
            {
                EternalWar.Core.Helpers.Debug.PrintError($"Failed to get {nameof(IMessageHandler)} for {nameof(typedMessage)} ({typedMessage})");
                return;
            }
            handler.Handle(new TypedMessageWithClient(typedMessage, messageData.ClientId));
        }
    }
}
