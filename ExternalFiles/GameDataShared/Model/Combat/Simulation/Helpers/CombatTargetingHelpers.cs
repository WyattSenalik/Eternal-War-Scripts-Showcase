// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Handlers;
using EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    /// <summary>Helpers for determine who is affected by a combat actions.</summary>
    public static class CombatTargetingHelpers
    {
        public static IEnumerable<CombatUnitUniqueIdentifier> GetTargets(GameDatabase database, CombatState combatState, ActionExecutionContext actionContext, int selectionGroupIndex, IHaveTileLocation stepInfoWithTileLocation)
        {
            IReadOnlyDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;

            IReadOnlyList<GridPosition> gridPositions = GetTargetPositions(database, combatState, actionContext, selectionGroupIndex, stepInfoWithTileLocation);
            return unitMap.Where(kvp => gridPositions.Contains(kvp.Value.GridPosition)).Select(kvp => kvp.Key);
        }

        public static IEnumerable<CombatUnitUniqueIdentifier> GetTargets(GameDatabase database, CombatState combatState, ActionExecutionContext actionContext, int selectionGroupIndex, IHaveTileLocation stepInfoWithTileLocation, HitRulesInfo hitRules, CombatUnitUniqueIdentifier userIdentifier)
        {
            IReadOnlyDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;

            UnitState user = combatState.GetUnit(userIdentifier);
            IReadOnlyList<GridPosition> gridPositions = GetTargetPositions(database, combatState, actionContext, selectionGroupIndex, stepInfoWithTileLocation);

            return unitMap.Where(kvp => gridPositions.Contains(kvp.Value.GridPosition) && CombatTargetingRulesHelpers.AreHitRulesSatisfied(hitRules, user, kvp.Value, database)).Select(kvp => kvp.Key);
        }

        public static IReadOnlyList<GridPosition> GetTargetPositions(GameDatabase database, CombatState combatState, ActionExecutionContext actionContext, int selectionGroupIndex, IHaveTileLocation stepInfoWithTileLocation)
        {
            CombatUnitUniqueIdentifier userUniqueIdentifier = actionContext.UserIdentifier;
            ActionSelectionContext selectionContext = actionContext.SelectionContext;
            ValueArray<GridPositionGroup> selectionGroups = selectionContext.SelectionGroups;

            GridPositionGroup selectionGroup = selectionGroups[selectionGroupIndex];

            TileLocationInfo locationInfo = stepInfoWithTileLocation.Location;
            int locationValue = locationInfo.Value;
            TileLocation location = locationInfo.TileLocation;
            ITileLocationHandler locationHandler = CombatHandlersRegistration.GetHandler(location);
            TileLocationHandlerContext tileLocationContext = new(database, combatState, userUniqueIdentifier, selectionGroup, locationValue);
            return locationHandler.DetermineGridPositions(tileLocationContext);
        }

        public static GridPosition GetSingleTargetPosition(GameDatabase database, CombatState combatState, ActionExecutionContext actionContext, int selectionGroupIndex, IHaveTileLocation stepInfoWithTileLocation)
        {
            IReadOnlyList<GridPosition> allPositions = GetTargetPositions(database, combatState, actionContext, selectionGroupIndex, stepInfoWithTileLocation);
            return allPositions[0];
        }

        public static GridPosition GetCenterPositionOfTargets(GameDatabase database, CombatState combatState, ActionExecutionContext actionContext, int selectionGroupIndex, IHaveTileLocation stepInfoWithTileLocation)
        {
            IReadOnlyList<GridPosition> targetPositions = GetTargetPositions(database, combatState, actionContext, selectionGroupIndex, stepInfoWithTileLocation);
            Vector2 centerVector = Vector2.Zero;

            foreach (GridPosition position in targetPositions)
            {
                centerVector += position.Position.ToVector2();
            }

            centerVector /= targetPositions.Count;
            return new GridPosition(centerVector.RoundToInt());
        }
    }
}
