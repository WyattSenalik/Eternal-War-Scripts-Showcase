// Original Authors - Jack Dekko & Wyatt Senalik
using EternalWar.Core.Model.Run.Runtime;

namespace EternalWar.Core.Model.Map.Runtime
{
    public sealed record LocationMapRuntime(LocationMapInstance Instance, LocationMapState State) : RunPhase;
}
