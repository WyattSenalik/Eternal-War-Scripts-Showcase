// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Map.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Core.Model
{
    public static class NetworkGameStateAccessors
    {
        public static GamePhase GetGamePhase(this NetworkGameState networkGameState)
        {
            GameState gameState = networkGameState.GameState;
            return gameState.Phase;
        }
        public static T GetGamePhase<T>(this NetworkGameState networkGameState) where T : GamePhase
        {
            GamePhase gamePhase = GetGamePhase(networkGameState);
            return (T)gamePhase;
        }

        public static LobbyState GetLobbyState(this NetworkGameState networkGameState)
        {
            GameState gameState = networkGameState.GameState;
            return GameStateAccessors.GetLobbyState(gameState);
        }

        public static CombatRuntime GetCombatRuntime(this NetworkGameState networkGameState)
        {
            var runRuntime = GetGamePhase<RunRuntime>(networkGameState);
            RunState runState = runRuntime.State;
            return RunStateAccessors.GetRunPhase<CombatRuntime>(runState);
        }

        public static CombatInstance GetCombatInstance(this NetworkGameState networkGameState)
        {
            CombatRuntime combatRuntime = GetCombatRuntime(networkGameState);
            return combatRuntime.Instance;
        }

        public static CombatState GetCombatState(this NetworkGameState networkGameState)
        {
            CombatRuntime combatRuntime = GetCombatRuntime(networkGameState);
            return combatRuntime.State;
        }

        public static RunInstance GetRunInstance(this NetworkGameState networkGameState)
        {
            var runRuntime = GetGamePhase<RunRuntime>(networkGameState);
            return runRuntime.Instance;
        }

        public static RunState GetRunState(this NetworkGameState networkGameState)
        {
            var runRuntime = GetGamePhase<RunRuntime>(networkGameState);
            return runRuntime.State;
        }

        public static RunPhase GetRunPhase(this NetworkGameState networkGameState)
        {
            RunState runState = networkGameState.GetRunState();
            return runState.Phase;
        }

        public static DeterministicRandomState GetRandomState(this NetworkGameState networkGameState)
        {
            RunState runState = networkGameState.GetRunState();
            return runState.RandomState;
        }

        public static LocationMapState GetLocationMapState(this NetworkGameState networkGameState)
        {
            RunState runState = GetRunState(networkGameState);
            return RunStateAccessors.GetLocationMapState(runState);
        }

        public static bool TryGetLobbyRuntime(this NetworkGameState networkGameState, [NotNullWhen(true)] out LobbyRuntime? lobbyRuntime)
        {
            GamePhase gamePhase = networkGameState.GetGamePhase();
            if (gamePhase is not LobbyRuntime castedPhase)
            {
                lobbyRuntime = null;
                return false;
            }
            lobbyRuntime = castedPhase;
            return true;
        }

        public static bool TryGetRunRuntime(this NetworkGameState networkGameState, [NotNullWhen(true)] out RunRuntime? runRuntime)
        {
            GamePhase gamePhase = networkGameState.GetGamePhase();
            if (gamePhase is not RunRuntime castedPhase)
            {
                runRuntime = null;
                return false;
            }
            runRuntime = castedPhase;
            return true;
        }

        public static bool TryGetRunState(this NetworkGameState networkGameState, [NotNullWhen(true)] out RunState? runState)
        {
            if (!networkGameState.TryGetRunRuntime(out RunRuntime? runRuntime))
            {
                runState = null;
                return false;
            }
            runState = runRuntime.State;
            return true;
        }

        public static bool TryGetRunPhase(this NetworkGameState networkGameState, [NotNullWhen(true)] out RunPhase? runPhase)
        {
            if (!networkGameState.TryGetRunState(out RunState? runState))
            {
                runPhase = null;
                return false;
            }
            runPhase = runState.Phase;
            return true;
        }

        public static bool TryGetRunPhase<T>(this NetworkGameState networkGameState, [NotNullWhen(true)] out T? runPhase) where T : RunPhase
        {
            if (!networkGameState.TryGetRunPhase(out RunPhase? uncastedPhase))
            {
                runPhase = null;
                return false;
            }
            if (uncastedPhase is not T castedPhase)
            {
                runPhase = null;
                return false;
            }
            runPhase = castedPhase;
            return true;
        }

        public static bool TryGetCombatRuntime(this NetworkGameState networkGameState, [NotNullWhen(true)] out CombatRuntime? combatRuntime) => networkGameState.TryGetRunPhase(out combatRuntime);

        public static bool TryGetCombatState(this NetworkGameState networkGameState, [NotNullWhen(true)] out CombatState? combatState)
        {
            if (!networkGameState.TryGetCombatRuntime(out CombatRuntime? combatRuntime))
            {
                combatState = null;
                return false;
            }
            combatState = combatRuntime.State;
            return true;
        }

        public static bool TryGetLocationMapRuntime(this NetworkGameState networkGameState, [NotNullWhen(true)] out LocationMapRuntime? locationMapRuntime) => networkGameState.TryGetRunPhase(out locationMapRuntime);
    }
}
