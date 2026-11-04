// Original Authors - Jack Dekko
using EternalWar.Core.Model;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Server.Application.NetworkStateChange.AllowHandlers
{
    internal sealed class AllowChangeHandler_Run_Map_VoteForNode : AllowChangeHandler<NetworkGameCommandRequest_Run_Map_VoteForNode>
    {
        protected override bool ShouldAllowChange(GameDatabase database, NetworkGameState networkGameState, Guid clientIdRequestingChange, NetworkGameCommandRequest_Run_Map_VoteForNode commandRequested)
        {
            // Check if we're in the right phase (must have LocationMapRuntime)
            if (!networkGameState.TryGetLocationMapRuntime(out _))
                return false;

            // Check if the voting client in the change matches the requesting client
            ClientInfo clientToChangeVoteFor = commandRequested.VotingClientInfo;
            if (clientToChangeVoteFor.Id != clientIdRequestingChange)
                return false;

            // TODO: Add a check that the voted for node on the map is one that is currently selectable

            return true;
        }
    }
}
