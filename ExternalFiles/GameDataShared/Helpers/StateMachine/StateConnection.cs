// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Helpers.StateMachine
{
    public sealed record StateConnection(IState FromState, IState ToState);
}