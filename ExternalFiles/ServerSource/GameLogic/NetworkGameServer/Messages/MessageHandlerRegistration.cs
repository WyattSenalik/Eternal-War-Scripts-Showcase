// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages;
using EternalWar.Server.Application.Actions;
using EternalWar.Server.Application.Messages.Debug;
using EternalWar.Server.Application.Messages.HighLevelGame;
using System.Collections.Generic;

namespace EternalWar.Server.Application.Messages
{
    internal static class MessageHandlerRegistration
    {
        private static readonly Dictionary<string, IMessageHandler> _handlerMap = new();

        public static void Setup(IMessageSender messageSender, IHighLevelGameActions highLevelGameActions)
        {
            Register(NetworkMessageTypeNames.LOG, new MessageHandler_Log());
            Register(NetworkMessageTypeNames.LOG_WITH_RECEIPT, new MessageHandler_LogWithReceipt(messageSender));
            Register(NetworkMessageTypeNames.CLIENT_STATE_DIVERGENCE, new MessageHandler_ClientStateDivergence(highLevelGameActions));
            Register(NetworkMessageTypeNames.REQUEST_COMMAND, new MessageHandler_RequestCommand(highLevelGameActions));
        }

        public static void Cleanup()
        {
            _handlerMap.Clear();
        }

        public static IMessageHandler? GetHandler(string key)
        {
            if (!_handlerMap.TryGetValue(key, out IMessageHandler? handler))
            {
                EternalWar.Core.Helpers.Debug.PrintError($"Handler map does not contain key ({key})");
                return null;
            }
            return handler;
        }

        private static void Register(string key, IMessageHandler handler)
        {
            if (_handlerMap.ContainsKey(key))
            {
                EternalWar.Core.Helpers.Debug.Print($"Handler map already registered handler for key {key}");
                return;
            }
            _handlerMap.Add(key, handler);
        }
    }
}
