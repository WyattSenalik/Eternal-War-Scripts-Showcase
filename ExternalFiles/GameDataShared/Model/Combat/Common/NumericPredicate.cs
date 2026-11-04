// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Common
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "comparisonType")]
    [JsonDerivedType(typeof(NumericPredicate_And), "And")]
    [JsonDerivedType(typeof(NumericPredicate_Or), "Or")]
    [JsonDerivedType(typeof(NumericPredicate_Not), "Not")]
    [JsonDerivedType(typeof(NumericPredicate_LessThan), "LessThan")]
    [JsonDerivedType(typeof(NumericPredicate_GreaterThan), "GreaterThan")]
    [JsonDerivedType(typeof(NumericPredicate_LessThanOrEqualTo), "LessThanOrEqualTo")]
    [JsonDerivedType(typeof(NumericPredicate_GreaterThanOrEqualTo), "GreaterThanOrEqualTo")]
    [JsonDerivedType(typeof(NumericPredicate_Equal), "Equal")]
    public abstract record NumericPredicate;

    public sealed record NumericPredicate_And(ValueArray<NumericPredicate> Predicates) : NumericPredicate;
    public sealed record NumericPredicate_Or(ValueArray<NumericPredicate> Predicates) : NumericPredicate;
    public sealed record NumericPredicate_Not(NumericPredicate Predicate) : NumericPredicate;
    public sealed record NumericPredicate_LessThan(float Value) : NumericPredicate;
    public sealed record NumericPredicate_GreaterThan(float Value) : NumericPredicate;
    public sealed record NumericPredicate_LessThanOrEqualTo(float Value) : NumericPredicate;
    public sealed record NumericPredicate_GreaterThanOrEqualTo(float Value) : NumericPredicate;
    public sealed record NumericPredicate_Equal(float Value) : NumericPredicate;

    public static class NumericPredicateExtensions
    {
        public static bool Evaluate(this NumericPredicate predicate, float value)
        {
            return predicate switch
            {
                NumericPredicate_And andPredicate => andPredicate.Evaluate(value),
                NumericPredicate_Or orPredicate => orPredicate.Evaluate(value),
                NumericPredicate_Not notPredicate => notPredicate.Evaluate(value),
                NumericPredicate_LessThan lessThanPredicate => lessThanPredicate.Evaluate(value),
                NumericPredicate_GreaterThan greaterThanPredicate => greaterThanPredicate.Evaluate(value),
                NumericPredicate_LessThanOrEqualTo lessThanOrEqualToPredicate => lessThanOrEqualToPredicate.Evaluate(value),
                NumericPredicate_GreaterThanOrEqualTo greaterThanOrEqualToPredicate => greaterThanOrEqualToPredicate.Evaluate(value),
                NumericPredicate_Equal equalPredicate => equalPredicate.Evaluate(value),
                _ => throw new NotImplementedException($"Unhandled {nameof(NumericPredicate)}: ({predicate.GetType()})")
            };
        }

        public static bool Evaluate(this NumericPredicate_And predicate, float value)
        {
            foreach (NumericPredicate subPredicate in predicate.Predicates)
            {
                if (!subPredicate.Evaluate(value))
                    return false;
            }
            return true;
        }

        public static bool Evaluate(this NumericPredicate_Or predicate, float value)
        {
            foreach (NumericPredicate subPredicate in predicate.Predicates)
            {
                if (subPredicate.Evaluate(value))
                    return true;
            }
            return false;
        }

        public static bool Evaluate(this NumericPredicate_Not predicate, float value)
        {
            NumericPredicate subPredicate = predicate.Predicate;
            return !subPredicate.Evaluate(value);
        }

        public static bool Evaluate(this NumericPredicate_LessThan predicate, float value)
        {
            float compareValue = predicate.Value;
            return value < compareValue;
        }

        public static bool Evaluate(this NumericPredicate_GreaterThan predicate, float value)
        {
            float compareValue = predicate.Value;
            return value > compareValue;
        }

        public static bool Evaluate(this NumericPredicate_LessThanOrEqualTo predicate, float value)
        {
            float compareValue = predicate.Value;
            return value <= compareValue;
        }

        public static bool Evaluate(this NumericPredicate_GreaterThanOrEqualTo predicate, float value)
        {
            float compareValue = predicate.Value;
            return value >= compareValue;
        }

        public static bool Evaluate(this NumericPredicate_Equal predicate, float value)
        {
            float compareValue = predicate.Value;
            return value == compareValue;
        }
    }
}
