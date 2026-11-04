// Original Authors - Wyatt Senalik
using EternalWar.Engine.Godot.Run.Combat.Nodes;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatView_UI_End(GodotEndUINodeCollection endNodeCollection)
    {
        private readonly GodotEndUINodeCollection _endNodeCollection = endNodeCollection;

        public void ShowDefeatScreen()
        {
            _endNodeCollection.DefeatScreenRootNode.Show();
        }

        public void HideDefeatScreen()
        {
            _endNodeCollection.DefeatScreenRootNode.Hide();
        }

        public void ShowVictoryScreen()
        {
            _endNodeCollection.VictoryScreenRootNode.Show();
        }

        public void HideVictoryScreen()
        {
            _endNodeCollection.VictoryScreenRootNode.Hide();
        }
    }
}
