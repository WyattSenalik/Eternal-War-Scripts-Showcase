// Original Authors - Wyatt Senalik
using EternalWar.Core.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace EternalWar.Core.Network.Messages
{
    [Serializable]
    public sealed record TypedMessage(string Type, object? Data);

    public static class TypedMessageExtensions
    {
        public static string Serialize(this TypedMessage typedMessage)
        {
            return GameDataSerialization.Serialize(typedMessage);
        }

        public static TypedMessage? DeserializeIntoTypedMessage(this string serializedTypedMessage)
        {
            return GameDataSerialization.Deserialize<TypedMessage>(serializedTypedMessage);
        }

        public static TData ExtractData<TData>(this TypedMessage typedMessage)
        {
            return TryExtractData(typedMessage, out TData? data, out string errorMessage) ? data
                : throw new Exception($"Failed to extract data from message ({typedMessage}).\nError=({errorMessage})");
        }

        public static bool TryExtractData<TData>(this TypedMessage typedMessage, [NotNullWhen(true)] out TData? data, out string errorMessage)
        {
            if (!typedMessage.TryExtractDataAsJson(out JsonElement elementData, out errorMessage))
            {
                data = default;
                return false;
            }
            try
            {
                data = GameDataSerialization.Deserialize<TData>(elementData);
            }
            catch (JsonException exception)
            {
                data = default;
                errorMessage = exception.Message;
                return false;
            }
            if (data == null)
            {
                errorMessage = $"Failed to deserialize {typeof(TData).Name} from Json element ({elementData}).";
                return false;
            }
            return true;
        }

        public static bool TryExtractDataAsString(this TypedMessage typedMessage, [NotNullWhen(true)] out string? data, out string errorMessage)
        {
            if (!typedMessage.TryExtractDataAsJson(out JsonElement elementData, out errorMessage))
            {
                data = default;
                return false;
            }
            if (elementData.ValueKind != JsonValueKind.String)
            {
                data = default;
                errorMessage = $"Expected string data but found {elementData.ValueKind} in {nameof(TypedMessage)} ({typedMessage}).";
                return false;
            }

            data = elementData.GetString();
            if (data == null)
            {
                errorMessage = $"Failed to extract log from JsonEleemnt ({elementData}) for {nameof(TypedMessage)} ({typedMessage})";
                return false;
            }
            return true;
        }

        private static bool TryExtractDataAsJson(this TypedMessage typedMessage, out JsonElement jsonElement, out string errorMessage)
        {
            object? data = typedMessage.Data;
            if (data == null)
            {
                jsonElement = default;
                errorMessage = $"Data in {nameof(TypedMessage)} for {NetworkMessageTypeNames.LOG} was null ({typedMessage})";
                return false;
            }
            try
            {
                jsonElement = (JsonElement)data;
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                jsonElement = default;
                errorMessage = exception.Message;
                return false;
            }
        }
    }
}
