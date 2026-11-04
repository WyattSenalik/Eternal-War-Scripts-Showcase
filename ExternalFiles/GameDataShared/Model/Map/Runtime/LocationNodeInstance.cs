//Original Authors - Jack Dekko & Wyatt Senalik
using EternalWar.Core.Helpers;

namespace EternalWar.Core.Model.Map.Runtime
{
    public sealed record LocationNodeInstance(string NodeDefinitionId, ValueArray<int> ChildIndices);
}
