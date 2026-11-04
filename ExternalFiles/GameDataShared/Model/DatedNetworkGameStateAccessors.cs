// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Core.Model
{
    public static class DatedNetworkGameStateAccessors
    {
        public static GamePhase GetGamePhase(this DatedNetworkGameState datedNetworkGameState) => datedNetworkGameState.NetworkGameState.GetGamePhase();
        public static T GetGamePhase<T>(this DatedNetworkGameState datedNetworkGameState) where T : GamePhase => datedNetworkGameState.NetworkGameState.GetGamePhase<T>();

        public static LobbyState GetLobbyState(this DatedNetworkGameState datedNetworkGameState) => datedNetworkGameState.NetworkGameState.GetLobbyState();

        public static CombatRuntime GetCombatRuntime(this DatedNetworkGameState datedNetworkGameState) => datedNetworkGameState.NetworkGameState.GetCombatRuntime();

        public static CombatInstance GetCombatInstance(this DatedNetworkGameState datedNetworkGameState) => datedNetworkGameState.NetworkGameState.GetCombatInstance();

        public static CombatState GetCombatState(this DatedNetworkGameState datedNetworkGameState) => datedNetworkGameState.NetworkGameState.GetCombatState();

        public static RunPhase GetRunPhase(this DatedNetworkGameState datedNetworkGameState) => datedNetworkGameState.NetworkGameState.GetRunPhase();

        public static RunInstance GetRunInstance(this DatedNetworkGameState datedNetworkGameState) => datedNetworkGameState.NetworkGameState.GetRunInstance();

        public static bool TryGetLobbyRuntime(this DatedNetworkGameState datedNetworkGameState, [NotNullWhen(true)] out LobbyRuntime? lobbyRuntime) => datedNetworkGameState.NetworkGameState.TryGetLobbyRuntime(out lobbyRuntime);

        public static bool TryGetCombatRuntime(this DatedNetworkGameState datedNetworkGameState, [NotNullWhen(true)] out CombatRuntime? combatRuntime) => datedNetworkGameState.NetworkGameState.TryGetCombatRuntime(out combatRuntime);
    }
}
