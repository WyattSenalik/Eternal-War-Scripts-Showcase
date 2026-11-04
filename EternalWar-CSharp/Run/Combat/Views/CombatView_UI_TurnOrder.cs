// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    internal sealed class CombatView_UI_TurnOrder(GameDatabase database, ICombatViewActions combatViewActions)
    {
        private readonly GameDatabase _database = database;
        private readonly ICombatViewActions _combatViewActions = combatViewActions;

        public void Activate(CombatRuntime combatRuntime)
        {
            TurnState turnState = CombatTurnHelpers.DeriveInitialTurnState(_database, combatRuntime);
            CombatUnitUniqueIdentifier currentTurnUnitIdentifier = turnState.CurrentTurnUnitIdentifier ?? throw new NotImplementedException($"Need to handle if it's no one's turn");

            ValueArray<UnitTurnState> turnOrder = turnState.TurnOrder;
            foreach (UnitTurnState unitTurnState in turnOrder)
            {
                CombatUnitUniqueIdentifier unitIdentifier = unitTurnState.Identifier;
                GetRelevantUnitInformation(_database, combatRuntime, unitIdentifier, out TeamAffiliation unitAffiliation, out int unitCurrentHealth, out int unitMaxHealth);

                _combatViewActions.CreateUnitTurnCell(unitIdentifier, unitAffiliation, unitCurrentHealth, unitMaxHealth);
                if (unitIdentifier == currentTurnUnitIdentifier)
                    continue;
                _combatViewActions.SetUnitTurnCellState(unitIdentifier, UnitTurnCellState.YetToTakeTurn);
            }

            _combatViewActions.SetUnitTurnCellState(currentTurnUnitIdentifier, UnitTurnCellState.TakingTurnNow);
        }

        public void Deactivate()
        {
            _combatViewActions.DeleteAllUnitTurnCells();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            return gameEvent switch
            {
                CombatEvent_DamageApplied damageAppliedEvent => HandleChange_CombatEvent_DamageApplied(damageAppliedEvent, context),
                CombatEvent_HealApplied healAppliedEvent => HandleChange_CombatEvent_HealApplied(healAppliedEvent, context),
                CombatEvent_UnitDied unitDiedEvent => HandleChange_CombatEvent_UnitDied(unitDiedEvent),
                CombatEvent_UnitTurnEnded turnEndedEvent => HandleChange_TurnEnded(turnEndedEvent, context),
                CombatEvent_UnitTurnStarted turnStartedEvent => HandleChange_TurnStarted(turnStartedEvent, context),
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        private ChangeHandledResult_Finished HandleChange_TurnEnded(CombatEvent_UnitTurnEnded turnEndedEvent, ViewHandleChangeContext context)
        {
            CombatUnitUniqueIdentifier turnEndedUnitIdentifer = turnEndedEvent.UnitIdentifier;
            ClientGameState turnEndedGameState = context.CurrentClientGameState;
            CombatRuntime turnEndedCombatRuntime = turnEndedGameState.GetCombatRuntime();
            GetRelevantUnitInformation(_database, turnEndedCombatRuntime, turnEndedUnitIdentifer, out _, out int unitCurrentHealth, out _);

            if (unitCurrentHealth > 0)
                _combatViewActions.SetUnitTurnCellState(turnEndedEvent.UnitIdentifier, UnitTurnCellState.YetToTakeTurn);
            return ChangeHandledResult_Finished.Instance;

        }

        private ChangeHandledResult_Finished HandleChange_TurnStarted(CombatEvent_UnitTurnStarted turnStartedEvent, ViewHandleChangeContext context)
        {
            CombatUnitUniqueIdentifier turnStartedUnitIdentifer = turnStartedEvent.UnitIdentifier;
            ClientGameState turnStartedGameState = context.CurrentClientGameState;
            CombatRuntime turnStartedCombatRuntime = turnStartedGameState.GetCombatRuntime();
            GetRelevantUnitInformation(_database, turnStartedCombatRuntime, turnStartedUnitIdentifer, out _, out int unitCurrentHealth, out _);

            if (unitCurrentHealth > 0)
                _combatViewActions.SetUnitTurnCellState(turnStartedEvent.UnitIdentifier, UnitTurnCellState.TakingTurnNow);
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleChange_CombatEvent_DamageApplied(CombatEvent_DamageApplied combatEvent, ViewHandleChangeContext context)
        {
            CombatUnitUniqueIdentifier targetIdentifier = combatEvent.TargetIdentifier;
            int targetHealth = combatEvent.ResultingHealth;
            return HandleUnitHealthChanged(targetIdentifier, targetHealth, context);
        }

        private ChangeHandledResult_Finished HandleChange_CombatEvent_HealApplied(CombatEvent_HealApplied combatEvent, ViewHandleChangeContext context)
        {
            CombatUnitUniqueIdentifier targetIdentifier = combatEvent.TargetIdentifier;
            int targetHealth = combatEvent.ResultingHealth;
            return HandleUnitHealthChanged(targetIdentifier, targetHealth, context);
        }

        private ChangeHandledResult_Finished HandleChange_CombatEvent_UnitDied(CombatEvent_UnitDied combatEvent)
        {
            CombatUnitUniqueIdentifier deadUnitIdentifier = combatEvent.DeadUnitIdentifier;
            UnitDefinition deadUnitDefinition = _database.GetUnitDefinition(deadUnitIdentifier);
            if (deadUnitDefinition.CanBeRevived)
            {
                _combatViewActions.SetUnitTurnCellState(deadUnitIdentifier, UnitTurnCellState.Dead);
                return ChangeHandledResult_Finished.Instance;
            }
            _combatViewActions.DeleteUnitTurnCell(deadUnitIdentifier);
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleUnitHealthChanged(CombatUnitUniqueIdentifier unitIdentifier, int resultingHealth, ViewHandleChangeContext context)
        {
            NetworkGameState nextNetworkGameState = context.AfterApplyNetworkGameState;
            CombatRuntime nextCombatRuntime = nextNetworkGameState.GetCombatRuntime();

            CombatStats nextTargetStats = CombatStatHelpers.CalculateStats(_database, nextCombatRuntime, unitIdentifier);
            HealthInfo nextTargetMaxHealthInfo = nextTargetStats.MaxHealthInfo;

            int targetMaxHealth = nextTargetMaxHealthInfo.Health;
            _combatViewActions.SetUnitHealthBar(unitIdentifier, resultingHealth, targetMaxHealth);

            return ChangeHandledResult_Finished.Instance;
        }

        private static void GetRelevantUnitInformation(GameDatabase database, CombatRuntime combatRuntime, CombatUnitUniqueIdentifier unitIdentifier, out TeamAffiliation unitAffiliation, out int unitCurrentHealth, out int unitMaxHealth)
        {
            CombatInstance combatInstance = combatRuntime.Instance;
            CombatState combatState = combatRuntime.State;

            UnitState unit = combatState.GetUnit(unitIdentifier);
            unitAffiliation = unit.CurrentAffiliation;
            CombatStats unitStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, unitIdentifier);
            unitCurrentHealth = unit.CurrentHealth;
            HealthInfo maxHealthInfo = unitStats.MaxHealthInfo;
            unitMaxHealth = maxHealthInfo.Health;
        }
    }
}
