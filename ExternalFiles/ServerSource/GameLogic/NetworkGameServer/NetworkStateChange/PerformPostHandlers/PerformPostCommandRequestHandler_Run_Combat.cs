// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Handlers.UnitDecisionController;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using EternalWar.Core.Model.Run.Runtime;
using System;
using System.Linq;

namespace EternalWar.Server.Application.NetworkStateChange.PerformPostHandlers
{
    internal sealed class PerformPostCommandRequestHandler_Run_Combat(ServerGameSession session, GameDatabase database) : PerformPostCommandRequestHandler<NetworkGameCommandRequest_Run_Combat>
    {
        private const int STALEMATE_TURN_COUNT = 10;

        private readonly ServerGameSession _session = session;
        private readonly GameDatabase _database = database;

        protected override void PerformPostRequest(NetworkGameCommandRequest_Run_Combat request)
        {
            // Advancing AI turns is a consequence of a player's explicit end-turn request.
            if (request is not NetworkGameCommandRequest_Run_Combat_EndTurn)
                return;

            NetworkGameState networkGameState = _session.NetworkGameState;
            if (IsCombatOver(networkGameState))
                return;

            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatInstance combatInstance = combatRuntime.Instance;
            TurnState turnState = CombatTurnHelpers.DeriveTurnState(_database, combatRuntime);
            CombatUnitUniqueIdentifier currentTurnUnitIdentifier = turnState.CurrentTurnUnitIdentifier
                ?? throw new NotImplementedException("Have not implemented what should happen when no unit can take a turn");

            using PooledDictionary<CombatUnitUniqueIdentifier, int> aiTurnsTakenToDoNothingPerUnit = new();
            while (CombatUnitOwnershipHelpers.IsClientAIOwned(currentTurnUnitIdentifier))
            {
                networkGameState = _session.NetworkGameState;
                CombatState combatState = networkGameState.GetCombatState();
                DeterministicRandomState randomState = networkGameState.GetRandomState();
                AITurnSimulationResult? aiTurnSimulationResult = CombatAISimulationHelpers.SimulateAITurn(_database, randomState, combatInstance, combatState, currentTurnUnitIdentifier);
                if (aiTurnSimulationResult != null)
                {
                    PrintAiEventLog(aiTurnSimulationResult.CombatSimulationResult.EventLog);

                    foreach (ActionExecutionContext actionExecutionContext in aiTurnSimulationResult.ExecutionContexts)
                    {
                        NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence actionRequest = new(actionExecutionContext);
                        ValueArray<NetworkGameCommand> commands = actionRequest.ResolveToCommands(_database, networkGameState);
                        _session.ApplyCommandsAndRelay(commands, clientRequestedChangeId: null);
                        networkGameState = _session.NetworkGameState;

                        if (IsCombatOver(networkGameState))
                            return;
                    }
                    aiTurnsTakenToDoNothingPerUnit[currentTurnUnitIdentifier] = 0;
                }
                else
                {
                    Debug.Print($"Failed to simulate turn for ({currentTurnUnitIdentifier}). Skipped");
                    if (aiTurnsTakenToDoNothingPerUnit.TryGetValue(currentTurnUnitIdentifier, out int turnsTakenDoingNothing))
                    {
                        aiTurnsTakenToDoNothingPerUnit[currentTurnUnitIdentifier] = turnsTakenDoingNothing + 1;
                        bool didAnyoneDoSomething = aiTurnsTakenToDoNothingPerUnit.Values.Any(turns => turns == 0);
                        if (!didAnyoneDoSomething && turnsTakenDoingNothing > STALEMATE_TURN_COUNT)
                        {
                            Debug.Print("Detected potential infinite loop in AI logic. They are all doing nothing and player turns are being skipped.");
                            break;
                        }
                    }
                    else
                    {
                        aiTurnsTakenToDoNothingPerUnit[currentTurnUnitIdentifier] = 1;
                    }
                }

                var endTurnCommandRequest = NetworkGameCommandRequest_Run_Combat_EndTurn.Instance;
                networkGameState = _session.NetworkGameState;
                ValueArray<NetworkGameCommand> endTurnCommands = endTurnCommandRequest.ResolveToCommands(_database, networkGameState);
                _session.ApplyCommandsAndRelay(endTurnCommands, clientRequestedChangeId: null);
                networkGameState = _session.NetworkGameState;
                if (IsCombatOver(networkGameState))
                    return;

                combatRuntime = networkGameState.GetCombatRuntime();
                turnState = CombatTurnHelpers.DeriveTurnState(_database, combatRuntime);
                currentTurnUnitIdentifier = turnState.CurrentTurnUnitIdentifier
                    ?? throw new NotImplementedException("Have not implemented what should happen when no unit can take a turn");
            }
        }

        private static bool IsCombatOver(NetworkGameState networkGameState)
        {
            CombatState combatState = networkGameState.GetCombatState();
            CombatStatus status = combatState.DeriveStatus();
            return status is CombatStatus_Victory or CombatStatus_Defeat;
        }

        private static void PrintAiEventLog(ValueArray<CombatEvent> eventLog)
        {
            int indentationCount = 0;
            foreach (CombatEvent combatEvent in eventLog)
            {
                if (combatEvent is CombatEvent_ActionEnded or CombatEvent_StepEnded)
                    --indentationCount;

                string indentation = new('\t', Math.Max(0, indentationCount));
                Debug.Print($"{indentation}AI-Turn: CombatEvent-{combatEvent}");

                if (combatEvent is CombatEvent_ActionStarted or CombatEvent_StepStarted)
                    ++indentationCount;
            }
        }
    }
}
