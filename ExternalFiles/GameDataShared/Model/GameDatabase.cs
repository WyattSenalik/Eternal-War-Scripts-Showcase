// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Map.Definitions;
using System;
using System.Collections.Generic;

namespace EternalWar.Core.Model
{
    public sealed record GameDatabase(ValueDictionary<string, CombatDefinition> Combats, ValueDictionary<string, UnitDefinition> Units, ValueDictionary<string, CombatActionSequenceDefinition> ActionSequences, ValueDictionary<string, PassiveDefinition> Passives, ValueDictionary<string, NonStackableEffectDefinition> NonStackableEffects, ValueDictionary<string, ArtifactDefinition> Artifacts, ValueDictionary<string, BlockDefinition> Blocks, ValueDictionary<string, FakeLocationMapDefinition> FakeLocationMaps, ValueDictionary<string, LocationNodeDefinition> LocationNodes);

    public static class GameDatabaseExtensions
    {
        public static CombatDefinition GetCombatDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, CombatDefinition> combats = gameDatabase.Combats;
            return GetDefinition(combats, id);
        }

        public static UnitDefinition GetUnitDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, UnitDefinition> units = gameDatabase.Units;
            return GetDefinition(units, id);
        }

        public static CombatActionSequenceDefinition GetActionSequenceDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, CombatActionSequenceDefinition> actionSequences = gameDatabase.ActionSequences;
            return GetDefinition(actionSequences, id);
        }

        public static PassiveDefinition GetPassiveDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, PassiveDefinition> passives = gameDatabase.Passives;
            return GetDefinition(passives, id);
        }

        public static NonStackableEffectDefinition GetNonStackableEffectDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, NonStackableEffectDefinition> nonStackableEffects = gameDatabase.NonStackableEffects;
            return GetDefinition(nonStackableEffects, id);
        }

        public static ArtifactDefinition GetArtifactDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, ArtifactDefinition> artifacts = gameDatabase.Artifacts;
            return GetDefinition(artifacts, id);
        }

        public static BlockDefinition GetBlockDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, BlockDefinition> blocks = gameDatabase.Blocks;
            return GetDefinition(blocks, id);
        }

        public static FakeLocationMapDefinition GetFakeLocationMapDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, FakeLocationMapDefinition> fakeLocationMaps = gameDatabase.FakeLocationMaps;
            return GetDefinition(fakeLocationMaps, id);
        }

        public static LocationNodeDefinition GetLocationNodeDefinition(this GameDatabase gameDatabase, string id)
        {
            IReadOnlyDictionary<string, LocationNodeDefinition> locationNodes = gameDatabase.LocationNodes;
            return GetDefinition(locationNodes, id);
        }

        private static T GetDefinition<T>(IReadOnlyDictionary<string, T> map, string id)
        {
            if (map.TryGetValue(id, out T? definition))
                return definition;
            throw new Exception($"Failed to find: {typeof(T).Name} with id ({id})");
        }
    }
}
