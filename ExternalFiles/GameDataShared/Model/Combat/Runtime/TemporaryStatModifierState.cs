// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Runtime
{
    // TODO: Make this allow for operation (add/multiply/override) instead of just add
    public sealed record TemporaryStatModifierState(StatType Stat, int Amount, ExpirationInfoGroup ExpirationInfo, int ExpirationProgress);
}
