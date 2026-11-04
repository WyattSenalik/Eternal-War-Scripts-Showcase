// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Server.Application.NetworkStateChange.AllowHandlers
{
    internal sealed class AllowChangeHandler_Lobby_ToggleReady : AllowChangeHandler<NetworkGameCommandRequest_Lobby_ToggleReady>
    {
        protected override bool ShouldAllowChange(GameDatabase database, NetworkGameState networkGameState, Guid clientIdRequestingChange, NetworkGameCommandRequest_Lobby_ToggleReady changeRequested)
        {
            // Can't change toggle if not in lobby phase.
            if (!networkGameState.TryGetLobbyRuntime(out _))
                return false;

            // Clients are allowed to request to toggling of themselves only.
            ClientInfo clientInfo = changeRequested.ToggleClientInfo;
            return clientInfo.Id == clientIdRequestingChange;
        }
    }
}

