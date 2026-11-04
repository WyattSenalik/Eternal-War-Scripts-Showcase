// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Run.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation
{
    public static class CombatSimulationResultExtensions
    {
        public static CombatSimulationResult Append(this CombatSimulationResult firstResult, CombatSimulationResult secondResult)
        {
            ValueArray<CombatCommand> firstCommandLog = firstResult.CommandLog;
            ValueArray<CombatCommand> secondCommandLog = secondResult.CommandLog;
            ValueArray<CombatCommand> combinedCommandLog = firstCommandLog.Add(secondCommandLog);
            ValueArray<CombatEvent> firstEventLog = firstResult.EventLog;
            ValueArray<CombatEvent> secondEventLog = secondResult.EventLog;
            ValueArray<CombatEvent> combinedEventLog = firstEventLog.Add(secondEventLog);
            CombatState finalState = secondResult.FinalState;
            DeterministicRandomState finalRandomState = secondResult.FinalRandomState;

            return new CombatSimulationResult(finalState, finalRandomState, combinedCommandLog, combinedEventLog);
        }
    }
}
