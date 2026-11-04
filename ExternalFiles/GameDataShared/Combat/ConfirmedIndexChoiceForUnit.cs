// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using System;

namespace EternalWar.Core.Combat
{
    [Serializable]
    public sealed record ConfirmedIndexChoiceForUnit(CombatUnitUniqueIdentifier UnitIdentifier, int SelectedChoiceIndex);
}
