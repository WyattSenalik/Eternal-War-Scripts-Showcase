// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Runtime;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    internal sealed class CombatView_World(GameDatabase database, ICombatViewActions combatViewActions)
    {
        private readonly CombatView_World_Units _unitsView = new(database, combatViewActions);
        private readonly CombatView_World_Blocks _blocksView = new(database, combatViewActions);

        public void Activate(CombatRuntime combatRuntime)
        {
            _unitsView.Activate(combatRuntime);
            _blocksView.Activate(combatRuntime);
        }

        public void Deactivate()
        {
            _unitsView.Deactivate();
            _blocksView.Deactivate();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            ChangeHandledResult unitsResult = _unitsView.HandleEvent(gameEvent, context);
            // TODO: Maybe add one for blocks?
            return unitsResult;
        }
    }
}
