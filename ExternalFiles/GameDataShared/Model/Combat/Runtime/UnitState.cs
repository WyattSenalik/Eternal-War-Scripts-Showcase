// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Runtime
{
    // TODO: Add artifact state at some point?
    public sealed record UnitState(CombatUnitUniqueIdentifier UniqueIdentifier, TeamAffiliation CurrentAffiliation, int CurrentHealth, int CurrentActionPoints, bool HasTakenTurn, GridPosition GridPosition, EightDirection FacingDirection, ValueArray<PassiveState> ActivePassives, ValueArray<string> ActiveNonStackableEffectIds, ValueArray<TemporaryStatModifierState> TemporaryStatModifiers, CombatUnitUniqueIdentifier? UnitIdentifierWhoMostRecentlyDamagedMe);
}
