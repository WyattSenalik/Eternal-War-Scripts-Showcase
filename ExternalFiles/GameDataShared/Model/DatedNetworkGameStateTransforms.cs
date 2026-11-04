// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Simulation;
using System;

namespace EternalWar.Core.Model
{
    public static class DatedNetworkGameStateTransforms
    {
        public static DatedNetworkGameState TransformNetworkGameState(this DatedNetworkGameState datedNetworkGameState, Func<NetworkGameState, NetworkGameState> transformFunc, bool shouldIncrementSequence = true)
        {
            NetworkGameState oldNetworkGameState = datedNetworkGameState.NetworkGameState;
            NetworkGameState newNetworkGameState = transformFunc.Invoke(oldNetworkGameState);
            return datedNetworkGameState with
            {
                NetworkGameState = newNetworkGameState,
                Sequence = datedNetworkGameState.Sequence + (shouldIncrementSequence ? 1 : 0) 
            };
        }

        public static DatedNetworkGameState ApplyCommandBatch(this DatedNetworkGameState datedNetworkGameState, GameDatabase database, DatedNetworkGameCommandBatch commandBatch, bool shouldIncrementSequence = true)
        {
            return datedNetworkGameState.TransformNetworkGameState(oldNetworkGameState => oldNetworkGameState.ApplyCommands(database, commandBatch.Commands).State, shouldIncrementSequence);
        }
    }
}
