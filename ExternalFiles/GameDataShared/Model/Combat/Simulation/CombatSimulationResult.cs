// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Run.Runtime;

namespace EternalWar.Core.Model.Combat.Simulation
{
    // TODO: Add combat status result here to stream-line handling combat end
    public sealed record CombatSimulationResult(CombatState FinalState, DeterministicRandomState FinalRandomState, ValueArray<CombatCommand> CommandLog, ValueArray<CombatEvent> EventLog);
}
