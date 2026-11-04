// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record ExpirationInfo(ExpirationTime ExpirationTime, int OccurrencesBeforeExpiration);

    public sealed record ExpirationInfoGroup(ValueArray<ExpirationInfo> Group)
    {
        public static readonly ExpirationInfoGroup Empty = new(ValueArray<ExpirationInfo>.Empty);
        public static readonly ExpirationInfoGroup Never = new(One(new ExpirationInfo(ExpirationTime.Never, 0)));

        public static ExpirationInfoGroup One(ExpirationInfo info) => new(new ValueArray<ExpirationInfo>(info));
    }
}
