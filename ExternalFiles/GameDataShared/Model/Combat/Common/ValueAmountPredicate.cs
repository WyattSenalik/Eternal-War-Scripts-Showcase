// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Common
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "comparisonType")]
    [JsonDerivedType(typeof(ValueAmountPredicate_And), "And")]
    [JsonDerivedType(typeof(ValueAmountPredicate_Or), "Or")]
    [JsonDerivedType(typeof(ValueAmountPredicate_Not), "Not")]
    [JsonDerivedType(typeof(ValueAmountPredicate_LessThan), "LessThan")]
    [JsonDerivedType(typeof(ValueAmountPredicate_GreaterThan), "GreaterThan")]
    [JsonDerivedType(typeof(ValueAmountPredicate_LessThanOrEqualTo), "LessThanOrEqualTo")]
    [JsonDerivedType(typeof(ValueAmountPredicate_GreaterThanOrEqualTo), "GreaterThanOrEqualTo")]
    [JsonDerivedType(typeof(ValueAmountPredicate_Equal), "Equal")]
    public abstract record ValueAmountPredicate;

    public sealed record ValueAmountPredicate_And(ValueArray<ValueAmountPredicate> Predicates) : ValueAmountPredicate;
    public sealed record ValueAmountPredicate_Or(ValueArray<ValueAmountPredicate> Predicates) : ValueAmountPredicate;
    public sealed record ValueAmountPredicate_Not(ValueAmountPredicate Predicate) : ValueAmountPredicate;
    public sealed record ValueAmountPredicate_LessThan(ValueAmount ValueAmount) : ValueAmountPredicate;
    public sealed record ValueAmountPredicate_GreaterThan(ValueAmount ValueAmount) : ValueAmountPredicate;
    public sealed record ValueAmountPredicate_LessThanOrEqualTo(ValueAmount ValueAmount) : ValueAmountPredicate;
    public sealed record ValueAmountPredicate_GreaterThanOrEqualTo(ValueAmount ValueAmount) : ValueAmountPredicate;
    public sealed record ValueAmountPredicate_Equal(ValueAmount ValueAmount) : ValueAmountPredicate;

    public static class ValueAmountPredicateExtensions
    {
        public static bool Evaluate(this ValueAmountPredicate predicate, float max, float value)
        {
            return predicate switch
            {
                ValueAmountPredicate_And andPredicate => andPredicate.Evaluate(max, value),
                ValueAmountPredicate_Or orPredicate => orPredicate.Evaluate(max, value),
                ValueAmountPredicate_Not notPredicate => notPredicate.Evaluate(max, value),
                ValueAmountPredicate_LessThan lessThanPredicate => lessThanPredicate.Evaluate(max, value),
                ValueAmountPredicate_GreaterThan greaterThanPredicate => greaterThanPredicate.Evaluate(max, value),
                ValueAmountPredicate_LessThanOrEqualTo lessThanOrEqualToPredicate => lessThanOrEqualToPredicate.Evaluate(max, value),
                ValueAmountPredicate_GreaterThanOrEqualTo greaterThanOrEqualToPredicate => greaterThanOrEqualToPredicate.Evaluate(max, value),
                ValueAmountPredicate_Equal equalPredicate => equalPredicate.Evaluate(max, value),
                _ => throw new NotImplementedException($"Unhandled {nameof(NumericPredicate)}: ({predicate.GetType()})")
            };
        }

        public static bool Evaluate(this ValueAmountPredicate_And predicate, float max, float value)
        {
            foreach (ValueAmountPredicate subPredicate in predicate.Predicates)
            {
                if (!subPredicate.Evaluate(max, value))
                    return false;
            }
            return true;
        }

        public static bool Evaluate(this ValueAmountPredicate_Or predicate, float max, float value)
        {
            foreach (ValueAmountPredicate subPredicate in predicate.Predicates)
            {
                if (subPredicate.Evaluate(max, value))
                    return true;
            }
            return false;
        }

        public static bool Evaluate(this ValueAmountPredicate_Not predicate, float max, float value)
        {
            ValueAmountPredicate subPredicate = predicate.Predicate;
            return !subPredicate.Evaluate(max, value);
        }

        public static bool Evaluate(this ValueAmountPredicate_LessThan predicate, float max, float value)
        {
            ValueAmount compareValueAmount = predicate.ValueAmount;
            float compareValue = compareValueAmount.Evaluate(max);
            return value < compareValue;
        }

        public static bool Evaluate(this ValueAmountPredicate_GreaterThan predicate, float max, float value)
        {
            ValueAmount compareValueAmount = predicate.ValueAmount;
            float compareValue = compareValueAmount.Evaluate(max);
            return value > compareValue;
        }

        public static bool Evaluate(this ValueAmountPredicate_LessThanOrEqualTo predicate, float max, float value)
        {
            ValueAmount compareValueAmount = predicate.ValueAmount;
            float compareValue = compareValueAmount.Evaluate(max);
            return value <= compareValue;
        }

        public static bool Evaluate(this ValueAmountPredicate_GreaterThanOrEqualTo predicate, float max, float value)
        {
            ValueAmount compareValueAmount = predicate.ValueAmount;
            float compareValue = compareValueAmount.Evaluate(max);
            return value >= compareValue;
        }

        public static bool Evaluate(this ValueAmountPredicate_Equal predicate, float max, float value)
        {
            ValueAmount compareValueAmount = predicate.ValueAmount;
            float compareValue = compareValueAmount.Evaluate(max);
            return value == compareValue;
        }
    }
}
