// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages;
using System.Text.Json;
using CustomDebug = EternalWar.Core.Helpers.Debug;

namespace EternalWar.Server.Application.Messages.Debug
{
    internal sealed class MessageHandler_LogWithReceipt(IMessageSender messageSender) : IMessageHandler
    {
        private readonly IMessageSender _messageSender = messageSender;

        public void Handle(TypedMessageWithClient typedMessageWithClient)
        {
            TypedMessage typedMessage = typedMessageWithClient.TypedMessage;
            object? data = typedMessage.Data;
            if (data == null)
            {
                CustomDebug.PrintError($"Data in {nameof(TypedMessageWithClient)} for {NetworkMessageTypeNames.LOG_WITH_RECEIPT} was null ({typedMessageWithClient})");
                return;
            }
            JsonElement dataElement = (JsonElement)data;
            string? log = dataElement.GetString();
            if (log == null)
            {
                CustomDebug.PrintError($"Failed to get string log from data element {dataElement}");
                return;
            }
            CustomDebug.Print($"[{typedMessageWithClient.ClientId}] [{typedMessage.Type}] {log}");
            string receipt = $"Receipt for ({log})";
            TypedMessage receiptMessage = new(NetworkMessageTypeNames.LOG, receipt);
            _messageSender.SendMessageToClient(typedMessageWithClient.ClientId, receiptMessage.Serialize());
        }
    }
}