// Original Authors - Wyatt Senalik
using System.Collections.Generic;
using System;

namespace EternalWar.Core.Model.Random
{
    public sealed record HitRollResult(bool DidHit, bool DidCrit);

    public interface IRandomResolutionResolver
    {
        HitRollResult RollHitChance(int chance);
        int RollRange(int exclusiveMax);
    }

    public static class RandomResolutionResolverExtensions
    {
        public static T[] GenerateRandomEnumList<T>(this IRandomResolutionResolver resolver, int length, bool allowRepeating = false) where T : Enum
        {
            T[] options = (T[])Enum.GetValues(typeof(T));
            return resolver.SelectRandomFromList(length, options, allowRepeating);
        }

        public static T[] SelectRandomFromList<T>(this IRandomResolutionResolver resolver, int amountToSelect, IReadOnlyList<T> options, bool allowRepeating = false)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentOutOfRangeException.ThrowIfNegative(amountToSelect);

            int optionsLength = options.Count;
            if (optionsLength == 0)
                return Array.Empty<T>();

            T[] resultList = new T[amountToSelect];

            if (allowRepeating)
            {
                for (int i = 0; i < amountToSelect; ++i)
                {
                    resultList[i] = options[resolver.RollRange(optionsLength)];
                }
                return resultList;
            }

            List<T> optionsCopy = new(options);
            for (int i = 0; i < amountToSelect; ++i)
            {
                int optionsCopyLength = optionsCopy.Count;
                if (optionsCopyLength <= 0)
                {
                    Array.Resize(ref resultList, i);
                    break;
                }
                int randomOptionIndex = resolver.RollRange(optionsCopyLength);
                resultList[i] = optionsCopy[randomOptionIndex];
                optionsCopy.RemoveAt(randomOptionIndex);
            }
            return resultList;
        }
    }
}
