// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;
using System;

namespace EternalWar.Core.Network.Messages.HighLevelNetwork
{
    [Serializable]
    public sealed record TypedMessageData_SelfClientConnected(ClientInfo SelfInfo, NetworkGameState State, long SequenceId);
}
