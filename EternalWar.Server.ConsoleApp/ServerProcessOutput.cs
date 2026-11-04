// Original Authors - Wyatt Senalik
using System;
using System.IO;

namespace EternalWar.Server.ConsoleApp
{
    internal static class ServerProcessOutput
    {
        private const string LOG_FILE_NAME = "log.txt";
        private static readonly string _logFilePath = Path.Combine(AppContext.BaseDirectory, LOG_FILE_NAME);

        public static void Output(ServerProcessMessageType messageType, string message)
        {
            string processedMessage = $"{messageType}\t{message}";
            Console.WriteLine(processedMessage);

            string datedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{processedMessage}";
            try
            {
                File.AppendAllText(_logFilePath, datedMessage + Environment.NewLine);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to write to log file: ({exception})");
            }
        }

        public static void ClearFile()
        {
            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);
        }
    }
}
