// Original Authors - Wyatt Senalik
using System;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Common
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "valueAmountType")]
    [JsonDerivedType(typeof(ValueAmount_Percentage), "Percentage")]
    [JsonDerivedType(typeof(ValueAmount_Fixed), "Fixed")]
    [JsonDerivedType(typeof(ValueAmount_PercentagePlusFixed), "PercentagePlusFixed")]
    public abstract record ValueAmount;

    /// <summary></summary>
    /// <param name="Percent">Percent from 0.0 to 1.0</param>
    public sealed record ValueAmount_Percentage(float Percent) : ValueAmount;
    public sealed record ValueAmount_Fixed(float Amount) : ValueAmount;
    /// <summary></summary>
    /// <param name="Percent">Percent from 0.0 to 1.0</param>
    public sealed record ValueAmount_PercentagePlusFixed(float Percent, float Amount) : ValueAmount;

    public static class IntValueAmountExtensions
    {
        public static float Evaluate(this ValueAmount valueAmount, float max)
        {
            return valueAmount switch
            {
                ValueAmount_Percentage percentageAmount => max * percentageAmount.Percent,
                ValueAmount_Fixed fixedAmount => fixedAmount.Amount,
                ValueAmount_PercentagePlusFixed combinedAmount => combinedAmount.Amount + (max * combinedAmount.Percent),
                _ => throw new NotImplementedException($"Unhandled {typeof(ValueAmount)}: ({valueAmount.GetType()})")
            };
        }
    }
}
