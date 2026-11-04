// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Model
{
    public sealed record ChangeRequestId(Guid RequesterClientGuid, Guid Id)
    {
        public static ChangeRequestId New(Guid requesterClientGuid) => new(requesterClientGuid, Guid.NewGuid());
    }

    public sealed record ChangeRequest(NetworkGameCommandRequest Request, ChangeRequestId? LocalChangeId);
}
