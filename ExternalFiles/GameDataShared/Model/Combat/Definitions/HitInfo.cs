// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record HitInfo(NCalcFunctionInfo CalcFunction, bool HasMinimumValue, int MinimumValue, HitRulesInfo HitRules, bool ShouldInvokeOnHitEvent)
    {
        public static readonly HitInfo Empty = new(NCalcFunctionInfo.Empty, false, 0, HitRulesInfo.Empty, false);
    }

    public sealed record HitRulesInfo(bool CanHitEnemies, bool CanHitAllies, bool CanHitNeutral, bool CanHitSelf)
    {
        public static readonly HitRulesInfo Empty = new(false, false, false, false);
    }

    public static class HitInfoExtensions
    {
        public static float GetValue(this HitInfo hitInfo, ValueFunctionContext context)
        {
            NCalcFunctionInfo calcFunction = hitInfo.CalcFunction;
            bool hasMinimumValue = hitInfo.HasMinimumValue;
            int minimumValue = hitInfo.MinimumValue;

            float value = calcFunction.Evaluate(context);
            if (hasMinimumValue)
                value = MathF.Max(value, minimumValue);
            return value;
        }
    }
}
