// Original Authors - Wyatt Senalik
namespace EternalWar.Client.Application.Model.Interaction
{
    public abstract record InteractionStateChange_Lobby : InteractionStateChange;

    public sealed record InteractionStateChange_Lobby_IPAddress(string NewIPAddress) : InteractionStateChange_Lobby
    {
        public override InteractionState Apply(InteractionState oldState)
        {
            if (!oldState.TryGetLobbyState(out InteractionState_Lobby? oldLobbyState))
            {
                return InteractionState_Lobby.Empty with
                {
                    IPAddress = NewIPAddress
                };
            }
            return oldLobbyState with
            {
                IPAddress = NewIPAddress
            };
        }
    }

    public sealed record InteractionStateChange_Lobby_Port(string NewPort) : InteractionStateChange_Lobby
    {
        public override InteractionState Apply(InteractionState oldState)
        {
            if (!oldState.TryGetLobbyState(out InteractionState_Lobby? oldLobbyState))
            {
                return InteractionState_Lobby.Empty with
                {
                    Port = NewPort
                };
            }
            return oldLobbyState with
            {
                Port = NewPort
            };
        }
    }
}
