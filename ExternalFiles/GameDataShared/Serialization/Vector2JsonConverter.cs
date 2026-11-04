// Original Authors - Wyatt Senalik
using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Serialization
{
    public sealed class Vector2JsonConverter : JsonConverter<Vector2>
    {
        private const string X_PROPERTY_NAME = "x";
        private const string Y_PROPERTY_NAME = "y";

        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty(X_PROPERTY_NAME, out JsonElement xProperty) ||
                !root.TryGetProperty(Y_PROPERTY_NAME, out JsonElement yProperty) ||
                xProperty.ValueKind != JsonValueKind.Number ||
                yProperty.ValueKind != JsonValueKind.Number ||
                !xProperty.TryGetSingle(out float x) ||
                !yProperty.TryGetSingle(out float y))
            {
                throw new JsonException($"Expected an object with numeric '{X_PROPERTY_NAME}' and '{Y_PROPERTY_NAME}' properties.");
            }

            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber(X_PROPERTY_NAME, value.X);
            writer.WriteNumber(Y_PROPERTY_NAME, value.Y);

            writer.WriteEndObject();
        }
    }
}
