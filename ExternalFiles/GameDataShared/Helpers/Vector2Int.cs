// Author - Jack Dekko
using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Helpers
{
    [Serializable]
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public static readonly Vector2Int Zero = new(0, 0);
        public static readonly Vector2Int One = new(1, 1);

        [JsonInclude]
        public int X { get; set; }
        [JsonInclude]
        public int Y { get; set; }

        public Vector2Int()
        {
            X = 0;
            Y = 0;
        }

        [JsonConstructor]
        public Vector2Int(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public readonly int LengthSquared()
        {
            return X * X + Y * Y;
        }

        public readonly float Length()
        {
            return MathF.Sqrt(LengthSquared());
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is Vector2Int @int && Equals(@int);
        }

        public readonly bool Equals(Vector2Int other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(Vector2Int left, Vector2Int right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2Int left, Vector2Int right)
        {
            return !(left == right);
        }

        public static Vector2Int operator +(Vector2Int left, Vector2Int right)
        {
            return new Vector2Int(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2Int operator -(Vector2Int left, Vector2Int right)
        {
            return new Vector2Int(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2Int operator /(Vector2Int vector, int divisor)
        {
            return new Vector2Int(vector.X / divisor, vector.Y / divisor);
        }

        public static Vector2Int operator *(Vector2Int vector, int multiplicand)
        {
            return new Vector2Int(vector.X  * multiplicand, vector.Y * multiplicand);
        }

        public static Vector2Int operator *(int multiplicand, Vector2Int vector)
        {
            return new Vector2Int(vector.X * multiplicand, vector.Y * multiplicand);
        }

        public static Vector2 operator *(Vector2Int vector, float multiplicand)
        {
            return new Vector2(vector.X * multiplicand, vector.Y * multiplicand);
        }

        public static Vector2 operator *(float multiplicand, Vector2Int vector)
        {
            return new Vector2(vector.X * multiplicand, vector.Y * multiplicand);
        }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }

    public static class Vector2IntExtensions
    {
        public static Vector2 ToVector2(this Vector2Int vectorInt) => new(vectorInt.X, vectorInt.Y);

        public static Vector2Int RoundToInt(this Vector2 vector)
        {
            int x = (int)MathF.Round(vector.X);
            int y = (int)MathF.Round(vector.Y);
            return new Vector2Int(x, y);
        }
    }
}
