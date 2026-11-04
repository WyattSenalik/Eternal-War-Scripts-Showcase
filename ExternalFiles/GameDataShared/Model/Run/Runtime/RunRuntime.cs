// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Model.Run.Runtime
{
    public sealed record RunRuntime(RunInstance Instance, RunState State) : GamePhase;
}