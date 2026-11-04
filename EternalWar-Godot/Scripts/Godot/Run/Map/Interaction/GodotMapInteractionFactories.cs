// Original Authors - Jack Dekko
using EternalWar.Client.Application.Run.Map.Interaction.Actions;
using GodotExtensions;

namespace EternalWar.Engine.Godot.Run.Map.Interaction
{
    public sealed class GodotMapInteractionFactories
    {
        public ButtonFactory<int> MapActionButtonFactory => _mapActionButtonFactory;

        private readonly ButtonFactory<int> _mapActionButtonFactory;
        private readonly IMapInteractionActions _mapActions;

        public GodotMapInteractionFactories(GodotGameVisualDatabase visualDatabase, IMapInteractionActions mapActions)
        {
            _mapActionButtonFactory = new ButtonFactory<int>(visualDatabase.MapNodePrefab, OnButtonPressed_VoteForNode);
            _mapActions = mapActions;
        }

        private void OnButtonPressed_VoteForNode(int nodeIndex)
        {
            _mapActions.ButtonPressed_VoteForNode(nodeIndex);
        }
    }
}
