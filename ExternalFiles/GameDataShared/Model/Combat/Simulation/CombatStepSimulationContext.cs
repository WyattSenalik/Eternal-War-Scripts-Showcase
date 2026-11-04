// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Random;

namespace EternalWar.Core.Model.Combat.Simulation
{
    public sealed record CombatStepSimulationContext(CombatCommandProcessor CommandProcessor, ActionExecutionContext ActionContext, int Step, int SelectionGroupIndex, int? PassiveStacks = null);

    public sealed record ActionExecutionContext(string ActionSequenceId, CombatUnitUniqueIdentifier UserIdentifier, ActionSelectionContext SelectionContext);

    public sealed record ActionSelectionContext(ValueArray<GridPositionGroup> SelectionGroups);

    public sealed record SimulationOptions(IRandomResolutionResolver RandomHandler);

    public static class CombatStepSimulationContextExtensions
    {
        public static ValueFunctionContext CreateValueFunctionContext(this CombatStepSimulationContext simulationContext, CombatStats stats)
        {
            return new ValueFunctionContext(stats, simulationContext.PassiveStacks);
        }

        public static UnitState GetUserUnit(this CombatStepSimulationContext simulationContext)
        {
            ActionExecutionContext actionContext = simulationContext.ActionContext;
            CombatUnitUniqueIdentifier userIdentifier = actionContext.UserIdentifier;
            CombatCommandProcessor commandProcessor = simulationContext.CommandProcessor;
            CombatState combatState = commandProcessor.CurrentState;

            return combatState.GetUnit(userIdentifier);
        }
    }
}

