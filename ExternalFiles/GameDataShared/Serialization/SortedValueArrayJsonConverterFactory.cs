// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Serialization
{
    internal sealed class SortedValueArrayJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(SortedValueArray<,>);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type[] genericArgs = typeToConvert.GetGenericArguments();
            Type keyType = genericArgs[0];
            Type valueType = genericArgs[1];

            Type converterType = typeof(SortedValueArrayConverter<,>).MakeGenericType(keyType, valueType);
            object converter = Activator.CreateInstance(converterType) ?? throw new NullReferenceException($"Failed to create instance of SortedValueArrayConverter"); ;
            return (JsonConverter)converter;
        }
    }

    internal sealed class SortedValueArrayConverter<TKey, TValue> : JsonConverter<SortedValueArray<TKey, TValue>> where TKey : notnull
    {
        public override SortedValueArray<TKey, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            KeyValuePair<TKey, TValue>[]? items = JsonSerializer.Deserialize<KeyValuePair<TKey, TValue>[]>(ref reader, options);
            return items != null ? new SortedValueArray<TKey, TValue>(items) : SortedValueArray<TKey, TValue>.Empty;
        }

        public override void Write(Utf8JsonWriter writer, SortedValueArray<TKey, TValue> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToArray(), options);
        }
    }
}