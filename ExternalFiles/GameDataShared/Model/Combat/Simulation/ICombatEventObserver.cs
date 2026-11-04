// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Simulation.Events;

namespace EternalWar.Core.Model.Combat.Simulation
{
    internal interface ICombatEventObserver
    {
        void HandleEventRecorded(CombatEvent combatEvent);
    }
}

