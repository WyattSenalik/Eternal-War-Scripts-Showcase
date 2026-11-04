// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Map.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Core.Model
{
    public static class NetworkGameStateTransforms
    {
        public static NetworkGameState TransformGamePhase<T>(this NetworkGameState networkGameState, Func<T, T> transformFunc) where T : GamePhase
        {
            T oldGamePhase = NetworkGameStateAccessors.GetGamePhase<T>(networkGameState);
            T newGamePhase = transformFunc.Invoke(oldGamePhase);
            return ReplaceGamePhase(networkGameState, newGamePhase);
        }

        public static NetworkGameState TransformLobbyState(this NetworkGameState networkGameState, Func<LobbyState, LobbyState> transformFunc)
        {
            LobbyState oldLobbyState = NetworkGameStateAccessors.GetLobbyState(networkGameState);
            LobbyState newLobbyState = transformFunc.Invoke(oldLobbyState);
            return ReplaceLobbyState(networkGameState, newLobbyState);
        }

        public static NetworkGameState TransformCombatState(this NetworkGameState networkGameState, Func<CombatRuntime, CombatState> transformFunc)
        {
            GameState oldGameState = networkGameState.GameState;
            GameState newGameState = GameStateTransforms.TransformCombatState(oldGameState, transformFunc);
            return networkGameState with
            {
                GameState = newGameState
            };
        }

        public static NetworkGameState ReplaceGamePhase(this NetworkGameState networkGameState, GamePhase gamePhase)
        {
            GameState oldGameState = networkGameState.GameState;

            GameState newGameState = oldGameState with
            {
                Phase = gamePhase
            };
            return networkGameState with
            {
                GameState = newGameState
            };
        }

        public static NetworkGameState ReplaceRunState(this NetworkGameState networkGameState, RunState newRunState)
        {
            GameState oldGameState = networkGameState.GameState;
            return networkGameState with
            {
                GameState = GameStateTransforms.ReplaceRunState(oldGameState, newRunState)
            };
        }

        public static NetworkGameState TransformRunState(this NetworkGameState networkGameState, Func<RunState, RunState> transformFunc)
        {
            RunState oldRunState = NetworkGameStateAccessors.GetRunState(networkGameState);
            RunState newRunState = transformFunc.Invoke(oldRunState);
            return ReplaceRunState(networkGameState, newRunState);
        }

        public static NetworkGameState TransformMapState(this NetworkGameState networkGameState, Func<LocationMapState, LocationMapState> transformFunc)
        {
            return networkGameState.TransformRunState(oldRunState => oldRunState.TransformMapState(transformFunc));
        }

        private static NetworkGameState ReplaceLobbyState(NetworkGameState networkGameState, LobbyState newLobbyState)
        {
            GameState oldGameState = networkGameState.GameState;

            return networkGameState with
            {
                GameState = GameStateTransforms.ReplaceLobbyState(oldGameState, newLobbyState)
            };
        }
    }
}
