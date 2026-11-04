// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatSimulationHelpers
    {
        public static CombatState EndCurrentUnitsTurn(GameDatabase database, DeterministicRandomState randomState, CombatRuntime combatRuntime, SimulationOptions? simulationOptionsOverride = null) => combatRuntime.State.EndCurrentUnitsTurn(database, randomState, combatRuntime.Instance, simulationOptionsOverride);
        public static CombatState EndCurrentUnitsTurn(this CombatState combatState, GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, SimulationOptions? simulationOptionsOverride = null) => SimulateEndCurrentUnitsTurn(database, randomState, combatInstance, combatState, simulationOptionsOverride).FinalState;

        public static CombatSimulationResult SimulateEndCurrentUnitsTurn(GameDatabase database, DeterministicRandomState randomState, CombatRuntime combatRuntime, SimulationOptions? simulationOptionsOverride = null) => SimulateEndCurrentUnitsTurn(database, randomState, combatRuntime.Instance, combatRuntime.State, simulationOptionsOverride);
        public static CombatSimulationResult SimulateEndCurrentUnitsTurn(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, SimulationOptions? simulationOptionsOverride = null)
        {
            TurnState oldTurnState = CombatTurnHelpers.DeriveTurnState(database, combatInstance, combatState);
            CombatUnitUniqueIdentifier currentTurnUnitsIdentifier = oldTurnState.CurrentTurnUnitIdentifier ?? throw new NullReferenceException($"Tried to end turn when there was no current unit ({oldTurnState.TurnOrder})");

            // TODO: Probably want to have command pipeline as a parameter
            using CombatCommandPipeline commandPipeline = new(database, randomState, combatInstance, combatState, simulationOptionsOverride);
            CombatCommand_EndUnitTurn turnEndedCommand = new(currentTurnUnitsIdentifier);
            commandPipeline.Apply(turnEndedCommand);
            AdvanceToNextTurn(commandPipeline);
            return new CombatSimulationResult(commandPipeline.CurrentState, commandPipeline.CurrentRandomState, commandPipeline.BuildCommandLog(), commandPipeline.BuildEventLog());
        }

        public static void StartTurn(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier unitIdentifier, out CombatState combatStateResult, out DeterministicRandomState randomStateResult, SimulationOptions? simulationOptionsOverride = null)
        {
            using CombatCommandPipeline commandPipeline = new(database, randomState, combatInstance, combatState, simulationOptionsOverride);
            CombatCommand_StartUnitTurn turnStartedCommand = new(unitIdentifier);
            commandPipeline.Apply(turnStartedCommand);
            combatStateResult = commandPipeline.CurrentState;
            randomStateResult = commandPipeline.CurrentRandomState;
        }

        private static void AdvanceToNextTurn(CombatCommandPipeline commandPipeline)
        {
            GameDatabase database = commandPipeline.Database;
            CombatInstance combatInstance = commandPipeline.CombatInstance;
            CombatState? previousRoundResetState = null;

            while (true)
            {
                CombatState combatState = commandPipeline.CurrentState;
                TurnState turnState = CombatTurnHelpers.DeriveTurnState(database, combatInstance, combatState);
                ValueArray<UnitTurnState> turnOrder = turnState.TurnOrder;

                CombatUnitUniqueIdentifier? nextUnitIdentifier = null;
                foreach (UnitTurnState unitTurnState in turnOrder)
                {
                    CombatUnitUniqueIdentifier unitIdentifier = unitTurnState.Identifier;
                    UnitState unit = combatState.GetUnit(unitIdentifier);
                    if (unit.HasTakenTurn)
                        continue;

                    nextUnitIdentifier = unitIdentifier;
                    break;
                }

                if (nextUnitIdentifier != null)
                {
                    CombatCommand_StartUnitTurn unitTurnStartedCommand = new(nextUnitIdentifier);
                    commandPipeline.Apply(unitTurnStartedCommand);
                    combatState = commandPipeline.CurrentState;
                    UnitState nextUnit = combatState.GetUnit(nextUnitIdentifier);
                    if (!nextUnit.IsDead() && !nextUnit.HasSkipTurnNonStackableEffect(database))
                        return;

                    CombatCommand_EndUnitTurn unitTurnEndedCommand = new(nextUnitIdentifier);
                    commandPipeline.Apply(unitTurnEndedCommand);
                    continue;
                }

                // Every unit in this round has finished or skipped its turn. Reset the round before advancing again.
                foreach (UnitTurnState unitTurnState in turnOrder)
                {
                    CombatUnitUniqueIdentifier unitIdentifier = unitTurnState.Identifier;
                    combatState = commandPipeline.CurrentState;
                    CombatStats unitStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, unitIdentifier);

                    CombatCommand_RefreshUnitTurn unitTurnRefreshedCommand = new(unitIdentifier, unitStats.MaxActionPoints);
                    commandPipeline.Apply(unitTurnRefreshedCommand);
                }

                combatState = commandPipeline.CurrentState;
                if (previousRoundResetState != null && previousRoundResetState.Equals(combatState))
                    throw new InvalidOperationException("No unit can take a turn, and turn-boundary expirations did not change combat state.");
                previousRoundResetState = combatState;
            }
        }
    }
}


