// Original Authors - Wyatt Senalik
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Core.Helpers
{
    public sealed record StringKeyArguments(ValueDictionary<string, object> Arguments)
    {
        public object GetArgument(string key) => Arguments[key];

        public bool TryGetArgument(string key, out object argument) => Arguments.TryGetValue(key, out argument);

        public T GetArgument<T>(string key) => (T)GetArgument(key);

        public bool TryGetArgument<T>(string key, [NotNullWhen(true)] out T? argument)
        {
            if (!TryGetArgument(key, out object uncastedArgument))
            {
                argument = default;
                return false;
            }
            argument = (T)uncastedArgument;
            return true;
        }

        public static StringKeyArguments Create(params (string, object)[] keyedArguments)
        {
            using ValueDictionary<string, object>.Builder builder = ValueDictionary<string, object>.CreateBuilder();
            foreach ((string key, object value) in keyedArguments)
            {
                builder.Add(key, value);
            }
            return new StringKeyArguments(builder.ToValueDictionary());
        }
    }

    public static class StringKeyArgumentsExtensions
    {
        public static StringKeyArguments AddArguments(this StringKeyArguments arguments, params (string, object)[] additionalArguments)
        {
            using ValueDictionary<string, object>.Builder builder = ValueDictionary<string, object>.CreateBuilder(arguments.Arguments);
            foreach ((string key, object value) in additionalArguments)
            {
                builder.Add(key, value);
            }
            return new StringKeyArguments(builder.ToValueDictionary());
        }
    }
}
