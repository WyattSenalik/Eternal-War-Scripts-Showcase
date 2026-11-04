// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using System.Numerics;

namespace EternalWar.Client.Application.Model
{
    public static class ClientGameStateDerivations
    {
        public static TurnState DeriveTurnState(this ClientGameState clientGameState, GameDatabase database)
        {
            NetworkGameState networkGameState = clientGameState.NetworkGameState;
            return networkGameState.DeriveTurnState(database);
        }

        public static CombatUnitUniqueIdentifier DeriveCurrentTurnUnitIdentifier(this ClientGameState clientGameState, GameDatabase database)
        {
            NetworkGameState networkGameState = clientGameState.NetworkGameState;
            return networkGameState.DeriveCurrentTurnUnitIdentifier(database);
        }

        public static Vector3 GetWorldPosition(this ClientGameState clientGameState, GameDatabase database, GridPosition gridPosition)
        {
            CombatInstance combatInstance = clientGameState.GetCombatInstance();
            TileGridInstance tileGridInstance = combatInstance.TileGrid;
            string combatId = combatInstance.DefinitionId;

            return tileGridInstance.GetWorldPosition(database, combatId, gridPosition);
        }
    }
}
