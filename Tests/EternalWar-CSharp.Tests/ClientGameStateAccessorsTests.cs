// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelNetwork.Controllers;
using EternalWar.Client.Application.Model;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Network;
using NUnit.Framework;
using System;
using System.Linq;
using System.Numerics;

namespace EternalWar.Client.Application.Tests
{
    [TestFixture]
    internal sealed class ClientGameStateAccessorsTests
    {
        private const string DATA_FOLDER_PATH = "../../../../../Data";
        private static readonly ClientInfo _client = new(Guid.Parse("20000000-0000-0000-0000-000000000002"), 3);

        [Test]
        public void LobbyAndConnectionAccessorsReturnMatchingVariantsAndRejectWrongPhases()
        {
            ClientGameState disconnected = new(new ClientConnectionState_Disconnected(), NetworkGameState.InitialState);
            Assert.That(disconnected.GetConnectionState<ClientConnectionState_Disconnected>(), Is.TypeOf<ClientConnectionState_Disconnected>());
            Assert.Throws<Exception>(() => disconnected.GetConnectionState<ClientConnectionState_Connected>());
            Assert.Throws<Exception>(() => disconnected.GetMyClientInfo());
            Assert.That(disconnected.GetGamePhase(), Is.TypeOf<LobbyRuntime>());
            Assert.That(disconnected.GetLobbyRuntime(), Is.EqualTo(LobbyRuntime.InitialState));
            Assert.That(disconnected.NetworkGameState.TryGetLobbyRuntime(out LobbyRuntime? lobby), Is.True);
            Assert.That(lobby, Is.EqualTo(LobbyRuntime.InitialState));
            Assert.That(disconnected.NetworkGameState.TryGetCombatRuntime(out CombatRuntime? combat), Is.False);
            Assert.That(combat, Is.Null);
            Assert.Throws<InvalidCastException>(() => disconnected.GetRunPhase());
            Assert.Throws<InvalidCastException>(() => disconnected.GetRunPhase<TEMP_NOTHING_RUN_PHASE>());
            Assert.Throws<InvalidCastException>(() => disconnected.GetCombatRuntime());
            Assert.Throws<InvalidCastException>(() => disconnected.GetCombatInstance());
            Assert.Throws<InvalidCastException>(() => disconnected.GetCombatState());
            Assert.Throws<InvalidCastException>(() => disconnected.GetTileGridInstance());

            ClientGameState connected = disconnected with { ConnectionState = new ClientConnectionState_Connected(_client) };
            Assert.That(connected.GetMyClientInfo(), Is.EqualTo(_client));
        }

        [Test]
        public void RunAndCombatAccessorsExposeTheCorrectNestedRuntimeAndWorldPosition()
        {
            RunInstance runInstance = new(ValueArray<CombatUnitUniqueIdentifier>.Empty);
            DeterministicRandomState randomState = new(Seed: 0);
            RunState nonCombatRunState = new(new TEMP_NOTHING_RUN_PHASE(), ValueArray<PlayerUnitUpgradesState>.Empty, randomState, null!);
            NetworkGameState nonCombatState = WithRunState(runInstance, nonCombatRunState);
            ClientGameState nonCombat = new(new ClientConnectionState_Connecting(), nonCombatState);
            Assert.That(nonCombat.GetRunPhase(), Is.TypeOf<TEMP_NOTHING_RUN_PHASE>());
            Assert.That(nonCombat.GetRunPhase<TEMP_NOTHING_RUN_PHASE>(), Is.TypeOf<TEMP_NOTHING_RUN_PHASE>());
            Assert.That(nonCombat.NetworkGameState.GetRunInstance(), Is.EqualTo(runInstance));
            Assert.That(nonCombat.NetworkGameState.TryGetCombatRuntime(out CombatRuntime? absentCombat), Is.False);
            Assert.That(absentCombat, Is.Null);
            Assert.Throws<Exception>(() => nonCombat.GetCombatRuntime());

            GameDatabase database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);
            string combatId = database.Combats.Values.First().Id;
            CombatInstance combatInstance = new(combatId, new TileGridInstance(new Vector3(0, 1, 0)), ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.Empty);
            CombatState combatState = new(combatId, ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Empty);
            CombatRuntime combatRuntime = new(combatInstance, combatState);
            NetworkGameState networkState = WithRunState(runInstance, new RunState(combatRuntime, ValueArray<PlayerUnitUpgradesState>.Empty, randomState, null!));
            ClientGameState clientState = new(new ClientConnectionState_Connecting(), networkState);

            Assert.That(clientState.GetRunPhase(), Is.EqualTo(combatRuntime));
            Assert.That(clientState.GetRunPhase<CombatRuntime>(), Is.EqualTo(combatRuntime));
            Assert.That(clientState.GetCombatRuntime(), Is.EqualTo(combatRuntime));
            Assert.That(clientState.GetCombatInstance(), Is.EqualTo(combatInstance));
            Assert.That(clientState.GetCombatState(), Is.EqualTo(combatState));
            Assert.That(clientState.GetTileGridInstance(), Is.EqualTo(combatInstance.TileGrid));
            Assert.That(networkState.TryGetCombatRuntime(out CombatRuntime? foundCombat), Is.True);
            Assert.That(foundCombat, Is.EqualTo(combatRuntime));
            GridPosition position = new(0, 0);
            Assert.That(clientState.GetWorldPosition(database, position), Is.EqualTo(combatInstance.TileGrid.GetWorldPosition(database, combatId, position)));
        }

        [Test]
        public void HighLevelNetworkActionsDelegateConnectionTransitionsToClientSession()
        {
            const int SEQUENCE_ID = 42;

            ClientGameSession session = new(DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH));
            HighLevelNetworkController controller = new(session);

            controller.SelfClientCurrentlyConnecting();
            Assert.That(session.ConnectionState, Is.TypeOf<ClientConnectionState_Connecting>());
            controller.SelfClientConnected(_client, NetworkGameState.InitialState, SEQUENCE_ID);
            Assert.That(session.ConnectionState, Is.EqualTo(new ClientConnectionState_Connected(_client)));
            Assert.That(session.AuthoritativeGameState.Sequence, Is.EqualTo(SEQUENCE_ID));
            controller.SelfClientDisconnected();
            Assert.That(session.ConnectionState, Is.TypeOf<ClientConnectionState_Disconnected>());
            Assert.That(session.AuthoritativeGameState, Is.EqualTo(DatedNetworkGameState.InitialState));
        }

        private static NetworkGameState WithRunState(RunInstance instance, RunState state)
        {
            return new NetworkGameState(ValueArray<ClientInfo>.Empty, new GameState(new RunRuntime(instance, state)));
        }
    }
}
