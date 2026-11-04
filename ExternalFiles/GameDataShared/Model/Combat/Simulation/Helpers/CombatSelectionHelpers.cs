// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Handlers;
using EternalWar.Core.Model.Combat.Simulation.Handlers.SelectionType;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    /// <summary>Helpers for determining what can be selected by action sequences.</summary>
    public static class CombatSelectionHelpers
    {
        public static bool AreSelectionsValid(GameDatabase database, CombatRuntime combatRuntime, CombatUnitUniqueIdentifier userIdentifier, ActionExecutionContext actionExecutionContext)
        {
            string actionSequenceId = actionExecutionContext.ActionSequenceId;
            CombatActionSequenceDefinition actionSequenceDefinition = database.GetActionSequenceDefinition(actionSequenceId);

            ActionSelectionContext selectionContext = actionExecutionContext.SelectionContext;
            ValueArray<GridPositionGroup> actualSelectionGroups = selectionContext.SelectionGroups;
            bool isValidSelection = false;
            ForEachSelectionCombination(database, combatRuntime.Instance, combatRuntime.State, userIdentifier, actionSequenceDefinition, testSelectionGroups =>
            {
                // TODO: An earyly break would be nice here
                if (actualSelectionGroups == testSelectionGroups)
                    isValidSelection = true;
            });
            return isValidSelection;
        }

        public static void ForEachSelectionCombination(GameDatabase database, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, CombatActionSequenceDefinition actionDefinition, Action<ValueArray<GridPositionGroup>> callback)
        {
            using PooledList<(GridPositionGroup Group, int Amount)> selections = new();
            GetSelectionsForAction(database, combatInstance, combatState, userIdentifier, actionDefinition, selections);
            selections.ForEachCombinationSet(callback);
        }

        public static List<(GridPositionGroup Group, int Amount)> GetSelectionsForAction(GameDatabase database, CombatRuntime combatRuntime, CombatUnitUniqueIdentifier userIdentifier, string actionDefinitionId) => GetSelectionsForAction(database, combatRuntime.Instance, combatRuntime.State, userIdentifier, database.GetActionSequenceDefinition(actionDefinitionId));
        public static List<(GridPositionGroup Group, int Amount)> GetSelectionsForAction(GameDatabase database, CombatRuntime combatRuntime, CombatUnitUniqueIdentifier userIdentifier, CombatActionSequenceDefinition actionDefinition) => GetSelectionsForAction(database, combatRuntime.Instance, combatRuntime.State, userIdentifier, actionDefinition);
        public static List<(GridPositionGroup Group, int Amount)> GetSelectionsForAction(GameDatabase database, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, CombatActionSequenceDefinition actionDefinition)
        {
            List<(GridPositionGroup Group, int Amount)> selections = new();
            GetSelectionsForAction(database, combatInstance, combatState, userIdentifier, actionDefinition, selections);
            return selections;
        }

        public static void DetermineSelectableTiles(GameDatabase database, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, SelectionSpecificationsInfo selectionSpecifications, out GridPositionGroup selectableTiles, out GridPositionGroup unselectableTiles)
        {
            SelectionType selectionType = selectionSpecifications.Type;
            ISelectionTypeHandler selectionHandler = CombatHandlersRegistration.GetHandler(selectionType);
            SelectionTypeHandlerContext context = new(database, combatInstance, combatState, userIdentifier, selectionSpecifications);
            selectionHandler.GetTilesInRange(context, out selectableTiles, out unselectableTiles);
        }

        private static void GetSelectionsForAction(GameDatabase database, CombatInstance combatInstance, CombatState combatState, CombatUnitUniqueIdentifier userIdentifier, CombatActionSequenceDefinition actionDefinition, ICollection<(GridPositionGroup Group, int Amount)> outList)
        {
            outList.Clear();
            ValueArray<CombatStepInfo> steps = actionDefinition.Steps;
            foreach (CombatStepInfo stepInfo in steps)
            {
                if (!stepInfo.DoesUseSelection())
                    continue;

                TileLocationInfo locationInfo = stepInfo.Location;
                SelectionSpecificationsInfo selectionSpecifications = locationInfo.SelectionSpecifications;
                DetermineSelectableTiles(database, combatInstance, combatState, userIdentifier, selectionSpecifications, out GridPositionGroup tilesInRangeGroup, out _);
                int amount = selectionSpecifications.Amount;
                outList.Add((tilesInRangeGroup, amount));
            }
            // Want to still have a "selection" considered for an action that doesn't require any
            if (outList.Count <= 0)
                outList.Add((GridPositionGroup.EMPTY, 0));
        }
    }
}
