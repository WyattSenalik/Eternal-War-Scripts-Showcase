// Original Authors - Wyatt Senalik
namespace EternalWar.Client.Application.Model.Interaction
{
    public abstract record InteractionStateChange
    {
        public abstract InteractionState Apply(InteractionState oldState);
    }
}
