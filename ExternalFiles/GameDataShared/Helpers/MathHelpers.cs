// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Helpers
{
    public static class MathHelpers
    {
        public const float SQRT_2 = 1.41421354f;

        /// <summary>
        /// See https://en.wikipedia.org/wiki/Van_der_Corput_sequence.
        /// For generating evenly spaced spectrum values
        /// </summary>
        public static float VanDerCorput(uint n, uint baseNum = 2)
        {
            if (baseNum < 2)
                throw new System.ArgumentOutOfRangeException(nameof(baseNum), "The base must be at least 2.");

            float result = 0;
            float denom = 1;
            while (n > 0)
            {
                denom *= baseNum;
                result += (n % baseNum) / denom;
                n /= baseNum;
            }
            return result;
        }
    }
}
