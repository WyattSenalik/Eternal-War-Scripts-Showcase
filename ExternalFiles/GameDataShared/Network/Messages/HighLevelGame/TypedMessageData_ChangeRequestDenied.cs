// Original Authors - Wyatt Senalik
using EternalWar.Core.Model;

namespace EternalWar.Core.Network.Messages.HighLevelGame
{
    // TODO: Actually handle this on the client-side.
    public sealed record TypedMessageData_ChangeRequestDenied(ChangeRequestId ChangeId);
}
