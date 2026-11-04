// Original Authors - Wyatt Senalik
using System;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Client.Application.Model.Interaction
{
    public static class InteractionStateAccessors
    {
        public static InteractionState_Lobby GetLobbyState(this InteractionState interactionState)
        {
            if (interactionState is not InteractionState_Lobby lobbyState)
                throw new Exception($"Interaction State is not in lobby state as was assumed ({interactionState?.GetType()}: ({interactionState}))");
            return lobbyState;
        }
        public static bool TryGetLobbyState(this InteractionState interactionState, [NotNullWhen(true)] out InteractionState_Lobby? lobbyState)
        {
            if (interactionState is not InteractionState_Lobby castedState)
            {
                lobbyState = null;
                return false;
            }
            lobbyState = castedState;
            return true;
        }
    }
}
