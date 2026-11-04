// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Events
{
    public sealed class CombatEventRecorder : IDisposable
    {
        private List<CombatEvent> Events => _events ?? throw new ObjectDisposedException(nameof(CombatEventRecorder));

        private List<CombatEvent>? _events = ListPool.Get<CombatEvent>();

        public void Record(CombatEvent combatEvent)
        {
            Events.Add(combatEvent);
        }

        public ValueArray<CombatEvent> BuildLog()
        {
            return Events.ToValueArray();
        }

        void IDisposable.Dispose()
        {
            if (_events == null)
                return;
            ListPool.Release(_events);
            _events = null;
        }
    }
}
