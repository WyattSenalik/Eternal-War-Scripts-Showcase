// Original Authors - Jack Dekko
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Map.Runtime;
using EternalWar.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Server.Application.NetworkStateChange.PerformPostHandlers
{
    internal sealed class PerformPostCommandRequestHandler_Run_Map_VotingChanged(ServerGameSession session) : PerformPostCommandRequestHandler<NetworkGameCommandRequest_Run_Map_VoteForNode>
    {
        private readonly ServerGameSession _session = session;

        protected override void PerformPostRequest(NetworkGameCommandRequest_Run_Map_VoteForNode request)
        {
            NetworkGameState networkGameState = _session.NetworkGameState;

            if (!networkGameState.TryGetLocationMapRuntime(out LocationMapRuntime? locationMapRuntime))
                return;

            LocationMapState mapState = locationMapRuntime.State;
            ValueArray<ClientInfo> connectedClients = networkGameState.ConnectedClients;
            ValueDictionary<ClientInfo, int> clientVotes = mapState.ClientVotes;

            // Check if all clients have voted
            if (clientVotes.Count < connectedClients.Count)
                return;

            // Everyone has voted - determine the chosen node
            int chosenNode;
            // Check if all votes are the same
            if (AreAllVotesSame(clientVotes, out int commonVote))
            {
                chosenNode = commonVote;
                Debug.Print($"TODO: Enter node: ({chosenNode})");
                return;
            }
            // TODO: use network-synced randomness
            // Randomly pick one of the clients' choices
            Random random = new();
            List<int> votes = clientVotes.Values.ToList();
            int randomIndex = random.Next(votes.Count);
            chosenNode = votes[randomIndex];

            List<ClientInfo> clients = clientVotes.Keys.ToList();
            ClientInfo chosenClientInfo = clients[randomIndex];
            Debug.Print($"Randomly picked: {chosenClientInfo}'s choice. TODO: Enter node: ({chosenNode})");
        }

        private static bool AreAllVotesSame(ValueDictionary<ClientInfo, int> clientVotes, out int commonVote)
        {
            // Check if all votes match the first vote
            int? firstVote = null;
            foreach (int vote in clientVotes.Values)
            {
                if (!firstVote.HasValue)
                {
                    firstVote = vote;
                    continue;
                }
                if (firstVote.Value != vote)
                {
                    commonVote = -1;
                    return false;
                }
            }

            if (!firstVote.HasValue)
            {
                commonVote = -1;
                return false;
            }

            commonVote = firstVote.Value;
            return true;
        }
    }
}
