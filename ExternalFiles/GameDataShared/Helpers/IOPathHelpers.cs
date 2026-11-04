// Original Authors - Wyatt Senalik
using System.IO;

namespace EternalWar.Core.Helpers
{
    public static class IOPathHelpers
    {
        public static string GetAbsolutePath(string relativePath)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string absolutePath = Path.Combine(currentDirectory, relativePath);
            return Path.GetFullPath(absolutePath);
        }
    }
}
