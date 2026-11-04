// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using System;
using Direction = EternalWar.Core.Model.Combat.Definitions.PushDirection;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.PushDirection
{
    public sealed record PushDirectionContext(GameDatabase Database, CombatInstance CombatInstance, CombatState CombatState, CombatUnitUniqueIdentifier UserIdentifier, CombatUnitUniqueIdentifier TargetIdentifier, int TilesToPush);

    public interface IPushDirectionHandler
    {
        GridPosition DeterminePosition(PushDirectionContext context);
    }

    public static class PushDirectionHandlerFactory
    {
        public static IPushDirectionHandler CreateHandler(Direction pushDirection)
        {
            return pushDirection switch
            {
                Direction.AwayFromSource => new PushDirectionHandler_AwayFromSource(),
                _ => throw new NotImplementedException($"Unhandled {nameof(Direction)}: {pushDirection}")
            };
        }
    }
}
