// Original Authors - Jack Dekko
using EternalWar.Core.Helpers;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Core.Model.Map.Runtime
{
    public static class LocationMapStateTransforms
    {
        public static LocationMapState TransformVotes(this LocationMapState mapState, Func<ValueDictionary<ClientInfo, int>, ValueDictionary<ClientInfo, int>> transformFunc)
        {
            return new LocationMapState(mapState.CurrentNode, mapState.CurrentNodeIndex, transformFunc(mapState.ClientVotes));
        }
    }
}
