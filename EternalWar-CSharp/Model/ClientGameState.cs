// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using System.Text.Json.Serialization;

namespace EternalWar.Client.Application.Model
{
    public sealed record ClientGameState(ClientConnectionState ConnectionState, NetworkGameState NetworkGameState);

    [JsonPolymorphic]
    [JsonDerivedType(typeof(ClientConnectionState_Connected), "Connected")]
    [JsonDerivedType(typeof(ClientConnectionState_Connecting), "Connecting")]
    [JsonDerivedType(typeof(ClientConnectionState_Disconnected), "Disconnected")]
    public abstract record ClientConnectionState;

    public sealed record ClientConnectionState_Connected(ClientInfo MyClientInfo) : ClientConnectionState;
    public sealed record ClientConnectionState_Connecting : ClientConnectionState;
    public sealed record ClientConnectionState_Disconnected : ClientConnectionState;
}
