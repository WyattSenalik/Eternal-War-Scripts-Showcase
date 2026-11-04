using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Lobby.Simulation;
using EternalWar.Core.Network;
using NUnit.Framework;
using System;

namespace EternalWar.Core.Tests
{
    [TestFixture]
    internal sealed class LobbySimulationTests
    {
        private static readonly ClientInfo _clientA = new(Guid.Parse("20000000-0000-0000-0000-000000000001"), 0);
        private static readonly ClientInfo _clientB = new(Guid.Parse("20000000-0000-0000-0000-000000000002"), 1);

        [Test]
        public void ToggleReadyAddsAndRemovesClientsWithoutMutatingEarlierSnapshot()
        {
            LobbyState initial = LobbyState.InitialState;
            LobbyState readyA = initial.ToggleClientReady(_clientA);
            LobbyState readyAB = readyA.ToggleClientReady(_clientB);
            LobbyState readyB = readyAB.ToggleClientReady(_clientA);

            Assert.That(initial.ReadyClients.Count, Is.Zero);
            Assert.That(readyA.GetReadyClientCount(), Is.EqualTo(1));
            Assert.That(readyA.IsClientReady(_clientA), Is.True);
            Assert.That(readyA.IsClientReady(_clientB), Is.False);
            int[] expected = new[] { 0, 1 };
            Assert.That(readyAB.ReadyClients.Keys, Is.EqualTo(expected));
            Assert.That(readyB.IsClientReady(_clientA), Is.False);
            Assert.That(readyB.IsClientReady(_clientB), Is.True);
        }

        [Test]
        public void DisconnectRemovesOnlyTheDisconnectedReadyClient()
        {
            LobbyState ready = LobbyState.InitialState.ToggleClientReady(_clientA).ToggleClientReady(_clientB);
            LobbyState afterDisconnect = ready.HandleClientDisconnect(_clientA);
            LobbyState afterUnknownDisconnect = afterDisconnect.HandleClientDisconnect(new ClientInfo(Guid.NewGuid(), 8));

            Assert.That(afterDisconnect.IsClientReady(_clientA), Is.False);
            Assert.That(afterDisconnect.IsClientReady(_clientB), Is.True);
            Assert.That(afterUnknownDisconnect, Is.SameAs(afterDisconnect));

            LobbyRuntime runtime = LobbyRuntime.InitialState with { State = ready };
            LobbyRuntime disconnectedRuntime = runtime.HandleClientDisconnect(_clientB);
            Assert.That(disconnectedRuntime.State.IsClientReady(_clientB), Is.False);
            Assert.That(runtime.State.IsClientReady(_clientB), Is.True);
        }

        [Test]
        public void ToggleReadyUsesPlayerIndexAndUpdatesTheStoredClientInfo()
        {
            ClientInfo staleInfo = new(Guid.Parse("20000000-0000-0000-0000-000000000099"), 0);
            LobbyState state = LobbyState.InitialState.ToggleClientReady(_clientA);
            LobbyState toggledWithSamePlayerIndex = state.ToggleClientReady(staleInfo);
            Assert.That(toggledWithSamePlayerIndex.ReadyClients.Count, Is.EqualTo(1));
            Assert.That(toggledWithSamePlayerIndex.ReadyClients[0], Is.EqualTo(staleInfo));
            LobbyState replacedInfo = LobbyState.InitialState.ToggleClientReady(_clientA).ToggleClientReady(staleInfo);
            Assert.That(replacedInfo.ReadyClients.Count, Is.EqualTo(1));
            Assert.That(replacedInfo.ReadyClients[0], Is.EqualTo(staleInfo));
            Assert.That(replacedInfo.ToggleClientReady(staleInfo).ReadyClients.Count, Is.Zero);
        }
    }
}
