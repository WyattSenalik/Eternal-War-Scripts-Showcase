// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Helpers.StateMachine
{
    public interface IState
    {
        void Enter(object? arguments);
#pragma warning disable CA1716 // Identifiers should not match keywords
        void Exit();
#pragma warning restore CA1716 // Identifiers should not match keywords
    }
}