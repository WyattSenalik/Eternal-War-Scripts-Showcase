// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Random;
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation
{
    public sealed class CombatCommandProcessor : IDisposable
    {
        public DeterministicRandomState CurrentRandomState => _realRandomResolver.ToState();
        public CombatState CurrentState { get; private set; }

        internal IRandomResolutionResolver RandomResolver => _simulationOptionsOverride == null ? _realRandomResolver : _simulationOptionsOverride.RandomHandler;
        internal GameDatabase Database { get; }
        internal CombatInstance CombatInstance { get; }

        private readonly RandomResolutionResolver_Deterministic _realRandomResolver;
        private readonly SimulationOptions? _simulationOptionsOverride;
        private readonly CombatEventRecorder _eventRecorder = new();
        private readonly PooledList<CombatCommand> _commands = new();
        private readonly PooledList<ICombatEventObserver> _observers = new();

        internal CombatCommandProcessor(GameDatabase database, DeterministicRandomState randomState, CombatInstance combatInstance, CombatState initialCombatState, SimulationOptions? simulationOptionsOverride)
        {
            CurrentState = initialCombatState;
            Database = database;
            CombatInstance = combatInstance;
            _realRandomResolver = randomState.CreateResolver();
            _simulationOptionsOverride = simulationOptionsOverride;
        }

        public ValueArray<CombatEvent> Apply(CombatCommand combatCommand)
        {
            CurrentState = CurrentState.ApplyCommand(combatCommand);
            _commands.Add(combatCommand);
            ValueArray<CombatEvent> combatEvents = combatCommand.CreateEvents();
            foreach (CombatEvent combatEvent in combatEvents)
            {
                _eventRecorder.Record(combatEvent);
                foreach (ICombatEventObserver observer in _observers)
                {
                    observer.HandleEventRecorded(combatEvent);
                }
            }
            return combatEvents;
        }

        public ValueArray<CombatCommand> BuildCommandLog() => _commands.ToValueArray();
        public ValueArray<CombatEvent> BuildEventLog() => _eventRecorder.BuildLog();

        public void Dispose()
        {
            ((IDisposable)_eventRecorder).Dispose();
            _commands.Dispose();
            _observers.Dispose();
        }

        internal void RegisterObserver(ICombatEventObserver observer)
        {
            _observers.Add(observer);
        }
    }
}

