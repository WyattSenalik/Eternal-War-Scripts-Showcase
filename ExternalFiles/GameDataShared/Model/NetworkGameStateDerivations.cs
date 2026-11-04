// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;

namespace EternalWar.Core.Model
{
    public static class NetworkGameStateDerivations
    {
        public static TurnState DeriveTurnState(this NetworkGameState networkGameState, GameDatabase database)
        {
            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            return CombatTurnHelpers.DeriveTurnState(database, combatRuntime);
        }

        public static CombatUnitUniqueIdentifier DeriveCurrentTurnUnitIdentifier(this NetworkGameState networkGameState, GameDatabase database)
        {
            TurnState turnState = networkGameState.DeriveTurnState(database);
            return turnState.CurrentTurnUnitIdentifier ?? throw new NullReferenceException($"CurrentTurnUnitIdentifier is null");
        }
    }
}
