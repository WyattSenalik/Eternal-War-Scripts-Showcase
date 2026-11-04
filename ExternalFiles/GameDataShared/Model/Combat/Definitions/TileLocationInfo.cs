// Original Authors - Wyatt Senalik
namespace EternalWar.Core.Model.Combat.Definitions
{
    // TODO: May be able to just store TileLocation on Definition data used for the simulation instead of also value and selection specifications since those are used before simulation?
    // TODO: Convert this to not use TileLocation, but instead have TileLocationInfo be abstract with implementations for each (where MostRecentlySelected has more info)
    public sealed record TileLocationInfo(TileLocation TileLocation, int Value, SelectionSpecificationsInfo SelectionSpecifications)
    {
        public static readonly TileLocationInfo Empty = new(0, 0, SelectionSpecificationsInfo.Empty);
    }

    public enum TileLocation
    {
        MostRecentlySelected = 0,
        All = 1,
        Random = 2,
        Self = 3,
        UnitWhoMostRecentlyDamagedMe = 4,
        Square = 5,
    }

    public sealed record SelectionSpecificationsInfo(int Range, SelectionRangeType RangeType, int Radius, int Amount, SelectionType Type, HitRulesInfo OccupiedHitRules)
    {
        public static readonly SelectionSpecificationsInfo Empty = new(0, 0, 0, 0, 0, HitRulesInfo.Empty);
    }

    public enum SelectionRangeType
    {
        Distance = 0,
        Steps = 1,
        Movable = 2,
    }

    public enum SelectionType
    {
        Any = 0,
        Unblocked = 1,
        Blocked = 2,
        HasUnit = 3,
    }
}
