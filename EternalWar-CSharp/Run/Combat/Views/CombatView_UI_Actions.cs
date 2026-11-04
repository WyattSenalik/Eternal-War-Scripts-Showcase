// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    internal sealed class CombatView_UI_Actions(GameDatabase database, ICombatViewActions combatViewActions)
    {
        private readonly GameDatabase _database = database;
        private readonly ICombatViewActions _combatViewActions = combatViewActions;

        public void Activate(ClientInfo myClientInfo, CombatRuntime combatRuntime)
        {
            CombatInstance combatInstance = combatRuntime.Instance;

            TurnState turnState = CombatTurnHelpers.DeriveInitialTurnState(_database, combatInstance);
            CombatUnitUniqueIdentifier? currentTurnUnitIdentifier = turnState.CurrentTurnUnitIdentifier ?? throw new NotImplementedException($"Initial turn order has no one going first");

            if (CombatUnitOwnershipHelpers.DoesClientOwnUnit(myClientInfo, currentTurnUnitIdentifier))
                CreateActionButtonsForUnit(combatInstance, currentTurnUnitIdentifier);
        }

        public void Deactivate()
        {
            _combatViewActions.DeleteAllActionButtons();
            _combatViewActions.HideEndTurnButton();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            return gameEvent switch
            {
                CombatEvent_ActionStarted => HandleActionStarted(),
                CombatEvent_ActionEnded => HandleActionEnded(context),
                CombatEvent_UnitTurnEnded => HandleUnitTurnEnded(),
                CombatEvent_UnitTurnStarted turnStartedEvent => HandleUnitTurnStarted(turnStartedEvent, context),
                NetworkGameEvent_Run_Combat_Defeat => HandleChange_Defeat(),
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        private ChangeHandledResult_Finished HandleActionStarted()
        {
            _combatViewActions.DeleteAllActionButtons();
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleActionEnded(ViewHandleChangeContext context)
        {
            ClientGameState currentClientGameState = context.CurrentClientGameState;
            ClientInfo myClientInfo = currentClientGameState.GetMyClientInfo();
            NetworkGameState nextNetworkGameState = context.AfterApplyNetworkGameState;
            CombatUnitUniqueIdentifier nextTurnUnitIdentifier = nextNetworkGameState.DeriveCurrentTurnUnitIdentifier(_database);
            if (!CombatUnitOwnershipHelpers.DoesClientOwnUnit(myClientInfo, nextTurnUnitIdentifier))
                return ChangeHandledResult_Finished.Instance;

            CombatInstance nextCombatInstance = nextNetworkGameState.GetCombatInstance();
            CreateActionButtonsForUnit(nextCombatInstance, nextTurnUnitIdentifier);
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleUnitTurnEnded()
        {
            _combatViewActions.DeleteAllActionButtons();
            _combatViewActions.HideEndTurnButton();
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleUnitTurnStarted(CombatEvent_UnitTurnStarted turnStartedEvent, ViewHandleChangeContext context)
        {
            _combatViewActions.DeleteAllActionButtons();
            _combatViewActions.HideEndTurnButton();

            ClientGameState clientGameState = context.CurrentClientGameState;
            ClientInfo myClientInfo = clientGameState.GetMyClientInfo();
            CombatRuntime combatRuntime = context.AfterApplyNetworkGameState.GetCombatRuntime();
            CombatInstance combatInstance = combatRuntime.Instance;
            CombatState combatState = combatRuntime.State;
            CombatUnitUniqueIdentifier unitIdentifier = turnStartedEvent.UnitIdentifier;

            UnitState unit = combatState.GetUnit(unitIdentifier);
            if (unit.IsDead() || unit.HasSkipTurnNonStackableEffect(_database))
                return ChangeHandledResult_Finished.Instance;

            if (!CombatUnitOwnershipHelpers.DoesClientOwnUnit(myClientInfo, unitIdentifier))
                return ChangeHandledResult_Finished.Instance;

            CreateActionButtonsForUnit(combatInstance, unitIdentifier);
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleChange_Defeat()
        {
            Deactivate();
            return ChangeHandledResult_Finished.Instance;
        }

        private void CreateActionButtonsForUnit(CombatInstance combatInstance, CombatUnitUniqueIdentifier currentTurnUnitIdentifier)
        {
            UnitDefinition currentTurnUnitDefinition = _database.GetUnitDefinition(currentTurnUnitIdentifier);
            string moveActionId = currentTurnUnitDefinition.MoveCombatActionId;

            UnitBuild currentTurnUnitBuild = combatInstance.GetUnitBuild(currentTurnUnitIdentifier);
            ValueArray<string> actionIds = currentTurnUnitBuild.CombatActionIds;

            CreateActionButtons(moveActionId, actionIds);
        }

        private void CreateActionButtons(string moveActionId, ValueArray<string> actionIds)
        {
            if (!string.IsNullOrEmpty(moveActionId))
                _combatViewActions.CreateActionButton(moveActionId);

            foreach (string combatActionId in actionIds)
            {
                _combatViewActions.CreateActionButton(combatActionId);
            }

            _combatViewActions.ShowEndTurnButton();
        }
    }
}
