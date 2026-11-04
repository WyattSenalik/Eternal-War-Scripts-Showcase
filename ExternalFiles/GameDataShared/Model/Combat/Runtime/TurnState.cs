// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;

namespace EternalWar.Core.Model.Combat.Runtime
{
    // This is not held state, but derived state
    public sealed record TurnState(ValueArray<UnitTurnState> TurnOrder, CombatUnitUniqueIdentifier? CurrentTurnUnitIdentifier);

    public sealed record UnitTurnState(CombatUnitUniqueIdentifier Identifier, bool ShouldSkipTurn);
}
