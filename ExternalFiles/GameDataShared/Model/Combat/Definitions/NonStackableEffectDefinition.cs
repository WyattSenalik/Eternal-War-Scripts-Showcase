// Original Authors - Wyatt Senalik
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record NonStackableEffectDefinition(string Id, NonStackableEffectInfo Info) : IHaveStringId;

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "effectType")]
    [JsonDerivedType(typeof(NonStackableEffectInfo_SkipTurn), "SkipTurn")]
    [JsonDerivedType(typeof(NonStackableEffectInfo_DoubleTurn), "DoubleTurn")]
    [JsonDerivedType(typeof(NonStackableEffectInfo_Invincible), "Invincible")]
    [JsonDerivedType(typeof(NonStackableEffectInfo_ImmuneToSpecificPassive), "ImmuneToSpecificPassive")]
    public abstract record NonStackableEffectInfo;

    public sealed record NonStackableEffectInfo_SkipTurn() : NonStackableEffectInfo;

    public sealed record NonStackableEffectInfo_DoubleTurn() : NonStackableEffectInfo;

    public sealed record NonStackableEffectInfo_Invincible() : NonStackableEffectInfo;

    public sealed record NonStackableEffectInfo_ImmuneToSpecificPassive(string PassiveId) : NonStackableEffectInfo;
}
