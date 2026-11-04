// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;

namespace EternalWar.Server.Application.NetworkStateChange.AllowHandlers
{
    internal sealed class AllowChangeHandler_Run_Combat_EndTurn : AllowChangeHandler<NetworkGameCommandRequest_Run_Combat_EndTurn>
    {
        protected override bool ShouldAllowChange(GameDatabase database, NetworkGameState networkGameState, Guid clientIdRequestingChange, NetworkGameCommandRequest_Run_Combat_EndTurn changeRequested)
        {
            // Need to be in combat.
            if (!networkGameState.TryGetCombatRuntime(out _))
                return false;

            return CombatUnitOwnershipHelpers.DoesClientOwnCurrentTurnUnit(clientIdRequestingChange, database, networkGameState);
        }
    }
}

