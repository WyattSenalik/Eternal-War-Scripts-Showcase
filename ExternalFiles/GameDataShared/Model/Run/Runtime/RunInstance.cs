// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;

namespace EternalWar.Core.Model.Run.Runtime
{
    public sealed record RunInstance(ValueArray<CombatUnitUniqueIdentifier> PlayerUnitIdentifiers);
}