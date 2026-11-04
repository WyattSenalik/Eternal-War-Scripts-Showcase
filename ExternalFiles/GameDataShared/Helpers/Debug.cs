// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Helpers
{
    public static class Debug
    {
        public static event Action<string>? Printed;
        public static event Action<string>? PrintedError;

        public static void Print(string msg)
        {
            Printed?.Invoke(msg);
        }

        public static void PrintError(string msg)
        {
            PrintedError?.Invoke(msg);
        }
    }
}
