// Original Authors - Jack Dekko
using EternalWar.Core.Model.Map.Runtime;

namespace EternalWar.Core.Model.Map.Definitions
{
    public sealed record FakeLocationMapDefinition(string Id, LocationMapInstance LocationMapInstance): IHaveStringId;
}
