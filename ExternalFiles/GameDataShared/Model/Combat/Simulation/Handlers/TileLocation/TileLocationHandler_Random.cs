// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.TileLocation
{
    public sealed class TileLocationHandler_Random : ITileLocationHandler
    {
        public IReadOnlyList<GridPosition> DetermineGridPositions(TileLocationHandlerContext context)
        {
            throw new NotImplementedException($"{nameof(TileLocationHandler_Random)} is not yet implemented. Need to be passing the rng in the context");
        }
    }
}
