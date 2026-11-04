// Original Authors - Wyatt Senalik
namespace EternalWar.Client.Application
{
    public interface IGameViewComposer
    {
        GameViewActions Compose(GameInteractionActions interactionActions);
    }
}
