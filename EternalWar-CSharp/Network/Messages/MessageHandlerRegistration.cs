// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelGame.Actions;
using EternalWar.Client.Application.HighLevelNetwork.Actions;
using EternalWar.Client.Application.Network.Messages.Debug;
using EternalWar.Client.Application.Network.Messages.HighLevelGame;
using EternalWar.Client.Application.Network.Messages.HighLevelNetwork;
using EternalWar.Core.Network.Messages;
using System;
using System.Collections.Generic;

namespace EternalWar.Client.Application.Network.Messages
{
    internal static class MessageHandlerRegistration
    {
        private static readonly Dictionary<string, IMessageHandler> _handlerMap = new();

        public static void Setup(GameActions gameActions)
        {
            IHighLevelNetworkActions highLevelNetworkActions = gameActions.HighLevelNetworkActions;
            IHighLevelGameActions highLevelGameActions = gameActions.HighLevelGameActions;

            // Debug
            Register(NetworkMessageTypeNames.LOG, new MessageHandler_Log());
            Register(NetworkMessageTypeNames.LOG_WITH_RECEIPT, new MessageHandler_LogWithReceipt());

            // High-Level Networking
            Register(NetworkMessageTypeNames.SELF_CLIENT_CONNECTED, new MessageHandler_SelfClientConnected(highLevelNetworkActions));

            // High-Level Game
            Register(NetworkMessageTypeNames.NETWORK_GAME_COMMANDS_APPLIED, new MessageHandler_NetworkGameCommandsApplied(highLevelGameActions));
            Register(NetworkMessageTypeNames.NETWORK_GAME_STATE_SNAPSHOT, new MessageHandler_NetworkGameStateSnapshot(highLevelGameActions));
        }

        public static void Cleanup()
        {
            _handlerMap.Clear();
        }


        public static IMessageHandler GetHandler(string key)
        {
            return _handlerMap.TryGetValue(key, out IMessageHandler? handler) ? handler : throw new Exception($"Failed to find handler for ({key})");
        }

        private static void Register(string key, IMessageHandler handler)
        {
            _handlerMap.Add(key, handler);
        }
    }
}
