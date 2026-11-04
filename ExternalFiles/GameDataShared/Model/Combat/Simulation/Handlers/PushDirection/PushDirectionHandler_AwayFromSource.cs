// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;
using System.Numerics;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.PushDirection
{
    public sealed class PushDirectionHandler_AwayFromSource : IPushDirectionHandler
    {
        public GridPosition DeterminePosition(PushDirectionContext context)
        {
            GameDatabase database = context.Database;
            CombatState combatState = context.CombatState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            CombatUnitUniqueIdentifier targetIdentifier = context.TargetIdentifier;
            int tilesToPush = context.TilesToPush;

            UnitState user = combatState.GetUnit(userIdentifier);
            UnitState target = combatState.GetUnit(targetIdentifier);

            GridPosition userPosition = user.GridPosition;
            GridPosition targetPosition = target.GridPosition;
            Vector2Int userPositionVector = userPosition.Position;
            Vector2Int targetPositionVector = targetPosition.Position;
            Vector2Int awayFromSourceDifference = targetPositionVector - userPositionVector;
            Vector2 awayFromSourceDirection = new(awayFromSourceDifference.X, awayFromSourceDifference.Y);
            awayFromSourceDirection = awayFromSourceDirection.Normalized();
            EightDirection directionToPush = awayFromSourceDirection.GetClosestEightDirection();

            // Account for diagonal pushing
            tilesToPush = Math.Max(1, directionToPush.IsDiagonal() ? tilesToPush : (int)MathF.Floor(tilesToPush * MathHelpers.SQRT_2 * 0.5f));

            // Step through positions
            GridPosition pushToPosition = targetPosition;
            Vector2Int pushSingleStep = directionToPush.GetVector2Int();
            for (int i = 0; i < tilesToPush; ++i)
            {
                GridPosition nextPushToPosition = pushToPosition + pushSingleStep;
                if (CombatGridHelpers.IsTileBlocked(database, combatState, nextPushToPosition))
                    break;
                pushToPosition = nextPushToPosition;
            }
            return pushToPosition;
        }
    }
}
