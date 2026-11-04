// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Map.Definitions;
using EternalWar.Core.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;

namespace EternalWar.Core.Model
{
    public static class DatabaseLoader
    {
        public static GameDatabase LoadDatabase(string rootDataFolderPath, bool sanityCheck = false)
        {
            ValueDictionary<string, CombatDefinition> combats = LoadFolder<CombatDefinition>(rootDataFolderPath, DatabaseFolderNames.COMBATS_FOLDER_NAME, sanityCheck);
            ValueDictionary<string, UnitDefinition> units = LoadFolder<UnitDefinition>(rootDataFolderPath, DatabaseFolderNames.UNITS_FOLDER_NAME, sanityCheck);
            ValueDictionary<string, CombatActionSequenceDefinition> actionSequences = LoadFolder<CombatActionSequenceDefinition>(rootDataFolderPath, DatabaseFolderNames.ACTION_SEQUENCES_FOLDER_NAME, sanityCheck);
            ValueDictionary<string, PassiveDefinition> passives = LoadFolder<PassiveDefinition>(rootDataFolderPath, DatabaseFolderNames.PASSIVES_FOLDER_NAME, sanityCheck);
            ValueDictionary<string, NonStackableEffectDefinition> nonStackableEffects = LoadFolder<NonStackableEffectDefinition>(rootDataFolderPath, DatabaseFolderNames.NON_STACKABLE_EFFECTS_FOLDER_NAME, sanityCheck);
            ValueDictionary<string, ArtifactDefinition> artifacts = LoadFolder<ArtifactDefinition>(rootDataFolderPath, DatabaseFolderNames.ARTIFACTS_FOLDER_NAME, sanityCheck);
            ValueDictionary<string, BlockDefinition> blocks = LoadFolder<BlockDefinition>(rootDataFolderPath, DatabaseFolderNames.BLOCKS_FOLDER_NAME, sanityCheck);
            ValueDictionary<string, FakeLocationMapDefinition> fakeLocationMaps = LoadFolder<FakeLocationMapDefinition>(rootDataFolderPath, DatabaseFolderNames.FAKE_LOCATION_MAPS_FOLDER_NAME, sanityCheck);
            ValueDictionary<string, LocationNodeDefinition> locationNodes = LoadFolder<LocationNodeDefinition>(rootDataFolderPath, DatabaseFolderNames.LOCATION_NODE_FOLDER_NAME, sanityCheck);

            return new GameDatabase(combats, units, actionSequences, passives, nonStackableEffects, artifacts, blocks, fakeLocationMaps, locationNodes);
        }

        public static ValueDictionary<string, T> LoadFolder<T>(string rootDataFolderPath, string folderName, bool sanityCheck = false) where T : IHaveStringId
        {
            try
            {
                string folderPath = $"{rootDataFolderPath}/{folderName}";
                if (!Directory.Exists(folderPath))
                {
                    Debug.PrintError($"Failed to find folder: ({folderName}) in root folder: ({rootDataFolderPath})");
                    return ValueDictionary<string, T>.Empty;
                }

                return Load<T>(folderPath, sanityCheck);
            }
            catch (Exception e)
            {
                Debug.PrintError($"Failed to load folder:\n{e}");
                return ValueDictionary<string, T>.Empty;
            }
        }

        private static ValueDictionary<string, T> Load<T>(string folderPath, bool sanityCheck) where T : IHaveStringId
        {
            using ValueDictionary<string, T>.Builder builder = ValueDictionary<string, T>.CreateBuilder();
            foreach (string filePath in Directory.EnumerateFiles(folderPath))
            {
                try
                {
                    string text = File.ReadAllText(filePath);
                    T? data = GameDataSerialization.Deserialize<T>(text);
                    if (data == null)
                    {
                        Debug.PrintError($"Failed to deserialize text into type ({nameof(T)}). Text:\n{text}\n");
                        continue;
                    }
                    builder.Add(data.Id, data);

                    if (sanityCheck)
                    {
                        string reserialized = GameDataSerialization.Serialize(data);

                        JsonNode? originalJson = JsonNode.Parse(text);
                        if (originalJson == null)
                        {
                            Debug.PrintError($"Failed to parse text into json:\n{text}");
                            continue;
                        }
                        JsonNode reserializedJson = JsonNode.Parse(reserialized)!;

                        StringBuilder errors = new();
                        ContainsOriginalJson(originalJson, reserializedJson, "$", errors, typeof(T));
                        if (errors.Length > 0)
                        {
                            Debug.PrintError($"Reserialization of data did not preserve all source data in {filePath}\n{errors}");
                            continue;
                        }

                        try
                        {
                            ValidateForNulls(data, filePath);
                        }
                        catch (Exception e)
                        {
                            List<(int LineNumber, string Line)> linesContainingNull = FindLinesContaining(reserialized, "null");
                            Debug.PrintError($"Found null in the Database:\nIt might be one of these (line numbers may differ from original slightly):\n{string.Join("\n", linesContainingNull.Select(pair => $"[{pair.LineNumber}] {pair.Line}"))}\n{e}");
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.PrintError($"Failed to load file ({filePath})\n{e}");
                }
            }
            return builder.ToValueDictionary();
        }

        private static bool ContainsOriginalJson(JsonNode original, JsonNode reserialized, string path, StringBuilder errors, Type expectedType)
        {
            if (original is JsonObject originalObject && reserialized is JsonObject reserializedObject)
            {
                foreach ((string propertyName, JsonNode? originalValue) in originalObject)
                {
                    if (!reserializedObject.TryGetPropertyValue(propertyName, out JsonNode? reserializedValue))
                    {
                        errors.AppendLine(CultureInfo.CurrentCulture, $"Missing property at {path}.{propertyName}");
                        continue;
                    }

                    if (originalValue == null)
                    {
                        if (reserializedValue != null)
                        {
                            errors.AppendLine(CultureInfo.CurrentCulture, $"Property {path}.{propertyName} changed from null");
                        }
                        continue;
                    }

                    if (reserializedValue == null)
                    {
                        errors.AppendLine(CultureInfo.CurrentCulture, $"Property {path}.{propertyName} became null");
                        continue;
                    }

                    Type propertyType = GetPropertyType(expectedType, propertyName) ?? typeof(object);

                    ContainsOriginalJson(originalValue, reserializedValue, $"{path}.{propertyName}", errors, propertyType);
                }

                return errors.Length == 0;
            }

            if (original is JsonArray originalArray && reserialized is JsonArray reserializedArray)
            {
                if (originalArray.Count != reserializedArray.Count)
                {
                    errors.AppendLine(CultureInfo.CurrentCulture, $"Array {path} changed size: {originalArray.Count} -> {reserializedArray.Count}");
                    return false;
                }

                Type elementType = GetElementType(expectedType) ?? typeof(object);

                for (int i = 0; i < originalArray.Count; i++)
                {
                    JsonNode? originalValue = originalArray[i];
                    JsonNode? reserializedValue = reserializedArray[i];

                    if (originalValue == null)
                        continue;

                    if (reserializedValue == null)
                    {
                        errors.AppendLine(CultureInfo.CurrentCulture, $"Array element {path}[{i}] became null");
                        continue;
                    }

                    ContainsOriginalJson(originalValue, reserializedValue, $"{path}[{i}]", errors, elementType);
                }

                return errors.Length == 0;
            }

            // TODO: Add the enum check here before the deep equals.
            if (expectedType.IsEnum)
            {
                if (TryGetEnumValue(original, expectedType, out object? originalValue) &&
                    TryGetEnumValue(reserialized, expectedType, out object? reserializedValue) &&
                    Equals(originalValue, reserializedValue))
                {
                    return errors.Length == 0;
                }
            }

            if (!JsonNode.DeepEquals(original, reserialized))
            {
                errors.AppendLine(CultureInfo.CurrentCulture, $"Value changed at {path}:\n  Original:    {original}\n  Reserialized: {reserialized}");
            }

            return errors.Length == 0;
        }

        private static void ValidateForNulls(object? obj, string filePath)
        {
            ValidateForNulls(obj, "$", new HashSet<object>(ReferenceEqualityComparer.Instance), filePath);
        }

        private static void ValidateForNulls(object? obj, string path, HashSet<object> visited, string filePath)
        {
            if (obj == null)
                return;

            Type type = obj.GetType();
            if (!type.IsValueType && !visited.Add(obj))
                return;

            if (IsLeafType(type))
                return;

            if (obj is IEnumerable enumerable && obj is not string)
            {
                int index = 0;

                foreach (object? item in enumerable)
                {
                    if (item == null)
                    {
                        throw new InvalidOperationException($"Null collection element at {path}[{index}] in file ({filePath}).");
                    }

                    ValidateForNulls(item, $"{path}[{index}]", visited, filePath);

                    index++;
                }

                return;
            }

            NullabilityInfoContext nullabilityContext = new();
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead ||
                    property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                object? value = property.GetValue(obj);

                NullabilityInfo nullability = nullabilityContext.Create(property);

                if (value == null && nullability.ReadState == NullabilityState.NotNull)
                {
                    throw new InvalidOperationException($"Non-nullable property is null at {path}.{property.Name} ({type.Name}) in file ({filePath}).");
                }

                if (value != null)
                {
                    ValidateForNulls(value, $"{path}.{property.Name}", visited, filePath);
                }
            }
        }

        private static bool IsLeafType(Type type)
        {
            return type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid);
        }

        private static List<(int LineNumber, string Line)> FindLinesContaining(string text, string searchString)
        {
            return text
                .Split(Environment.NewLine)
                .Select((line, index) => (LineNumber: index + 1, Line: line))
                .Where(x => x.Line.Contains(searchString))
                .ToList();
        }

        private static Type? GetPropertyType(Type type, string propertyName)
        {
            return type.GetProperty(propertyName)?.PropertyType;
        }

        private static Type? GetElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(List<>) ||
                    genericType == typeof(IReadOnlyList<>) ||
                    genericType == typeof(IEnumerable<>))
                {
                    return type.GetGenericArguments()[0];
                }
            }

            Type? enumerableInterface = type
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableInterface?.GetGenericArguments()[0];
        }

        private static bool TryGetEnumValue(JsonNode node, Type enumType, out object? value)
        {
            value = null;

            if (node is not JsonValue jsonValue)
                return false;

            if (jsonValue.TryGetValue(out string? stringValue))
                return Enum.TryParse(enumType, stringValue, ignoreCase: true, out value);

            if (jsonValue.TryGetValue(out int intValue))
            {
                value = Enum.ToObject(enumType, intValue);
                return true;
            }

            return false;
        }
    }
}
