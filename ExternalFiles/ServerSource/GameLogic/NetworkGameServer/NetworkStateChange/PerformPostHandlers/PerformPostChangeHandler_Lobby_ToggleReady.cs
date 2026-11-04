// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Network;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace EternalWar.Server.Application.NetworkStateChange.PerformPostHandlers
{
    internal sealed class PerformPostCommandRequestHandler_Lobby_ToggleReady(ServerGameSession session, GameDatabase database) : PerformPostCommandRequestHandler<NetworkGameCommandRequest_Lobby_ToggleReady>
    {
        private const string TEST_SCENARIO_ID = "Forest_Test"; // TODO: DELETE
        private static readonly string[] TEST_PLAYER_UNIT_IDS = new string[4] {
            "Apothecary",
            "Warrior",
            "Barbarian",
            "Soldier"
        }; // TODO: DELETE

        private readonly ServerGameSession _session = session;
        private readonly GameDatabase _database = database;

        protected override void PerformPostRequest(NetworkGameCommandRequest_Lobby_ToggleReady request)
        {
            // Check if everyone is ready, if not do nothing more.
            NetworkGameState newNetworkGameState = _session.NetworkGameState;
            LobbyState newLobbyState = newNetworkGameState.GetLobbyState();
            int readyClientCount = newLobbyState.GetReadyClientCount();
            int totalClientCount = newNetworkGameState.ConnectedClients.Count;
            bool isEveryoneReady = readyClientCount >= totalClientCount;
            if (!isEveryoneReady)
                return;

            // Everyone is ready, start the run.
            ValueArray<CombatUnitUniqueIdentifier> playerChosenUnitIdentifiers = CreatePlayerUnitIdentifiers(newNetworkGameState, TEST_PLAYER_UNIT_IDS); // TODO: Don't use test player unit ids, get them to choose their starting class.

            // Start the first combat.
            string combatId = TEST_SCENARIO_ID; // TODO: Use a real scenario
            ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> playerUnitPlacements = CreateFakePlayerUnitPlacements(_database, combatId, playerChosenUnitIdentifiers); // TODO: Have players place their units before starting the first combat
            // TODO: Allow the host to set a seed through a request.
            ulong seed = GenerateRandomSeed();
            NetworkGameCommand_Lobby_RunStarted startRunCommand = new(playerChosenUnitIdentifiers, seed);
            NetworkGameCommand_Run_CombatStarted startCombatCommand = new(combatId, playerUnitPlacements);
            ValueArray<NetworkGameCommand> commands = new(new NetworkGameCommand[] { startRunCommand, startCombatCommand });
            _session.ApplyCommandsAndRelay(commands, clientRequestedChangeId: null);
        }

        private static ValueArray<CombatUnitUniqueIdentifier> CreatePlayerUnitIdentifiers(NetworkGameState networkGameState, IEnumerable<string> playerUnitDefinitionIds)
        {
            ValueArray<ClientInfo> connectedClients = networkGameState.ConnectedClients;

            using ValueArray<CombatUnitUniqueIdentifier>.Builder unitIdentifiersBuilder = new();
            using PooledList<ClientInfo> clientIds = new(connectedClients);
            clientIds.Sort((a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
            foreach (string unitId in playerUnitDefinitionIds)
            {
                // TODO: Add something that can assign an owner based session save data
                if (clientIds.Count <= 0)
                {
                    clientIds.AddRange(connectedClients);
                    clientIds.Sort((a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
                }
                CombatUnitOwner owner = new CombatUnitOwner_Player(clientIds[0]);
                clientIds.RemoveAt(0);
                int existingUnitCountWithSameIdentifier = unitIdentifiersBuilder.Count(identifier => identifier.Owner == owner && identifier.DefinitionId == unitId);
                CombatUnitUniqueIdentifier unitIdentifier = new(owner, unitId, existingUnitCountWithSameIdentifier);
                unitIdentifiersBuilder.Add(unitIdentifier);
            }

            return unitIdentifiersBuilder.ToValueArray();
        }

        private static ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo> CreateFakePlayerUnitPlacements(GameDatabase database, string combatId, ValueArray<CombatUnitUniqueIdentifier> playerUnitIdentifiers)
        {
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo>.Builder unitPlacementMapBuilder = ValueDictionary<CombatUnitUniqueIdentifier, UnitPlacementOptionInfo>.CreateBuilder();

            CombatDefinition combatDefinition = database.GetCombatDefinition(combatId);
            ValueArray<UnitPlacementOptionInfo> playerUnitPlacementOptions = combatDefinition.PlayerUnitPlacementOptions;
            if (playerUnitPlacementOptions.Count < playerUnitIdentifiers.Count)
            {
                throw new Exception($"Scenario has too few placement options. Has {playerUnitPlacementOptions.Count} but there are {playerUnitIdentifiers.Count} player characters. Scenario Id: ({combatId})");
            }
            if (playerUnitIdentifiers.Count <= 0)
            {
                throw new Exception("Tried to create fake unit placements with no player characters.");
            }

            // Actually place them
            for (int i = 0; i < playerUnitIdentifiers.Count; ++i)
            {
                CombatUnitUniqueIdentifier unitIdentifier = playerUnitIdentifiers[i];
                UnitPlacementOptionInfo placementOption = playerUnitPlacementOptions[i];

                unitPlacementMapBuilder.Add(unitIdentifier, placementOption);
            }

            return unitPlacementMapBuilder.ToValueDictionary();
        }

        private static ulong GenerateRandomSeed()
        {
            Span<byte> bytes = stackalloc byte[sizeof(ulong)];
            RandomNumberGenerator.Fill(bytes);
            return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
        }
    }
}
