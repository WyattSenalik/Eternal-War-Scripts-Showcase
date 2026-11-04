// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;

namespace EternalWar.Server.Application.NetworkStateChange.AllowHandlers
{
    // TODO: Then continue if making the handlers get used so that we can just make the client always send a request change call.
    internal sealed class AllowChangeHandler_Run_Combat_ExecuteActionSequence : AllowChangeHandler<NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence>
    {
        protected override bool ShouldAllowChange(GameDatabase database, NetworkGameState networkGameState, Guid clientIdRequestingChange, NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence changeRequested)
        {
            // Need to be in combat.
            if (!networkGameState.TryGetCombatRuntime(out _))
                return false;

            if (!CombatUnitOwnershipHelpers.DoesClientOwnCurrentTurnUnit(clientIdRequestingChange, database, networkGameState, out CombatUnitUniqueIdentifier? currentTurnUnitIdentifier))
                return false;

            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();
            CombatInstance combatInstance = combatRuntime.Instance;
            UnitBuild unitBuild = combatInstance.GetUnitBuild(currentTurnUnitIdentifier);

            ActionExecutionContext actionExecutionContext = changeRequested.ActionExecutionContext;
            string actionSequenceId = actionExecutionContext.ActionSequenceId;

            // Not allowed to use an aciton if the unit doesn't have that action available to them.
            if (!unitBuild.HasActionSequenceId(database, actionSequenceId))
                return false;

            // Not allowed to use an action if the selections made for it weren't valid.
            return CombatSelectionHelpers.AreSelectionsValid(database, combatRuntime, currentTurnUnitIdentifier, actionExecutionContext);
        }
    }
}

