// Original Authors - Wyatt Senalik
using EternalWar.Core.Network.Messages;
using System;

namespace EternalWar.Server.Application.Messages
{
    internal sealed record TypedMessageWithClient(TypedMessage TypedMessage, Guid ClientId);
}