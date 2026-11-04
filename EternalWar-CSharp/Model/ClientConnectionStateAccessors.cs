// Original Authors - Wyatt Senalik
using EternalWar.Core.Network;
using System;

namespace EternalWar.Client.Application.Model
{
    public static class ClientConnectionStateAccessors
    {
        public static ClientInfo GetMyClientInfo(this ClientConnectionState clientConnectionState)
        {
            return clientConnectionState is ClientConnectionState_Connected connectedState ? connectedState.MyClientInfo : throw new Exception($"Tried to get MyClientInfo while not connected");
        }
    }
}
