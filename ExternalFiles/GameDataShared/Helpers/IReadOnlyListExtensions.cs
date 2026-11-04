// Original Authors - Wyatt Senalik
using System.Collections.Generic;

namespace EternalWar.Core.Helpers
{
    public static class IReadOnlyListExtensions
    {
        public static int IndexOf<T>(this IReadOnlyList<T> list, T element)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                T currentElement = list[i];
                if (currentElement == null)
                {
                    if (element == null)
                        return i;
                    continue;
                }
                if (currentElement.Equals(element))
                    return i;
            }
            return -1;
        }
    }
}
