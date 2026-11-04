// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Combat.Actions;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using EternalWar.Core.Network;

namespace EternalWar.Client.Application.Run.Combat.Controllers
{
    internal sealed class CombatController(ClientGameSession clientSession) : ICombatActions
    {
        private readonly ClientGameSession _clientSession = clientSession;

        public void EndTurn()
        {
            GameDatabase database = _clientSession.Database;
            ClientGameState clientGameState = _clientSession.GetFinalPresentationClientGameState();

            if (!IsItMyTurn(database, clientGameState))
                return;

            NetworkGameCommandRequest_Run_Combat_EndTurn endTurnRequest = NetworkGameCommandRequest_Run_Combat_EndTurn.Instance;
            _clientSession.ApplyLocalCommandRequestAndSendToServer(endTurnRequest);
        }

        private static bool IsItMyTurn(GameDatabase database, ClientGameState clientGameState)
        {
            ClientInfo myClientInfo = clientGameState.GetMyClientInfo();
            CombatRuntime combatRuntime = clientGameState.GetCombatRuntime();
            return CombatTurnHelpers.IsItClientsTurn(database, combatRuntime, myClientInfo);
        }
    }
}

