// Author - Jack Dekko
using EternalWar.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EternalWar.Core.Model.Combat.Common
{
    public static class GridPositionExtensions
    {
        public static GridPosition Interpolate(this GridPosition a, GridPosition b, float t)
        {
            if (t <= 0.0f)
                return a;
            if (t >= 1.0f)
                return b;

            Vector2Int difference = b.Position - a.Position;
            Vector2 tDifferenceFloat = difference * t;
            Vector2Int tDifference = tDifferenceFloat.RoundToInt();
            return a + tDifference;
        }

        public static GridPosition Add(this GridPosition position, int x, int y)
        {
            return position + new Vector2Int(x, y);
        }

        public static GridPosition[] GetAdjacentGridPositions(this GridPosition position)
        {
            var adjacents = new GridPosition[4]
            {
                new(position.Position.X - 1, position.Position.Y),
                new(position.Position.X + 1, position.Position.Y),
                new(position.Position.X, position.Position.Y - 1),
                new(position.Position.X, position.Position.Y + 1)
            };
            return adjacents;
        }

        public static float DistanceTo(this GridPosition fromPosition, GridPosition toPosition, DistanceType distanceType)
        {
            Vector2Int fromVector = fromPosition.Position;
            Vector2Int toVector = toPosition.Position;
            Vector2Int difference = fromVector - toVector;

            return distanceType switch
            {
                DistanceType.Manhattan => Math.Abs(difference.X) + Math.Abs(difference.Y),
                DistanceType.Chebyshev => Math.Max(Math.Abs(difference.X), Math.Abs(difference.Y)),
                DistanceType.Euclidean => difference.Length(),
                _ => throw new NotImplementedException($"Unhandled {nameof(DistanceType)}: ({distanceType})")
            };
        }

        public static GridPosition CalculateCenterPosition(this IEnumerable<GridPosition> positions)
        {
            Vector2Int center = Vector2Int.Zero;
            int count = 0;
            foreach (GridPosition position in positions)
            {
                center += position.Position;
                ++count;
            }
            center /= count;
            return new GridPosition(center);
        }
    }
}
