// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Lobby.Views.Actions;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.Run.Map.Views.Actions;

namespace EternalWar.Client.Application
{
    public sealed record GameViewActions(ILobbyViewActions LobbyViewActions, ICombatViewActions CombatViewActions, IMapViewActions MapViewActions);
}
