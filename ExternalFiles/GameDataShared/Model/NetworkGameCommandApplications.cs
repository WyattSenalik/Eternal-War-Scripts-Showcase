// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Lobby.Simulation;
using EternalWar.Core.Model.Run.Runtime;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EternalWar.Core.Model
{
    public sealed record NetworkGameCommandApplyResult(NetworkGameState State, ValueArray<NetworkGameEvent> Events);
    public sealed record NetworkGameCommandBatchApplyResult(NetworkGameState State, ValueArray<NetworkGameEvent> Events);

    public static class NetworkGameCommandApplications
    {
        public static NetworkGameCommandBatchApplyResult ApplyCommands(this NetworkGameState networkGameState, GameDatabase database, IEnumerable<NetworkGameCommand> commands)
        {
            NetworkGameState state = networkGameState;
            using ValueArray<NetworkGameEvent>.Builder events = new();
            foreach (NetworkGameCommand command in commands)
            {
                NetworkGameCommandApplyResult result = state.ApplyCommand(database, command);
                state = result.State;
                foreach (NetworkGameEvent gameEvent in result.Events)
                {
                    events.Add(gameEvent);
                }
            }
            return new NetworkGameCommandBatchApplyResult(state, events.ToValueArray());
        }

        public static NetworkGameCommandApplyResult ApplyCommand(this NetworkGameState networkGameState, GameDatabase database, NetworkGameCommand command)
        {
            return command switch
            {
                NetworkGameCommand_ConnectedClientsChanged connectedClientsCommand => new(
                    networkGameState with { ConnectedClients = connectedClientsCommand.NewConnectedClients },
                    One(new NetworkGameEvent_ConnectedClientsChanged(connectedClientsCommand.NewConnectedClients))),

                NetworkGameCommand_ClientDisconnected clientDisconnectedCommand => ApplyClientDisconnected(networkGameState, clientDisconnectedCommand),

                NetworkGameCommand_Lobby_ReadyToggled readyToggledCommand => new(
                    networkGameState.TransformLobbyState(oldLobbyState => oldLobbyState.ToggleClientReady(readyToggledCommand.ToggleClientInfo)),
                    One(new NetworkGameEvent_Lobby_ReadyToggled(readyToggledCommand.ToggleClientInfo))),

                NetworkGameCommand_Lobby_RunStarted runStartedCommand => new(
                    networkGameState.ReplaceGamePhase(RunInitialization.InitializeRun(runStartedCommand.PlayerChosenUnitIdentifiers, runStartedCommand.RandomnessSeed)),
                    One(new NetworkGameEvent_Lobby_RunStarted(runStartedCommand.PlayerChosenUnitIdentifiers))),

                NetworkGameCommand_Run_CombatStarted combatStartedCommand => ApplyStartCombat(networkGameState, database, combatStartedCommand),

                CombatCommand combatCommand => ApplyCombatCommand(networkGameState, combatCommand),

                _ => throw new NotImplementedException($"Unhandled {nameof(NetworkGameCommand)}: ({command?.GetType()})")
            };
        }

        private static NetworkGameCommandApplyResult ApplyStartCombat(NetworkGameState oldNetworkGameState, GameDatabase database, NetworkGameCommand_Run_CombatStarted command)
        {
            NetworkGameState newState = oldNetworkGameState.TransformGamePhase<RunRuntime>(oldRunRuntime =>
            {
                CombatRuntime combatRuntime = CombatInitialization.InitializeCombat(database, oldRunRuntime, command.CombatId, Vector3.Zero, command.PlayerUnitPlacements, out DeterministicRandomState randomStateResult);
                RunState oldRunState = oldRunRuntime.State;
                RunState newRunState = oldRunState with
                {
                    Phase = combatRuntime,
                    RandomState = randomStateResult,
                };
                return oldRunRuntime with
                {
                    State = newRunState
                };
            });
            NetworkGameEvent_Run_CombatStarted combatStartedEvent = new(command.CombatId, command.PlayerUnitPlacements);
            return new NetworkGameCommandApplyResult(newState, One(combatStartedEvent));
        }

        private static NetworkGameCommandApplyResult ApplyClientDisconnected(NetworkGameState oldNetworkGameState, NetworkGameCommand_ClientDisconnected command)
        {
            NetworkGameState newState = oldNetworkGameState.DisconnectClient(command.DisconnectedClient.Id);
            NetworkGameEvent_ClientDisconnected disconnectedEvent = new(command.DisconnectedClient);
            return new NetworkGameCommandApplyResult(newState, One(disconnectedEvent));
        }

        private static NetworkGameCommandApplyResult ApplyCombatCommand(NetworkGameState networkGameState, CombatCommand command)
        {
            ValueArray<CombatEvent> combatEvents = command.CreateEvents();
            NetworkGameState newState = networkGameState.TransformCombatState(oldCombatRuntime => oldCombatRuntime.State.ApplyCommand(command));
            ValueArray<NetworkGameEvent> events = new(combatEvents);
            return new NetworkGameCommandApplyResult(newState, events);
        }

        private static ValueArray<NetworkGameEvent> One(NetworkGameEvent gameEvent) => new(new[] { gameEvent });
    }
}
