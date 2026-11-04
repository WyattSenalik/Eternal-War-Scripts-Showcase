// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Network;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model
{
    /// <summary>A high-level client request. The server resolves an approved request into atomic commands.</summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "requestType")]
    [JsonDerivedType(typeof(NetworkGameCommandRequest_Lobby_ToggleReady), "Lobby_ToggleReady")]
    [JsonDerivedType(typeof(NetworkGameCommandRequest_Run_Combat_EndTurn), "Run_Combat_EndTurn")]
    [JsonDerivedType(typeof(NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence), "Run_Combat_ExecuteActionSequence")]
    [JsonDerivedType(typeof(NetworkGameCommandRequest_Run_Map_VoteForNode), "Run_Map_VoteForNode")]
    public abstract record NetworkGameCommandRequest;

    public abstract record NetworkGameCommandRequest_Lobby : NetworkGameCommandRequest;
    public abstract record NetworkGameCommandRequest_Run : NetworkGameCommandRequest;
    public abstract record NetworkGameCommandRequest_Run_Combat : NetworkGameCommandRequest_Run;
    public abstract record NetworkGameCommandRequest_Run_Map : NetworkGameCommandRequest_Run;

    public sealed record NetworkGameCommandRequest_Lobby_ToggleReady(ClientInfo ToggleClientInfo) : NetworkGameCommandRequest_Lobby;

    public sealed record NetworkGameCommandRequest_Run_Combat_EndTurn : NetworkGameCommandRequest_Run_Combat
    {
        public static readonly NetworkGameCommandRequest_Run_Combat_EndTurn Instance = new();
    }

    public sealed record NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence(ActionExecutionContext ActionExecutionContext) : NetworkGameCommandRequest_Run_Combat;

    public sealed record NetworkGameCommandRequest_Run_Map_VoteForNode(ClientInfo VotingClientInfo, int NodeIndex) : NetworkGameCommandRequest_Run_Map;
}
