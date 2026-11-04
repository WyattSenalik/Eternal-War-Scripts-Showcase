// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public sealed record CombatState(string DefinitionId, ValueDictionary<CombatUnitUniqueIdentifier, UnitState> UnitMap)
    {
        // TODO: Maybe eventually hold something for blocks and the grid state here in case we can destroy terrain in the future (and if that terrain isn't just a unit [which it most likely would be])
    }
}
