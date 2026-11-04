// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;

namespace EternalWar.Client.Application.Model
{
    internal abstract record ClientGameStateChange(ClientGameState CurrentClientGameState, ClientGameState AfterApplyClientGameState);

    internal sealed record ClientGameStateChange_NetworkGameEvent(NetworkGameEvent Event, ClientGameState CurrentClientGameState, ClientGameState AfterApplyClientGameState) : ClientGameStateChange(CurrentClientGameState, AfterApplyClientGameState);

    internal sealed record ClientGameStateChange_NetworkGameStateReset(ClientGameState CurrentClientGameState, ClientGameState AfterApplyClientGameState) : ClientGameStateChange(CurrentClientGameState, AfterApplyClientGameState);

    internal sealed record ClientGameStateChange_ConnectionConnecting(ClientGameState CurrentClientGameState, ClientGameState AfterApplyClientGameState) : ClientGameStateChange(CurrentClientGameState, AfterApplyClientGameState);

    internal sealed record ClientGameStateChange_ConnectionConnected(ClientGameState CurrentClientGameState, ClientGameState AfterApplyClientGameState) : ClientGameStateChange(CurrentClientGameState, AfterApplyClientGameState);

    internal sealed record ClientGameStateChange_ConnectionDisconnected(ClientGameState CurrentClientGameState, ClientGameState AfterApplyClientGameState) : ClientGameStateChange(CurrentClientGameState, AfterApplyClientGameState);
}
