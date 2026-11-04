// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Core.Model
{
    public static class GameStateTransforms
    {
        public static GameState TransformCombatState(GameState gameState, Func<CombatRuntime, CombatState> transformFunc)
        {
            RunState oldRunState = GameStateAccessors.GetRunState(gameState);
            RunState newRunState = RunStateTransforms.TransformCombatState(oldRunState, transformFunc);
            return ReplaceRunState(gameState, newRunState);
        }

        public static GameState ReplaceLobbyState(GameState gameState, LobbyState newLobbyState)
        {
            LobbyRuntime oldLobbyRuntime = GameStateAccessors.GetPhase<LobbyRuntime>(gameState);

            LobbyRuntime newLobbyRuntime = oldLobbyRuntime with
            {
                State = newLobbyState
            };
            return gameState with
            {
                Phase = newLobbyRuntime
            };
        }

        public static GameState ReplaceRunState(GameState gameState, RunState newRunState)
        {
            var oldRunRuntime = GameStateAccessors.GetPhase<RunRuntime>(gameState);

            RunRuntime newRunRuntime = oldRunRuntime with
            {
                State = newRunState
            };
            return gameState with
            {
                Phase = newRunRuntime
            };
        }
    }
}
