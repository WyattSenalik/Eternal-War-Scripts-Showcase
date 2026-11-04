// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using EternalWar.Core.Model.Run.Runtime;
using System;
using System.Linq;

namespace EternalWar.Core.Model
{
    public static class NetworkGameCommandRequestResolution
    {
        public static ValueArray<NetworkGameCommand> ResolveToCommands(this NetworkGameCommandRequest request, GameDatabase database, NetworkGameState networkGameState)
        {
            return request switch
            {
                NetworkGameCommandRequest_Lobby_ToggleReady toggleReadyRequest => One(new NetworkGameCommand_Lobby_ReadyToggled(toggleReadyRequest.ToggleClientInfo)),
                NetworkGameCommandRequest_Run_Combat_EndTurn => ResolveEndTurn(database, networkGameState),
                NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence executeActionSequenceRequest => ResolveActionSequence(executeActionSequenceRequest, database, networkGameState),
                NetworkGameCommandRequest_Run_Map_VoteForNode voteForNodeRequest => One(new NetworkGameCommand_Run_Map_VoteForNode(voteForNodeRequest.VotingClientInfo, voteForNodeRequest.NodeIndex)),
                _ => throw new NotImplementedException($"Unhandled {nameof(NetworkGameCommandRequest)}: ({request?.GetType()})")
            };
        }

        private static ValueArray<NetworkGameCommand> ResolveActionSequence(NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence request, GameDatabase database, NetworkGameState networkGameState)
        {
            DeterministicRandomState randomState = networkGameState.GetRandomState();
            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatSimulationResult result = CombatSimulator.Simulate(database, randomState, combatRuntime, request.ActionExecutionContext);
            return result.CommandLog.Select(command => (NetworkGameCommand)command).ToValueArray();
        }

        private static ValueArray<NetworkGameCommand> ResolveEndTurn(GameDatabase database, NetworkGameState networkGameState)
        {
            DeterministicRandomState randomState = networkGameState.GetRandomState();
            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatSimulationResult result = CombatSimulationHelpers.SimulateEndCurrentUnitsTurn(database, randomState, combatRuntime);
            return result.CommandLog.Select(command => (NetworkGameCommand)command).ToValueArray();
        }

        private static ValueArray<NetworkGameCommand> One(NetworkGameCommand command) => new(new[] { command });
    }
}
