// Original Authors - Wyatt Senalik
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "actionType")]
    [JsonDerivedType(typeof(PassiveActionInfo_CombatAction), "CombatAction")]
    [JsonDerivedType(typeof(PassiveActionInfo_None), "None")]
    public abstract record PassiveActionInfo
    {
        public static readonly PassiveActionInfo Empty = PassiveActionInfo_None.Empty;
    }

    public sealed record PassiveActionInfo_CombatAction(string CombatActionId) : PassiveActionInfo
    {
        public static readonly PassiveActionInfo_CombatAction Empty = new(string.Empty);
    }

    public sealed record PassiveActionInfo_None() : PassiveActionInfo
    {
        public static readonly PassiveActionInfo_None Empty = new();
    }
}
