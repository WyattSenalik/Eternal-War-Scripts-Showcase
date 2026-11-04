// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages;

namespace EternalWar.Client.Application.Network.Messages
{
    internal sealed class NetworkMessageReceiver
    {
        public void Update()
        {
            while (NetworkHelpers.TryDequeueIncomingMessage(out string? message))
            {
                if (message is null)
                    continue;

                TypedMessage? typedMessage = message.DeserializeIntoTypedMessage();
                if (typedMessage == null)
                {
                    EternalWar.Core.Helpers.Debug.PrintError($"Failed to deserialize message ({message}) into a {nameof(TypedMessage)}");
                    continue;
                }

                IMessageHandler handler = MessageHandlerRegistration.GetHandler(typedMessage.Type);
                if (handler == null)
                {
                    EternalWar.Core.Helpers.Debug.PrintError($"Null message handler for message with type ({typedMessage.Type})");
                    continue;
                }
                //GameDataShared.Helpers.Debug.Print($"Handling message: {typedMessage}");
                handler.Handle(typedMessage);
            }
        }
    }
}
