// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common.Stats;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record ArtifactDefinition : IHaveStringId
    {
        public string Id { get; init; }
        public ValueArray<StatModificationInfo> StatModifications { get; init; }
        public ValueArray<string> PassivesToApply { get; init; }

        [JsonConstructor]
        public ArtifactDefinition(string id, ValueArray<StatModificationInfo> statModifications, ValueArray<string> passivesToApply)
        {
            Id = id;
            StatModifications = statModifications;
            PassivesToApply = passivesToApply;
        }
        public ArtifactDefinition(string id, ValueArray<StatModificationInfo> statModifications) : this(id, statModifications, ValueArray<string>.Empty)
        { }
        public ArtifactDefinition(string id, ValueArray<string> passivesToApply) : this(id, ValueArray<StatModificationInfo>.Empty, passivesToApply)
        { }
        public ArtifactDefinition(string id) : this(id, ValueArray<StatModificationInfo>.Empty, ValueArray<string>.Empty)
        { }
    }

    public sealed record StatModificationInfo(StatType StatToModify, NCalcFunctionInfo NCalcFunction)
    {
        public static readonly StatModificationInfo Empty = new(default, NCalcFunctionInfo.Empty);
    }
}
