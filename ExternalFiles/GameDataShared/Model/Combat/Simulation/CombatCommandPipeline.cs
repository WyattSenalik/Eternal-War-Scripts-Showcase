// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Random;
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation
{
    /// <summary>Applies one atomic combat command, records its observer event, and resolves reactions before returning.</summary>
    public sealed class CombatCommandPipeline : IDisposable
    {
        public CombatCommandProcessor Processor { get; private init; }
        public DeterministicRandomState CurrentRandomState => Processor.CurrentRandomState;
        public CombatState CurrentState => Processor.CurrentState;

        internal IRandomResolutionResolver RandomResolver => Processor.RandomResolver;
        internal GameDatabase Database => Processor.Database;
        internal CombatInstance CombatInstance => Processor.CombatInstance;

        private readonly PassiveResolver _passiveResolver;

        public CombatCommandPipeline(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState initialCombatState, SimulationOptions? simulationOptionsOverride = null)
        {
            Processor = new CombatCommandProcessor(database, randomState, combatInstance, initialCombatState, simulationOptionsOverride);

            // Create observers
            _passiveResolver = new PassiveResolver(Processor);

            // Register observers
            Processor.RegisterObserver(_passiveResolver);
        }

        public ValueArray<CombatEvent> Apply(CombatCommand combatCommand) => Processor.Apply(combatCommand);
        public ValueArray<CombatCommand> BuildCommandLog() => Processor.BuildCommandLog();
        public ValueArray<CombatEvent> BuildEventLog() => Processor.BuildEventLog();

        public void Dispose()
        {
            Processor.Dispose();
            _passiveResolver.Dispose();
        }
    }
}

