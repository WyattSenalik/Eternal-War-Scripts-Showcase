// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation;
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Network;
using EternalWar.Server.Application.NetworkStateChange.AllowHandlers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EternalWar.Server.Application.Tests
{
    [TestFixture]
    internal sealed class AllowChangeHandlerTests
    {
        private const string DATA_FOLDER_PATH = "../../../../../Data";
        private static readonly ClientInfo _clientA = new(Guid.Parse("40000000-0000-0000-0000-000000000001"), 0);
        private static readonly ClientInfo _clientB = new(Guid.Parse("40000000-0000-0000-0000-000000000002"), 1);
        private GameDatabase _database = null!;

        [SetUp]
        public void SetUp() => _database = DatabaseLoader.LoadDatabase(DATA_FOLDER_PATH);

        [Test]
        public void RegistratorFindsEverySupportedChangeHandlerAndRejectsUnknownTypes()
        {
            AllowChangeHandlerRegistrator registrator = new();
            NetworkGameCommandRequest[] supportedChanges =
            {
                new NetworkGameCommandRequest_Lobby_ToggleReady(_clientA),
                NetworkGameCommandRequest_Run_Combat_EndTurn.Instance,
                new NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence(new ActionExecutionContext("missing", MakeUnitIdentifier(_clientA), new ActionSelectionContext(ValueArray<GridPositionGroup>.Empty)))
            };

            foreach (NetworkGameCommandRequest change in supportedChanges)
            {
                Assert.That(registrator.GetHandler(change), Is.Not.Null);
            }

            Assert.Throws<KeyNotFoundException>(() => registrator.GetHandler(new UnknownChange()));
        }

        [Test]
        public void LobbyToggleAllowsOnlyTheRequestingClientAndOnlyInLobby()
        {
            AllowChangeHandlerRegistrator registrator = new();
            IAllowChangeHandler handler = registrator.GetHandler(new NetworkGameCommandRequest_Lobby_ToggleReady(_clientA));
            NetworkGameState lobby = new(new ValueArray<ClientInfo>(_clientA, _clientB), GameState.InitialState);
            NetworkGameState run = new(new ValueArray<ClientInfo>(_clientA, _clientB), new GameState(RunInitialization.InitializeRun(ValueArray<CombatUnitUniqueIdentifier>.Empty, 0)));

            Assert.That(handler.ShouldAllowChange(_database, lobby, _clientA.Id, new NetworkGameCommandRequest_Lobby_ToggleReady(_clientA)), Is.True);
            Assert.That(handler.ShouldAllowChange(_database, lobby, _clientA.Id, new NetworkGameCommandRequest_Lobby_ToggleReady(_clientB)), Is.False);
            Assert.That(handler.ShouldAllowChange(_database, run, _clientA.Id, new NetworkGameCommandRequest_Lobby_ToggleReady(_clientA)), Is.False);
        }

        [Test]
        public void CombatEndTurnRequiresTheCurrentTurnOwner()
        {
            NetworkGameState combat = CreateSinglePlayerCombatState();
            AllowChangeHandlerRegistrator registrator = new();
            IAllowChangeHandler handler = registrator.GetHandler(NetworkGameCommandRequest_Run_Combat_EndTurn.Instance);

            Assert.That(handler.ShouldAllowChange(_database, combat, _clientA.Id, NetworkGameCommandRequest_Run_Combat_EndTurn.Instance), Is.True);
            Assert.That(handler.ShouldAllowChange(_database, combat, _clientB.Id, NetworkGameCommandRequest_Run_Combat_EndTurn.Instance), Is.False);

            NetworkGameState notCombat = new(new ValueArray<ClientInfo>(_clientA), GameState.InitialState);
            Assert.That(handler.ShouldAllowChange(_database, notCombat, _clientA.Id, NetworkGameCommandRequest_Run_Combat_EndTurn.Instance), Is.False);
        }

        [Test]
        public void CombatActionRequestRequiresCombatCurrentTurnOwnershipAndOwnedAction()
        {
            const string APOTHECARY_ID = "Apothecary";

            NetworkGameState combat = CreateSinglePlayerCombatState();
            CombatUnitUniqueIdentifier actor = MakeUnitIdentifier(_clientA);
            AllowChangeHandlerRegistrator registrator = new();
            NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence change = new(new ActionExecutionContext("missing", actor, new ActionSelectionContext(ValueArray<GridPositionGroup>.Empty)));
            IAllowChangeHandler handler = registrator.GetHandler(change);

            Assert.That(handler.ShouldAllowChange(_database, new NetworkGameState(new ValueArray<ClientInfo>(_clientA), GameState.InitialState), _clientA.Id, change), Is.False);
            Assert.That(handler.ShouldAllowChange(_database, combat, _clientB.Id, change), Is.False);
            Assert.That(handler.ShouldAllowChange(_database, combat, _clientA.Id, change), Is.False);

            UnitDefinition apothecaryDefinition = _database.GetUnitDefinition(APOTHECARY_ID);
            NetworkGameCommandRequest_Run_Combat_ExecuteActionSequence invalidSelection = new(new ActionExecutionContext(apothecaryDefinition.MoveCombatActionId, actor, new ActionSelectionContext(ValueArray<GridPositionGroup>.Empty)));
            Assert.That(handler.ShouldAllowChange(_database, combat, _clientA.Id, invalidSelection), Is.False);
        }

        private NetworkGameState CreateSinglePlayerCombatState()
        {
            CombatUnitUniqueIdentifier unitId = MakeUnitIdentifier(_clientA);
            UnitDefinition unitDefinition = _database.GetUnitDefinition(unitId.DefinitionId);
            GridPosition position = new(3, 0);
            UnitBuild build = new(unitId, position, EightDirection.Down, EternalWar.Core.Model.Combat.Common.Stats.CombatStats.Empty, unitDefinition.CombatActionIds, ValueArray<string>.Empty);
            UnitState state = new(unitId, unitDefinition.StartingAffiliation, unitDefinition.BaseHealth.Health, unitDefinition.BaseActionPoints, false, position, EightDirection.Down, ValueArray<PassiveState>.Empty, ValueArray<string>.Empty, ValueArray<TemporaryStatModifierState>.Empty, null);

            using ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.Builder builds = ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild>.CreateBuilder();
            using ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.Builder states = ValueDictionary<CombatUnitUniqueIdentifier, UnitState>.CreateBuilder();
            builds.Add(unitId, build);
            states.Add(unitId, state);
            CombatInstance instance = new("Forest_Test", new TileGridInstance(Vector3.Zero), builds.ToValueDictionary());
            CombatState combatState = new("Forest_Test", states.ToValueDictionary());
            CombatRuntime runtime = new(instance, combatState);
            RunRuntime run = RunInitialization.InitializeRun(new ValueArray<CombatUnitUniqueIdentifier>(unitId), RandomnessSeed: 0).ReplacePhase(runtime);
            return new NetworkGameState(new ValueArray<ClientInfo>(_clientA), new GameState(run));
        }

        private static CombatUnitUniqueIdentifier MakeUnitIdentifier(ClientInfo client) => new(new CombatUnitOwner_Player(client), "Apothecary", 0);

        private sealed record UnknownChange : NetworkGameCommandRequest;
    }
}
