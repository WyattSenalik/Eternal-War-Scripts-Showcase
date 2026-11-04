// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Model;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    internal sealed class CombatView_UI_End(GameDatabase database, ICombatViewActions combatViewActions)
    {
        private readonly GameDatabase _database = database;
        private readonly ICombatViewActions _combatViewActions = combatViewActions;

        public void Deactivate()
        {
            _combatViewActions.HideVictoryScreen();
            _combatViewActions.HideDefeatScreen();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            return gameEvent switch
            {
                NetworkGameEvent_Run_Combat_Victory => HandleChange_Victory(),
                NetworkGameEvent_Run_Combat_Defeat => HandleChange_Defeat(),
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        private ChangeHandledResult_Finished HandleChange_Victory()
        {
            _combatViewActions.ShowVictoryScreen();
            return ChangeHandledResult_Finished.Instance;
        }

        private ChangeHandledResult_Finished HandleChange_Defeat()
        {
            _combatViewActions.ShowDefeatScreen();
            return ChangeHandledResult_Finished.Instance;
        }
    }
}
