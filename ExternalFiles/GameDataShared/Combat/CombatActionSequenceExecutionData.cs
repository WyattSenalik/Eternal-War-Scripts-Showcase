using EternalWar.Core.Model.Combat.Common;
using System;

namespace EternalWar.Core.Combat
{
    [Serializable]
    public sealed record CombatActionSequenceExecutionData(string SequenceResourcePath, CombatUnitUniqueIdentifier UserUnitIdentifier, GridPositionGroupListWrapper SelectedGridPositionGroups);
}