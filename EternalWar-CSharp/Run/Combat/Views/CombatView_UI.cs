// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Network;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    internal sealed class CombatView_UI(GameDatabase database, ICombatViewActions combatViewActions)
    {
        private readonly CombatView_UI_Actions _actionsView = new(database, combatViewActions);
        private readonly CombatView_UI_TurnOrder _turnOrderView = new(database, combatViewActions);
        private readonly CombatView_UI_End _endView = new(database, combatViewActions);

        public void Activate(ClientInfo myClientInfo, CombatRuntime combatRuntime)
        {
            _actionsView.Activate(myClientInfo, combatRuntime);
            _turnOrderView.Activate(combatRuntime);
        }

        public void Deactivate()
        {
            _actionsView.Deactivate();
            _turnOrderView.Deactivate();
            _endView.Deactivate();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            ChangeHandledResult actionsResult = _actionsView.HandleEvent(gameEvent, context);
            ChangeHandledResult turnOrderResult = _turnOrderView.HandleEvent(gameEvent, context);
            ChangeHandledResult endResult = _endView.HandleEvent(gameEvent, context);
            return actionsResult.Combine(turnOrderResult, endResult);
        }
    }
}
