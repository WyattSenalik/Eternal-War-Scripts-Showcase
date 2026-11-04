namespace EternalWar.Core.Network.Messages
{
    public static class NetworkMessageTypeNames
    {
        public const string LOG = "LOG";
        public const string LOG_WITH_RECEIPT = "LOG_WITH_RECEIPT";

        // Client handling only
        public const string SELF_CLIENT_CONNECTED = "SELF_CLIENT_CONNECTED";
        public const string NETWORK_GAME_COMMANDS_APPLIED = "NETWORK_GAME_COMMANDS_APPLIED";
        public const string NETWORK_GAME_STATE_SNAPSHOT = "NETWORK_GAME_STATE_SNAPSHOT";
        public const string CHANGE_REQUEST_DENIED = "CHANGE_REQUEST_DENIED";

        // Server handling only
        public const string CLIENT_STATE_DIVERGENCE = "CLIENT_STATE_DIVERGENCE";
        public const string REQUEST_COMMAND = "REQUEST_COMMAND";

        // Used both ways (usually means that one client is requesting this and then the server is just relaying it if the request is accepted [if not cheating, the request is usually accepted])
    }
}
