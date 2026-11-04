// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System;
using TargetSelectionType = EternalWar.Core.Model.Combat.Definitions.SelectionType;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionType
{
    public sealed record SelectionTypeHandlerContext(GameDatabase Database, CombatInstance CombatInstance, CombatState CombatState, CombatUnitUniqueIdentifier UserIdentifier, SelectionSpecificationsInfo SelectionSpecifications);

    public interface ISelectionTypeHandler
    {
        void GetTilesInRange(SelectionTypeHandlerContext context, out GridPositionGroup selectableTiles, out GridPositionGroup unselectableTiles);
    }

    public static class SelectionTypeHandlerFactory
    {
        public static ISelectionTypeHandler CreateHandler(TargetSelectionType selectionType)
        {
            return selectionType switch
            {
                TargetSelectionType.Any => new SelectionTypeHandler_Any(),
                TargetSelectionType.Unblocked => new SelectionTypeHandler_Unblocked(),
                TargetSelectionType.Blocked => new SelectionTypeHandler_Blocked(),
                TargetSelectionType.HasUnit => new SelectionTypeHandler_HasUnit(),
                _ => throw new NotImplementedException($"Unhandled {nameof(TargetSelectionType)}: ({selectionType})")
            };
        }
    }
}
