// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using System.Security.Cryptography;

namespace EternalWar.Core.Serialization
{
    public record struct GameDataHash(ValueArray<byte> RawBytes)
    {
        public static readonly GameDataHash Empty = new(ValueArray<byte>.Empty);
    }

    public static class GameDataHashing
    {
        public static GameDataHash Hash<T>(T obj)
        {
            byte[] bytes = GameDataSerialization.SerializeToUtf8Bytes(obj);
            byte[] hash = SHA256.HashData(bytes);
            return new GameDataHash(hash);
        }
    }
}