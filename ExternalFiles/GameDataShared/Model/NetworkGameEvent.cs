// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Network;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model
{
    /// <summary>Observer output generated after a command is applied. Events never mutate game state.</summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "eventType")]
    [JsonDerivedType(typeof(NetworkGameEvent_ConnectedClientsChanged), "ConnectedClientsChanged")]
    [JsonDerivedType(typeof(NetworkGameEvent_ClientDisconnected), "ClientDisconnected")]
    [JsonDerivedType(typeof(NetworkGameEvent_Lobby_ReadyToggled), "Lobby_ReadyToggled")]
    [JsonDerivedType(typeof(NetworkGameEvent_Lobby_RunStarted), "Lobby_RunStarted")]
    [JsonDerivedType(typeof(NetworkGameEvent_Run_CombatStarted), "Run_CombatStarted")]
    [JsonDerivedType(typeof(NetworkGameEvent_Run_Combat_Victory), "Run_Combat_Victory")]
    [JsonDerivedType(typeof(NetworkGameEvent_Run_Combat_Defeat), "Run_Combat_Defeat")]
    [JsonDerivedType(typeof(NetworkGameEvent_Run_Map_VotingChanged), "Run_Map_NodeVotingChanged")]
    [JsonDerivedType(typeof(CombatEvent_ActionStarted), "CombatEvent_ActionStarted")]
    [JsonDerivedType(typeof(CombatEvent_ActionEnded), "CombatEvent_ActionEnded")]
    [JsonDerivedType(typeof(CombatEvent_StepStarted), "CombatEvent_StepStarted")]
    [JsonDerivedType(typeof(CombatEvent_StepEnded), "CombatEvent_StepEnded")]
    [JsonDerivedType(typeof(CombatEvent_AttackSwung), "CombatEvent_AttackSwung")]
    [JsonDerivedType(typeof(CombatEvent_AttackMissed), "CombatEvent_AttackMissed")]
    [JsonDerivedType(typeof(CombatEvent_AttackHit), "CombatEvent_AttackHit")]
    [JsonDerivedType(typeof(CombatEvent_UnitDied), "CombatEvent_UnitDied")]
    [JsonDerivedType(typeof(CombatEvent_ActionPointsSpent), "CombatEvent_ActionPointsSpent")]
    [JsonDerivedType(typeof(CombatEvent_Moved), "CombatEvent_Moved")]
    [JsonDerivedType(typeof(CombatEvent_FacingChanged), "CombatEvent_FacingChanged")]
    [JsonDerivedType(typeof(CombatEvent_DamageApplied), "CombatEvent_DamageApplied")]
    [JsonDerivedType(typeof(CombatEvent_HealApplied), "CombatEvent_HealApplied")]
    [JsonDerivedType(typeof(CombatEvent_StatModifierApplied), "CombatEvent_StatModifierApplied")]
    [JsonDerivedType(typeof(CombatEvent_StatModifierRemoved), "CombatEvent_StatModifierRemoved")]
    [JsonDerivedType(typeof(CombatEvent_PassiveAdded), "CombatEvent_PassiveAdded")]
    [JsonDerivedType(typeof(CombatEvent_PassiveRemoved), "CombatEvent_PassiveRemoved")]
    [JsonDerivedType(typeof(CombatEvent_PassiveActivated), "CombatEvent_PassiveActivated")]
    [JsonDerivedType(typeof(CombatEvent_PassiveExpirationProgressed), "CombatEvent_PassiveExpirationProgressed")]
    [JsonDerivedType(typeof(CombatEvent_PassiveStacksReduced), "CombatEvent_PassiveStacksReduced")]
    [JsonDerivedType(typeof(CombatEvent_UnitTurnStarted), "CombatEvent_UnitTurnStarted")]
    [JsonDerivedType(typeof(CombatEvent_UnitTurnEnded), "CombatEvent_UnitTurnEnded")]
    [JsonDerivedType(typeof(CombatEvent_UnitTurnRefreshed), "CombatEvent_UnitTurnRefreshed")]
    [JsonDerivedType(typeof(CombatEvent_NonStackableEffectAdded), "CombatEvent_NonStackableEffectAdded")]
    [JsonDerivedType(typeof(CombatEvent_NonStackableEffectRemoved), "CombatEvent_NonStackableEffectRemoved")]
    [JsonDerivedType(typeof(CombatEvent_Shoved), "CombatEvent_Shoved")]
    public abstract record NetworkGameEvent;

    public abstract record NetworkGameEvent_Lobby : NetworkGameEvent;
    public abstract record NetworkGameEvent_Run : NetworkGameEvent;
    public abstract record NetworkGameEvent_Run_Combat : NetworkGameEvent_Run;
    public abstract record NetworkGameEvent_Run_Map : NetworkGameEvent_Run;

    public sealed record NetworkGameEvent_ConnectedClientsChanged(ValueArray<ClientInfo> NewConnectedClients) : NetworkGameEvent;
    public sealed record NetworkGameEvent_ClientDisconnected(ClientInfo DisconnectedClient) : NetworkGameEvent;
    public sealed record NetworkGameEvent_Lobby_ReadyToggled(ClientInfo ToggleClientInfo) : NetworkGameEvent_Lobby;
    public sealed record NetworkGameEvent_Lobby_RunStarted(ValueArray<CombatUnitUniqueIdentifier> PlayerChosenUnitIdentifiers) : NetworkGameEvent_Lobby;
    public sealed record NetworkGameEvent_Run_CombatStarted(string CombatId, ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> PlayerUnitPlacements) : NetworkGameEvent_Run;
    public sealed record NetworkGameEvent_Run_Combat_Victory : NetworkGameEvent_Run_Combat;
    public sealed record NetworkGameEvent_Run_Combat_Defeat : NetworkGameEvent_Run_Combat;
    public sealed record NetworkGameEvent_Run_Map_VotingChanged(ValueDictionary<ClientInfo, int> Votes) : NetworkGameEvent_Run_Map;
    public sealed record NetworkGameEvent_Run_Map_Created : NetworkGameEvent_Run_Map; // TODO: Add data for it
    public sealed record NetworkGameEvent_Run_Map_Show : NetworkGameEvent_Run_Map;
    public sealed record NetworkGameEvent_Run_Map_Hide : NetworkGameEvent_Run_Map;
}

