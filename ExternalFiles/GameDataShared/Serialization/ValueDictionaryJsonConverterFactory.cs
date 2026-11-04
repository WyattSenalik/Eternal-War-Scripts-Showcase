// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Serialization
{
    public sealed class ValueDictionaryJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ValueDictionary<,>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type[] genericArguments = typeToConvert.GetGenericArguments();

            Type converterType = typeof(ValueDictionaryJsonConverter<,>).MakeGenericType(genericArguments);
            object? converter = Activator.CreateInstance(converterType);
            if (converter == null)
                throw new NullReferenceException($"Failed to create instance of ValueArrayJsonConverter");

            return (JsonConverter)converter;
        }
    }

    public sealed class ValueDictionaryJsonConverter<TKey, TValue> : JsonConverter<ValueDictionary<TKey, TValue>> where TKey : notnull
    {
        public override ValueDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            KeyValuePair<TKey, TValue>[]? items = JsonSerializer.Deserialize<KeyValuePair<TKey, TValue>[]>(ref reader, options);

            return items != null ? new ValueDictionary<TKey, TValue>(items) : ValueDictionary<TKey, TValue>.Empty;
        }

        public override void Write(Utf8JsonWriter writer, ValueDictionary<TKey, TValue> value, JsonSerializerOptions options)
        {
            IOrderedEnumerable<KeyValuePair<TKey, TValue>> ordered = value.OrderBy(kvp => kvp.Key.ToString(), StringComparer.Ordinal);

            JsonSerializer.Serialize<IEnumerable<KeyValuePair<TKey, TValue>>>(writer, ordered, options);
        }
    }
}