// Author - Jack Dekko
using EternalWar.Core.Helpers;

namespace EternalWar.Core.Model.Combat.Common
{
    public readonly record struct GridPosition(Vector2Int Position)
    {
        public GridPosition(int x, int y) : this(new Vector2Int(x, y)) { }

        public static GridPosition operator +(GridPosition a, GridPosition b)
        {
            return new GridPosition(a.Position + b.Position);
        }

        public static GridPosition operator -(GridPosition a, GridPosition b)
        {
            return new GridPosition(a.Position - b.Position);
        }

        public static GridPosition operator +(GridPosition a, Vector2Int b)
        {
            return new GridPosition(a.Position + b);
        }

        public static GridPosition operator +(Vector2Int a, GridPosition b)
        {
            return new GridPosition(a + b.Position);
        }

        public static GridPosition operator -(GridPosition a, Vector2Int b)
        {
            return new GridPosition(a.Position - b);
        }

        public static GridPosition operator -(Vector2Int a, GridPosition b)
        {
            return new GridPosition(a - b.Position);
        }
    }

    public interface IHaveGridPosition
    {
        public GridPosition GridPosition { get; }
    }
}
