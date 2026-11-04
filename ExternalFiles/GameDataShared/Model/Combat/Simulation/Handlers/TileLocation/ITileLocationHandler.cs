// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Runtime;
using System;
using System.Collections.Generic;
using Location = EternalWar.Core.Model.Combat.Definitions.TileLocation;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation
{
    public sealed record TileLocationHandlerContext(GameDatabase Database, CombatState CombatState, CombatUnitUniqueIdentifier UserIdentifier, GridPositionGroup SelectionGroup, int LocationValue);

    public interface ITileLocationHandler
    {
        IReadOnlyList<GridPosition> DetermineGridPositions(TileLocationHandlerContext context);
    }

    public static class TileLocationHandlerFactory
    {
        public static ITileLocationHandler CreateHandler(Location tileLocation)
        {
            return tileLocation switch
            {
                Location.MostRecentlySelected => new TileLocationHandler_MostRecentlySelected(),
                Location.All => new TileLocationHandler_All(),
                Location.Random => new TileLocationHandler_Random(),
                Location.Self => new TileLocationHandler_Self(),
                Location.UnitWhoMostRecentlyDamagedMe => new TileLocationHandler_UnitWhoMostRecentlyDamagedMe(),
                Location.Square => new TileLocationHandler_Square(),
                _ => throw new NotImplementedException($"Unhandled {nameof(TileLocation)}: ({tileLocation})")
            };
        }
    }
}
