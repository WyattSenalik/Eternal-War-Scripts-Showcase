// Original Authors - Wyatt Senalik
namespace EternalWar.Server.ConsoleApp
{
    public enum ServerProcessMessageType
    {
        Log,
        Error_InvalidNumberOfArgs,
        Error_InvalidIPAddress,
        Error_InvalidPort,
        Error_InvalidProcessId,
        Error_FailedToCreateEndpoint,
        Error_NullReference,
        Error_Generic,
    }
}
