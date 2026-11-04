// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Network;
using System.Linq;

namespace EternalWar.Core.Model.Lobby.Runtime
{
    public sealed record LobbyRuntime(LobbyInstance Instance, LobbyState State) : GamePhase
    {
        public static readonly LobbyRuntime InitialState = new(new LobbyInstance(), LobbyState.InitialState);
    }
    public sealed record LobbyInstance(); // Added for architectual consistency
    public sealed record LobbyState(SortedValueArray<int, ClientInfo> ReadyClients)
    {
        public static readonly LobbyState InitialState = new(SortedValueArray<int, ClientInfo>.Empty);
    }

    public static class LobbyStateExtensions
    {
        public static int GetReadyClientCount(this LobbyState lobbyState)
        {
            return lobbyState.ReadyClients.Count;
        }

        public static bool IsClientReady(this LobbyState lobbyState, ClientInfo clientInfo)
        {
            return lobbyState.ReadyClients.Contains(clientInfo);
        }
    }
}