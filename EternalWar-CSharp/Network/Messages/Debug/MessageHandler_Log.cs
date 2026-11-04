// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages;
using CustomDebug = EternalWar.Core.Helpers.Debug;

namespace EternalWar.Client.Application.Network.Messages.Debug
{
    internal sealed class MessageHandler_Log : IMessageHandler
    {
        public void Handle(TypedMessage typedMessage)
        {
            if (!typedMessage.TryExtractDataAsString(out string? log, out string errorMessage))
            {
                CustomDebug.PrintError(errorMessage);
                return;
            }
            CustomDebug.Print($"[{typedMessage.Type}] ({log})");
        }
    }
}
