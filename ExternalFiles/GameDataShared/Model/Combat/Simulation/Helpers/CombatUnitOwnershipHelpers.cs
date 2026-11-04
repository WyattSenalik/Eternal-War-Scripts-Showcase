// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Network;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatUnitOwnershipHelpers
    {
        public static bool DoesClientOwnCurrentTurnUnit(Guid clientGuid, GameDatabase database, NetworkGameState networkGameState) => DoesClientOwnCurrentTurnUnit(clientGuid, database, networkGameState, out _);
        public static bool DoesClientOwnCurrentTurnUnit(Guid clientGuid, GameDatabase database, NetworkGameState networkGameState, [NotNullWhen(true)] out CombatUnitUniqueIdentifier? currentTurnUnitIdentifier)
        {
            CombatRuntime combatRuntime = networkGameState.GetCombatRuntime();

            TurnState turnState = CombatTurnHelpers.DeriveTurnState(database, combatRuntime);
            currentTurnUnitIdentifier = turnState.CurrentTurnUnitIdentifier;

            return currentTurnUnitIdentifier != null && DoesClientOwnUnit(clientGuid, currentTurnUnitIdentifier);
        }

        public static bool DoesClientOwnUnit(ClientInfo clientInfo, CombatUnitUniqueIdentifier unitIdentifier)
        {
            CombatUnitOwner unitOwner = unitIdentifier.Owner;
            if (unitOwner is not CombatUnitOwner_Player playerOwner)
                return false;

            ClientInfo currentTurnClient = playerOwner.PlayerClient;
            return currentTurnClient == clientInfo;
        }

        public static bool DoesClientOwnUnit(Guid clientGuid, CombatUnitUniqueIdentifier unitIdentifier)
        {
            CombatUnitOwner unitOwner = unitIdentifier.Owner;
            if (unitOwner is not CombatUnitOwner_Player playerOwner)
                return false;

            ClientInfo currentTurnClient = playerOwner.PlayerClient;
            return currentTurnClient.Id == clientGuid;
        }

        public static bool IsClientAIOwned(CombatUnitUniqueIdentifier unitIdentifier)
        {
            CombatUnitOwner owner = unitIdentifier.Owner;
            return owner is CombatUnitOwner_AI;
        }
    }
}
