// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common.Stats;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record NCalcFunctionInfo(string Function, ValueArray<NCalcVariableInfo> Variables)
    {
        public static readonly NCalcFunctionInfo Empty = new(string.Empty, ValueArray<NCalcVariableInfo>.Empty);
    }

    public sealed record NCalcVariableInfo(string Alias, ValueSource ValueSource);

    public enum ValueSource
    {
        FlatValueOfOne = 0,
        Health = 1,
        Strength = 2,
        Defense = 3,
        Speed = 4,
        Magic = 5,
        Will = 6,
        Movement = 7,
        ActionPoints = 8,
        Evasion = 9,
        Accuracy = 10,
        PassiveStacks = 11,
    }

    public sealed record ValueFunctionContext(CombatStats Stats, int? PassiveStacks)
    {
        public ValueFunctionContext(CombatStats Stats) : this(Stats, null) { }
    }

    public static class NCalcFunctionInfoExtensions
    {
        public static float Evaluate(this NCalcFunctionInfo calcFunction, ValueFunctionContext context)
        {
            string function = calcFunction.Function;
            IReadOnlyList<NCalcVariableInfo> variables = calcFunction.Variables;

            NCalc.Expression expression = new(function);
            foreach (NCalcVariableInfo currentVariable in variables)
            {
                string variableAlias = currentVariable.Alias;
                ValueSource valueSource = currentVariable.ValueSource;
                float value = valueSource.GetValue(context);
                expression.Parameters[variableAlias] = value;
            }
            object? rawValue = expression.Evaluate();
            return Convert.ToSingle(rawValue, CultureInfo.InvariantCulture);
        }
    }

    public static class ValueSourceExtensions
    {
        public static float GetValue(this ValueSource valueSource, ValueFunctionContext context)
        {
            ValueSource source = valueSource;
            if (source.IsStatType())
            {
                CombatStats stats = context.Stats;
                StatType sourceStat = source.ToStatType();
                return sourceStat.GetStatValue(stats);
            }

            int? passiveStacks = context.PassiveStacks;
            return source switch
            {
                ValueSource.FlatValueOfOne => 1,
                ValueSource.PassiveStacks => passiveStacks ?? throw new Exception($"No passive stacks were specified despite the value source being {ValueSource.PassiveStacks}"),
                _ => throw new NotImplementedException($"Unhandled {nameof(ValueSource)}: ({source})")
            };
        }

        public static bool IsStatType(this ValueSource valueSource)
        {
            return valueSource switch
            {
                ValueSource.Health => true,
                ValueSource.Strength => true,
                ValueSource.Defense => true,
                ValueSource.Speed => true,
                ValueSource.Magic => true,
                ValueSource.Will => true,
                ValueSource.Movement => true,
                ValueSource.ActionPoints => true,
                ValueSource.Evasion => true,
                ValueSource.Accuracy => true,
                _ => false
            };
        }

        public static StatType ToStatType(this ValueSource damageSource)
        {
            return damageSource switch
            {
                ValueSource.FlatValueOfOne => throw new Exception($"{nameof(ValueSource.FlatValueOfOne)} cannot be casted to a stat"),
                ValueSource.Health => StatType.Health,
                ValueSource.Strength => StatType.Strength,
                ValueSource.Defense => StatType.Defense,
                ValueSource.Speed => StatType.Speed,
                ValueSource.Magic => StatType.Magic,
                ValueSource.Will => StatType.Will,
                ValueSource.Movement => StatType.Movement,
                ValueSource.ActionPoints => StatType.ActionPoints,
                ValueSource.Evasion => StatType.Evasion,
                ValueSource.Accuracy => StatType.Accuracy,
                _ => throw new NotImplementedException()
            };
        }
    }
}
