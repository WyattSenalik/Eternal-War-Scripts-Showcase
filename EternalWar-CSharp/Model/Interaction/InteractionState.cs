// Original Authors - Wyatt Senalik
namespace EternalWar.Client.Application.Model.Interaction
{
    public abstract record InteractionState
    {
        public static readonly InteractionState Initial = InteractionState_Lobby.Empty;
    }

    public sealed record InteractionState_Lobby(string IPAddress, string Port) : InteractionState
    {
        public static readonly InteractionState_Lobby Empty = new(string.Empty, string.Empty);
    }

    public sealed record InteractionState_Run(bool IsMapShowing) : InteractionState
    {
        public static readonly InteractionState_Run Empty = new(false);
    }
}
