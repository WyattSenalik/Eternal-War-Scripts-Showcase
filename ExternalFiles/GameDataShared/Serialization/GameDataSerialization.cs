// Original Authors - Wyatt Senalik
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Serialization
{
    public static class GameDataSerialization
    {
        private static readonly JsonSerializerOptions Options = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            // Initialize options with glocal settings
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            };

            // Add converters
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new ValueArrayJsonConverterFactory());
            options.Converters.Add(new ValueDictionaryJsonConverterFactory());
            options.Converters.Add(new SortedValueArrayJsonConverterFactory());
            options.Converters.Add(new Vector2JsonConverter());

            return options;
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, Options);
        }

        public static byte[] SerializeToUtf8Bytes<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, Options);
        }

        public static T? Deserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data, Options);
        }

        public static T? Deserialize<T>(JsonElement element)
        {
            return JsonSerializer.Deserialize<T>(element, Options);
        }
    }
}