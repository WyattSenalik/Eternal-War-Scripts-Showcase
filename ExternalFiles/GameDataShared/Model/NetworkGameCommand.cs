// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Network;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model
{
    /// <summary>A server-approved group of atomic commands produced while resolving one request.</summary>
    [method: JsonConstructor]
    public sealed record DatedNetworkGameCommandBatch(long Sequence, ValueArray<NetworkGameCommand> Commands)
    {
        public DatedNetworkGameCommandBatch(long sequence, NetworkGameCommand command) : this(sequence, new ValueArray<NetworkGameCommand>(command)) { }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "commandType")]
    [JsonDerivedType(typeof(NetworkGameCommand_ConnectedClientsChanged), "ConnectedClientsChanged")]
    [JsonDerivedType(typeof(NetworkGameCommand_ClientDisconnected), "ClientDisconnected")]
    [JsonDerivedType(typeof(NetworkGameCommand_Lobby_ReadyToggled), "Lobby_ReadyToggled")]
    [JsonDerivedType(typeof(NetworkGameCommand_Lobby_RunStarted), "Lobby_RunStarted")]
    [JsonDerivedType(typeof(NetworkGameCommand_Run_CombatStarted), "Run_CombatStarted")]
    [JsonDerivedType(typeof(NetworkGameCommand_Run_Map_VoteForNode), "Run_Map_VoteForNode")]
    [JsonDerivedType(typeof(CombatCommand_BeginAction), "CombatCommand_BeginAction")]
    [JsonDerivedType(typeof(CombatCommand_CompleteAction), "CombatCommand_CompleteAction")]
    [JsonDerivedType(typeof(CombatCommand_BeginActionStep), "CombatCommand_BeginActionStep")]
    [JsonDerivedType(typeof(CombatCommand_CompleteActionStep), "CombatCommand_CompleteActionStep")]
    [JsonDerivedType(typeof(CombatCommand_PerformAttack), "CombatCommand_PerformAttack")]
    [JsonDerivedType(typeof(CombatCommand_ResolveAttackMiss), "CombatCommand_ResolveAttackMiss")]
    [JsonDerivedType(typeof(CombatCommand_ResolveAttackHit), "CombatCommand_ResolveAttackHit")]
    [JsonDerivedType(typeof(CombatCommand_NotifyUnitDied), "CombatCommand_NotifyUnitDied")]
    [JsonDerivedType(typeof(CombatCommand_MoveUnit), "CombatCommand_MoveUnit")]
    [JsonDerivedType(typeof(CombatCommand_ChangeFacing), "CombatCommand_ChangeFacing")]
    [JsonDerivedType(typeof(CombatCommand_ApplyHealing), "CombatCommand_ApplyHealing")]
    [JsonDerivedType(typeof(CombatCommand_ApplyStatModifier), "CombatCommand_ApplyStatModifier")]
    [JsonDerivedType(typeof(CombatCommand_RemoveStatModifier), "CombatCommand_RemoveStatModifier")]
    [JsonDerivedType(typeof(CombatCommand_AddPassive), "CombatCommand_AddPassive")]
    [JsonDerivedType(typeof(CombatCommand_ActivatePassive), "CombatCommand_ActivatePassive")]
    [JsonDerivedType(typeof(CombatCommand_ProgressPassiveExpiration), "CombatCommand_ProgressPassiveExpiration")]
    [JsonDerivedType(typeof(CombatCommand_ReducePassiveStacks), "CombatCommand_ReducePassiveStacks")]
    [JsonDerivedType(typeof(CombatCommand_StartUnitTurn), "CombatCommand_StartUnitTurn")]
    [JsonDerivedType(typeof(CombatCommand_EndUnitTurn), "CombatCommand_EndUnitTurn")]
    [JsonDerivedType(typeof(CombatCommand_RefreshUnitTurn), "CombatCommand_RefreshUnitTurn")]
    [JsonDerivedType(typeof(CombatCommand_AddNonStackableEffect), "CombatCommand_AddNonStackableEffect")]
    [JsonDerivedType(typeof(CombatCommand_RemoveNonStackableEffect), "CombatCommand_RemoveNonStackableEffect")]
    [JsonDerivedType(typeof(CombatCommand_PerformShove), "CombatCommand_PerformShove")]
    public abstract record NetworkGameCommand;

    public abstract record NetworkGameCommand_Lobby : NetworkGameCommand;
    public abstract record NetworkGameCommand_Run : NetworkGameCommand;
    public abstract record NetworkGameCommand_Run_Combat : NetworkGameCommand_Run;
    public abstract record NetworkGameCommand_Run_Map : NetworkGameCommand_Run;

    public sealed record NetworkGameCommand_ConnectedClientsChanged(ValueArray<ClientInfo> NewConnectedClients) : NetworkGameCommand;
    public sealed record NetworkGameCommand_ClientDisconnected(ClientInfo DisconnectedClient) : NetworkGameCommand;
    public sealed record NetworkGameCommand_Lobby_ReadyToggled(ClientInfo ToggleClientInfo) : NetworkGameCommand_Lobby;
    public sealed record NetworkGameCommand_Lobby_RunStarted(ValueArray<CombatUnitUniqueIdentifier> PlayerChosenUnitIdentifiers, ulong RandomnessSeed) : NetworkGameCommand_Lobby;
    public sealed record NetworkGameCommand_Run_CombatStarted(string CombatId, ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> PlayerUnitPlacements) : NetworkGameCommand_Run;
    public sealed record NetworkGameCommand_Run_Map_VoteForNode(ClientInfo VotingClientInfo, int NodeIndex) : NetworkGameCommand_Run_Map;
}

