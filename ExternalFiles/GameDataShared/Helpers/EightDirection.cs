// Original Authors - Wyatt Senalik
using System;
using System.Numerics;

namespace EternalWar.Core.Helpers
{
    public enum EightDirection_Cardinal
    {
        None = 0,
        West = -1,
        East = 1,
        North = 3,
        South = -3,
        NorthWest = 2,
        NorthEast = 4,
        SouthWest = -4,
        SouthEast = -2
    }

    public enum EightDirection
    {
        None = 0,
        Left = -1,
        Right = 1,
        Up = 3,
        Down = -3,
        UpLeft = 2,
        UpRight = 4,
        DownLeft = -4,
        DownRight = -2
    }

    public static class EightDirectionExtensions
    {
        private static readonly Vector2 NONE_V2 = Vector2.Zero;
        private static readonly Vector2 LEFT_V2 = new(-1.0f, 0.0f);
        private static readonly Vector2 RIGHT_V2 = new(1.0f, 0.0f);
        private static readonly Vector2 UP_V2 = new(0.0f, 1.0f);
        private static readonly Vector2 DOWN_V2 = new(0.0f, -1.0f);
        private static readonly Vector2 UP_LEFT_V2 = new(-1.0f, 1.0f);
        private static readonly Vector2 UP_RIGHT_V2 = new(1.0f, 1.0f);
        private static readonly Vector2 DOWN_LEFT_V2 = new(-1.0f, -1.0f);
        private static readonly Vector2 DOWN_RIGHT_V2 = new(1.0f, -1.0f);

        private static readonly Vector2Int NONE_V2INT = Vector2Int.Zero;
        private static readonly Vector2Int LEFT_V2INT = new(-1, 0);
        private static readonly Vector2Int RIGHT_V2INT = new(1, 0);
        private static readonly Vector2Int UP_V2INT = new(0, 1);
        private static readonly Vector2Int DOWN_V2INT = new(0, -1);
        private static readonly Vector2Int UP_LEFT_V2INT = new(-1, 1);
        private static readonly Vector2Int UP_RIGHT_V2INT = new(1, 1);
        private static readonly Vector2Int DOWN_LEFT_V2INT = new(-1, -1);
        private static readonly Vector2Int DOWN_RIGHT_V2INT = new(1, -1);

        public static EightDirection GetOpposite(this EightDirection direction)
        {
            return (EightDirection)(-(int)direction);
        }

        public static Vector2 GetVector2(this EightDirection direction)
        {
            return direction switch
            {
                EightDirection.None => NONE_V2,
                EightDirection.Left => LEFT_V2,
                EightDirection.Right => RIGHT_V2,
                EightDirection.Up => UP_V2,
                EightDirection.Down => DOWN_V2,
                EightDirection.UpLeft => UP_LEFT_V2,
                EightDirection.UpRight => UP_RIGHT_V2,
                EightDirection.DownLeft => DOWN_LEFT_V2,
                EightDirection.DownRight => DOWN_RIGHT_V2,
                _ => throw new NotImplementedException()
            };
        }

        public static Vector2Int GetVector2Int(this EightDirection direction)
        {
            return direction switch
            {
                EightDirection.None => NONE_V2INT,
                EightDirection.Left => LEFT_V2INT,
                EightDirection.Right => RIGHT_V2INT,
                EightDirection.Up => UP_V2INT,
                EightDirection.Down => DOWN_V2INT,
                EightDirection.UpLeft => UP_LEFT_V2INT,
                EightDirection.UpRight => UP_RIGHT_V2INT,
                EightDirection.DownLeft => DOWN_LEFT_V2INT,
                EightDirection.DownRight => DOWN_RIGHT_V2INT,
                _ => throw new NotImplementedException()
            };
        }

        public static EightDirection GetClosestEightDirection(this Vector2Int vector2IntDirection, float positiveCutOffAmount = 0.05f) => GetClosestEightDirection(vector2IntDirection.ToVector2(), positiveCutOffAmount);
        public static EightDirection GetClosestEightDirection(this Vector2 vectorDirection, float positiveCutOffAmount = 0.05f)
        {
            EightDirection horizontalDirection = GetHorizontalEightDirection(vectorDirection.X, positiveCutOffAmount);
            EightDirection verticalDirection = GetVerticalEightDirection(vectorDirection.Y, positiveCutOffAmount);
            return horizontalDirection.UnsafeAdd(verticalDirection);
        }

        public static EightDirection Add(this EightDirection thisDirection, EightDirection otherDirection)
        {
            if (thisDirection == otherDirection)
                return thisDirection;

            if (thisDirection.IsDiagonal() || otherDirection.IsDiagonal())
                return thisDirection.DiagonalAdd(otherDirection);

            return thisDirection.UnsafeAdd(otherDirection);
        }

        public static bool IsDiagonal(this EightDirection direction)
        {
            return direction switch
            {
                EightDirection.None => false,
                EightDirection.Left => false,
                EightDirection.Right => false,
                EightDirection.Up => false,
                EightDirection.Down => false,
                EightDirection.UpLeft => true,
                EightDirection.UpRight => true,
                EightDirection.DownLeft => true,
                EightDirection.DownRight => true,
                _ => throw new NotImplementedException()
            };
        }

        public static void GetComponentDirections(this EightDirection thisDirection, out EightDirection horizontalDirection, out EightDirection verticalDirection)
        {
            horizontalDirection = EightDirection.None;
            verticalDirection = EightDirection.None;
            switch (thisDirection)
            {
                case EightDirection.None:
                    break;
                case EightDirection.Left:
                    horizontalDirection = EightDirection.Left;
                    break;
                case EightDirection.Right:
                    horizontalDirection = EightDirection.Right;
                    break;
                case EightDirection.Up:
                    verticalDirection = EightDirection.Up;
                    break;
                case EightDirection.Down:
                    verticalDirection = EightDirection.Down;
                    break;
                case EightDirection.UpLeft:
                    horizontalDirection = EightDirection.Left;
                    verticalDirection = EightDirection.Up;
                    break;
                case EightDirection.UpRight:
                    horizontalDirection = EightDirection.Right;
                    verticalDirection = EightDirection.Up;
                    break;
                case EightDirection.DownLeft:
                    horizontalDirection = EightDirection.Left;
                    verticalDirection = EightDirection.Down;
                    break;
                case EightDirection.DownRight:
                    horizontalDirection = EightDirection.Right;
                    verticalDirection = EightDirection.Down;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static EightDirection_Cardinal ToCardinal(this EightDirection direction)
        {
            return (EightDirection_Cardinal)direction;
        }

        private static EightDirection GetHorizontalEightDirection(float xComponent, float positiveCutOffAmount)
        {
            if (xComponent > positiveCutOffAmount)
                return EightDirection.Right;
            if (xComponent < -positiveCutOffAmount)
                return EightDirection.Left;
            return EightDirection.None;
        }

        private static EightDirection GetVerticalEightDirection(float yComponent, float positiveCutOffAmount)
        {
            if (yComponent > positiveCutOffAmount)
                return EightDirection.Up;
            if (yComponent < -positiveCutOffAmount)
                return EightDirection.Down;
            return EightDirection.None;
        }

        private static EightDirection DiagonalAdd(this EightDirection thisDirection, EightDirection otherDirection)
        {
            thisDirection.GetComponentDirections(out EightDirection thisHorizontal, out EightDirection thisVertical);
            otherDirection.GetComponentDirections(out EightDirection otherHorizontal, out EightDirection otherVertical);

            EightDirection horizontal = thisHorizontal == otherHorizontal ? thisHorizontal : thisHorizontal.UnsafeAdd(otherHorizontal);
            EightDirection vertical = thisVertical == otherVertical ? thisVertical : thisVertical.UnsafeAdd(otherVertical);
            return horizontal.UnsafeAdd(vertical);
        }

        private static EightDirection UnsafeAdd(this EightDirection thisDirection, EightDirection otherDirection)
        {
            return (EightDirection)((int)thisDirection + (int)otherDirection);
        }
    }

    public static class EightDirection_CardinalExtensions
    {
        public static EightDirection ToEightDirection(this EightDirection_Cardinal cardinalDirection)
        {
            return (EightDirection)cardinalDirection;
        }
    }
}
