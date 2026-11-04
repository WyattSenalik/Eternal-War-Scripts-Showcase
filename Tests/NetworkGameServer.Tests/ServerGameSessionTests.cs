// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Network;
using EternalWar.Core.Network.Messages;
using EternalWar.Core.Network.Messages.HighLevelGame;
using EternalWar.Server.Application;
using EternalWar.Server.Application.Messages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace NetworkGameServer.Tests
{
    [TestFixture]
    internal sealed class ServerGameSessionTests
    {
        private const string DATA_FOLDER_PATH = "../../../../../Data";
        private static readonly ClientInfo _clientA = new(Guid.Parse("50000000-0000-0000-0000-000000000001"), 0);
        private static readonly ClientInfo _clientB = new(Guid.Parse("50000000-0000-0000-0000-000000000002"), 1);

        [Test]
        public void ApplyCommandAdvancesSequenceWithoutRelayingAndApplyRequestAndRelayBroadcasts()
        {
            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
            RecordingMessageSender sender = new();
            ServerGameSession session = new(sender, database);
            session.ApplyCommand(new NetworkGameCommand_ConnectedClientsChanged(new ValueArray<ClientInfo>(_clientA, _clientB)));

            DatedNetworkGameState result = session.ApplyRequestAndRelay(new NetworkGameCommandRequest_Lobby_ToggleReady(_clientA), clientRequestedChangeId: null);

            Assert.That(result.Sequence, Is.EqualTo(2));
            Assert.That(result.GetLobbyState().IsClientReady(_clientA), Is.True);
            Assert.That(sender.Messages.Count, Is.EqualTo(2));
            Assert.That(sender.Messages[0].ClientId, Is.EqualTo(_clientA.Id));
            AssertThatCommandBatch(sender.Messages[0].Message, 2, typeof(NetworkGameCommand_Lobby_ReadyToggled));
        }

        [Test]
        public void EveryoneReadyStartsRunAndCombatInOneCommandBatch()
        {
            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
            RecordingMessageSender sender = new();
            ServerGameSession session = new(sender, database);
            session.ApplyCommand(new NetworkGameCommand_ConnectedClientsChanged(new ValueArray<ClientInfo>(_clientA, _clientB)));

            session.ApplyRequestAndRelay(new NetworkGameCommandRequest_Lobby_ToggleReady(_clientA), clientRequestedChangeId: null);
            Assert.That(session.DatedNetworkGameState.Sequence, Is.EqualTo(2));
            Assert.That(session.NetworkGameState.GameState.Phase, Is.TypeOf<LobbyRuntime>());
            Assert.That(sender.Messages.Count, Is.EqualTo(2));

            DatedNetworkGameState result = session.ApplyRequestAndRelay(new NetworkGameCommandRequest_Lobby_ToggleReady(_clientB), clientRequestedChangeId: null);

            Assert.That(result.Sequence, Is.EqualTo(4));
            Assert.That(result.GetCombatRuntime().Instance.DefinitionId, Is.EqualTo("Forest_Test"));
            Assert.That(sender.Messages.Count, Is.EqualTo(6));
            AssertThatCommandBatch(sender.Messages[2].Message, 3, typeof(NetworkGameCommand_Lobby_ReadyToggled));
            AssertThatCommandBatch(sender.Messages[4].Message, 4, typeof(NetworkGameCommand_Lobby_RunStarted), typeof(NetworkGameCommand_Run_CombatStarted));
        }

        [Test]
        public void CombatEndStatusIsDerivedFromStateInsteadOfBroadcastAsACommand()
        {
            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);

            DatedNetworkGameState active = CreateCombatState(database, allyHealth: 10, enemyHealth: 10);
            Assert.That(active.NetworkGameState.GetCombatState().DeriveStatus(), Is.TypeOf<CombatStatus_Active>());

            DatedNetworkGameState victory = CreateCombatState(database, allyHealth: 10, enemyHealth: 0);
            Assert.That(victory.NetworkGameState.GetCombatState().DeriveStatus(), Is.TypeOf<CombatStatus_Victory>());

            DatedNetworkGameState defeat = CreateCombatState(database, allyHealth: 0, enemyHealth: 10);
            Assert.That(defeat.NetworkGameState.GetCombatState().DeriveStatus(), Is.TypeOf<CombatStatus_Defeat>());
        }

        [Test]
        public void MessageHelpersSendCommandBatchesToSelectedClients()
        {
            RecordingMessageSender sender = new();
            NetworkGameState state = new(new ValueArray<ClientInfo>(_clientA, _clientB), GameState.InitialState);
            NetworkGameCommand command = new NetworkGameCommand_Lobby_ReadyToggled(_clientA);
            ValueArray<NetworkGameCommand> commands = new(command);

            sender.SendCommandBatchToAllClientsExcept(_clientA.Id, state, commands, sequenceId: 7, clientRequestedChangeId: null);
            Assert.That(sender.Messages.Count, Is.EqualTo(1));
            Assert.That(sender.Messages[0].ClientId, Is.EqualTo(_clientB.Id));
            AssertThatCommandBatch(sender.Messages[0].Message, 7, command.GetType());

            sender.Messages.Clear();
            sender.SendCommandBatchToClients(state.ConnectedClients, state, commands, sequenceId: 8, clientRequestedChangeId: null);
            Assert.That(sender.Messages.Count, Is.EqualTo(2));

            sender.Messages.Clear();
            sender.SendCommandBatchToClients(new[] { _clientA }, state, commands, sequenceId: 9, clientRequestedChangeId: null);
            Assert.That(sender.Messages.Count, Is.EqualTo(1));
            Assert.That(sender.Messages[0].ClientId, Is.EqualTo(_clientA.Id));
            AssertThatCommandBatch(sender.Messages[0].Message, 9, command.GetType());

            sender.Messages.Clear();
            sender.SendMessageToClient(_clientA.Id, NetworkMessageTypeNames.LOG, "hello");
            TypedMessage log = sender.Messages[0].Message.DeserializeIntoTypedMessage()!;
            Assert.That(log.Type, Is.EqualTo(NetworkMessageTypeNames.LOG));
            Assert.That(log.TryExtractDataAsString(out string? value, out _), Is.True);
            Assert.That(value, Is.EqualTo("hello"));
        }

        private static void AssertThatCommandBatch(string serialized, long sequence, params Type[] commandTypes)
        {
            TypedMessage typedMessage = serialized.DeserializeIntoTypedMessage()!;
            Assert.That(typedMessage.Type, Is.EqualTo(NetworkMessageTypeNames.NETWORK_GAME_COMMANDS_APPLIED));
            Assert.That(typedMessage.TryExtractData<TypedMessageData_NetworkGameCommandsApplied>(out TypedMessageData_NetworkGameCommandsApplied? data, out string error), Is.True, error);
            Assert.That(data!.DatedCommandBatch.Sequence, Is.EqualTo(sequence));
            Assert.That(data.DatedCommandBatch.Commands.Select(command => command.GetType()), Is.EqualTo(commandTypes));
            Assert.That(data.NetworkGameStateHash.RawBytes.Count, Is.EqualTo(32));
        }

        private static DatedNetworkGameState CreateCombatState(GameDatabase database, int allyHealth, int enemyHealth)
        {
            CombatUnitUniqueIdentifier allyId = new(new CombatUnitOwner_Player(_clientA), "Apothecary", 0);
            CombatUnitUniqueIdentifier enemyId = new(new CombatUnitOwner_AI(), "Spider", 0);
            UnitDefinition allyDefinition = database.GetUnitDefinition(allyId.DefinitionId);
            UnitDefinition enemyDefinition = database.GetUnitDefinition(enemyId.DefinitionId);
            UnitState ally = MakeUnit(allyId, allyDefinition.StartingAffiliation, allyHealth, new GridPosition(2, 2), allyDefinition.BaseActionPoints);
            UnitState enemy = MakeUnit(enemyId, enemyDefinition.StartingAffiliation, enemyHealth, new GridPosition(4, 2), enemyDefinition.BaseActionPoints);
            UnitBuild allyBuild = new(allyId, ally.GridPosition, ally.FacingDirection, EternalWar.Core.Model.Combat.Common.Stats.CombatStats.Empty, allyDefinition.CombatActionIds, ValueArray<string>.Empty);
            UnitBuild enemyBuild = new(enemyId, enemy.GridPosition, enemy.FacingDirection, EternalWar.Core.Model.Combat.Common.Stats.CombatStats.Empty, enemyDefinition.CombatActionIds, ValueArray<string>.Empty);
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder states = ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.CreateBuilder();
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.Builder builds = ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.CreateBuilder();
            states.Add(allyId, ally);
            states.Add(enemyId, enemy);
            builds.Add(allyId, allyBuild);
            builds.Add(enemyId, enemyBuild);
            CombatRuntime combat = new(new CombatInstance("Forest_Test", new TileGridInstance(Vector3.Zero), builds.ToValueDictionary()), new CombatState("Forest_Test", states.ToValueDictionary()));
            RunRuntime run = RunInitialization.InitializeRun(new ValueArray<CombatUnitUniqueIdentifier>(allyId), RandomnessSeed: 0).ReplacePhase(combat);
            NetworkGameState networkState = new(new ValueArray<ClientInfo>(_clientA), new GameState(run));
            return new DatedNetworkGameState(10, networkState);
        }

        private static UnitState MakeUnit(CombatUnitUniqueIdentifier id, TeamAffiliation affiliation, int health, GridPosition position, int actionPoints)
        {
            return new UnitState(id, affiliation, health, actionPoints, false, position, EightDirection.Down, ValueArray<PassiveState>.Empty, ValueArray<string>.Empty, ValueArray<TemporaryStatModifierState>.Empty, null);
        }

        private sealed class RecordingMessageSender : IMessageSender
        {
            public List<(Guid ClientId, string Message)> Messages { get; } = new();
            public void SendMessageToClient(Guid clientId, string message) => Messages.Add((clientId, message));
        }
    }
}
