//Original Authors - Jack Dekko & Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Network;

namespace EternalWar.Core.Model.Map.Runtime
{
    // Represents runtime state on a map (which node we're on currently)
    // and ClientVotes where the int is the index of the node voted for.
    // Initially empty and then populates with votes
    public sealed record LocationMapState(LocationNodeInstance CurrentNode, int CurrentNodeIndex, ValueDictionary<ClientInfo, int> ClientVotes)
    {
        public static LocationMapState CreateInitial(LocationNodeInstance FirstNode) => new(FirstNode, CurrentNodeIndex: 0, ClientVotes: ValueDictionary<ClientInfo, int>.Empty);
    }
}
