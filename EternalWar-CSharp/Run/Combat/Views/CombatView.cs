// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Network;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    internal sealed class CombatView(GameDatabase database, ICombatViewActions combatViewActions)
    {
        private readonly ICombatViewActions _combatViewActions = combatViewActions;
        private readonly CombatView_World _worldView = new(database, combatViewActions);
        private readonly CombatView_UI _uiView = new(database, combatViewActions);

        public void Activate(ClientInfo myClientInfo, CombatRuntime combatRuntime)
        {
            _worldView.Activate(combatRuntime);
            _uiView.Activate(myClientInfo, combatRuntime);
            _combatViewActions.Show();
        }

        public void Deactivate()
        {
            _combatViewActions.Hide();
            _worldView.Deactivate();
            _uiView.Deactivate();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            ChangeHandledResult worldResult = _worldView.HandleEvent(gameEvent, context);
            ChangeHandledResult uiResult = _uiView.HandleEvent(gameEvent, context);
            return worldResult.Combine(uiResult);
        }
    }
}
