// Original Authors - Wyatt Senalik
using System.Numerics;

namespace EternalWar.Core.Helpers
{
    public static class Vector2Extensions
    {
        public static Vector2 Normalized(this Vector2 vector)
        {
            float length = vector.Length();
            if (length == 0.0f)
                return Vector2.Zero;
            float invertedLength = 1.0f / length;
            return invertedLength * vector;
        }
    }
}
