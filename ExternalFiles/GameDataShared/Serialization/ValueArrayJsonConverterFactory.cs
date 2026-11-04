// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Serialization
{
    internal sealed class ValueArrayJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ValueArray<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type itemType = typeToConvert.GetGenericArguments()[0];

            Type converterType = typeof(ValueArrayJsonConverter<>).MakeGenericType(itemType);
            object converter = Activator.CreateInstance(converterType) ?? throw new NullReferenceException($"Failed to create instance of ValueArrayJsonConverter");
            return (JsonConverter)converter;
        }
    }

    internal sealed class ValueArrayJsonConverter<T> : JsonConverter<ValueArray<T>>
    {
        public override ValueArray<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            T[]? items = JsonSerializer.Deserialize<T[]>(ref reader, options);
            return items != null ? new ValueArray<T>(items) : ValueArray<T>.Empty;
        }

        public override void Write(Utf8JsonWriter writer, ValueArray<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToArray(), options);
        }
    }
}