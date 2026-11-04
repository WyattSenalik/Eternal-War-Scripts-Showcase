// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record PassiveDefinition(string Id, bool CanStack, PassiveActivateTime ActivateTime, bool CanActivateWhenDead, PassiveConditionInfo ConditionInfo, bool HasActivationLimitPerEncounter, int MaxActivationsPerEncounter, PassiveActionInfo ActionInfo, ExpirationInfoGroup StackReductionExpirationInfo, NCalcFunctionInfo StackReductionNCalcFunction, PassiveActionInfo SeparateExpirationActionInfo) : IHaveStringId
    {
        public int Priority { get; init; }
    }

    public enum PassiveActivateTime
    {
        OnHit = 0,
        TurnStart = 1,
        TurnEnd = 2,
        OnApply = 3,
    }
}
