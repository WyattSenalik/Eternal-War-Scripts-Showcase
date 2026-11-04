// Original Authors - Jack Dekko
using EternalWar.Client.Application.Run.Map.Actions;
using EternalWar.Client.Application.Run.Map.Interaction.Actions;

namespace EternalWar.Client.Application.Run.Map.Interaction.Controllers
{
    public class MapInteractionController(IMapActions mapActions) : IMapInteractionActions
    {
        public void ButtonPressed_VoteForNode(int nodeIndex)
        {
            mapActions.VoteForNode(nodeIndex);
        }
    }
}
