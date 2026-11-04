// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public sealed record Path(ValueArray<GridPosition> Positions)
    {
        public override string ToString()
        {
            if (Positions.Count <= 0)
                return "Empty Path";
            Vector2Int indices = Positions[0].Position;
            string pathDisplay = $"Path: ({indices.X}, {indices.Y})";
            for (int i = 1; i < Positions.Count; ++i)
            {
                indices = Positions[i].Position;
                pathDisplay += $" -> ({indices.X}, {indices.Y})";
            }
            pathDisplay += " End";
            return pathDisplay;
        }
    }
}
