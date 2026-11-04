// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Server.Application;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CustomDebug = EternalWar.Core.Helpers.Debug;

namespace EternalWar.Server.ConsoleApp
{
    internal sealed class Program
    {
        private const string DATA_FOLDER_RELATIVE_PATH = "../Data";

        /**
         * Regex used to verify validity of IP addresses.
         */
        private const string IP_ADDRESS_REGEX = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

        private const int MAX_CLIENT_CONNECTIONS = 4;

        private static ServerGameApplication? _application;
        private static Process? _gameProcess;
        private static bool _hasCleanedUp;

        /**
         * Entry point for the program.
         */
        internal async static Task Main(string[] args)
        {
            ServerProcessOutput.ClearFile();

            CustomDebug.Printed += OnGameDataSharedDebugPrinted;
            CustomDebug.PrintedError += OnGameDataSharedDebugPrintedError;

            // Validate the IP address arguments
            int argsLength = args.Length;
            if (argsLength != 2 && argsLength != 3)
            {
                ServerProcessOutput.Output(ServerProcessMessageType.Error_InvalidNumberOfArgs, "Invalid number of arguments. Expected: IP Address, Port, and game process id (optional).");
                return;
            }
            bool hasGameProcessId = argsLength > 2;

            string ipAddressString = args[0];
            string portString = args[1];
            string gameProcessIdString = hasGameProcessId ? args[2] : string.Empty;
            ServerProcessOutput.Output(ServerProcessMessageType.Log, $"IP Address: {ipAddressString}");
            ServerProcessOutput.Output(ServerProcessMessageType.Log, $"Port: {portString}");
            ServerProcessOutput.Output(ServerProcessMessageType.Log, $"Game Process ID: {gameProcessIdString}");

            if (!Regex.IsMatch(ipAddressString, IP_ADDRESS_REGEX) || !System.Net.IPAddress.TryParse(ipAddressString, out _))
            {
                ServerProcessOutput.Output(ServerProcessMessageType.Error_InvalidIPAddress, "Argument 1 is not a valid ip address.");
                return;
            }

            if (!ushort.TryParse(portString, out ushort port) || port == 0)
            {
                ServerProcessOutput.Output(ServerProcessMessageType.Error_InvalidPort, "Argument 2 is not a valid port.");
                return;
            }

            if (!int.TryParse(gameProcessIdString, out int gameProcessId))
            {
                if (hasGameProcessId)
                {
                    ServerProcessOutput.Output(ServerProcessMessageType.Error_InvalidProcessId, "Argument 3 is not a process id.");
                    return;
                }
            }

            try
            {
                if (hasGameProcessId)
                {
                    _gameProcess = Process.GetProcessById(gameProcessId);
                }

                string dataAbsolutePath = IOPathHelpers.GetAbsolutePath(DATA_FOLDER_RELATIVE_PATH);
                _application = new ServerGameApplication(dataAbsolutePath, port, MAX_CLIENT_CONNECTIONS);
                await _application.RunAsync(OnApplicationTicked);
                Cleanup();
                return;
            }
            catch (Exception e)
            {
                ServerProcessOutput.Output(ServerProcessMessageType.Error_Generic, e.ToString());
                Cleanup();
                return;
            }
        }

        private static void OnApplicationTicked()
        {
            // If there is no game process driving the server, leave it open until manually closed
            if (_gameProcess == null)
                return;

            // Cleanup if the game has closed
            if ( !_gameProcess.HasExited)
                return;

            Cleanup();
        }

        private static void Cleanup()
        {
            if (_hasCleanedUp)
                return;
            _hasCleanedUp = true;

            _gameProcess?.Dispose();
            _application?.Stop();
            _application?.Dispose();

            CustomDebug.Printed -= OnGameDataSharedDebugPrinted;
            CustomDebug.PrintedError -= OnGameDataSharedDebugPrintedError;
        }

        private static void OnGameDataSharedDebugPrinted(string message)
        {
            ServerProcessOutput.Output(ServerProcessMessageType.Log, message);
        }

        private static void OnGameDataSharedDebugPrintedError(string message)
        {
            ServerProcessOutput.Output(ServerProcessMessageType.Error_Generic, message);
        }
    }
}
