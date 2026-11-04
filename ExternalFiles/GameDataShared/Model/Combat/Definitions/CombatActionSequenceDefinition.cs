// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common.Stats;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record CombatActionSequenceDefinition(string Id, int ActionPointCost, ValueArray<CombatStepInfo> Steps) : IHaveStringId;

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "stepType")]
    [JsonDerivedType(typeof(CombatStepInfo_Move), "Move")]
    [JsonDerivedType(typeof(CombatStepInfo_Melee), "Melee")]
    [JsonDerivedType(typeof(CombatStepInfo_Heal), "Heal")]
    [JsonDerivedType(typeof(CombatStepInfo_Push), "Push")]
    [JsonDerivedType(typeof(CombatStepInfo_ModifyStat), "ModifyStat")]
    [JsonDerivedType(typeof(CombatStepInfo_AddRemoveNonStackableEffect), "AddRemoveNonStackableEffect")]
    [JsonDerivedType(typeof(CombatStepInfo_AddPassive), "AddPassive")]
    public abstract record CombatStepInfo(bool ShouldUserFaceTarget, TileLocationInfo Location) : IHaveTileLocation;
    // TODO: MakeShouldUserFaceTarget implicit

    public sealed record CombatStepInfo_Move(bool ShouldUserFaceTarget, TileLocationInfo Location) : CombatStepInfo(ShouldUserFaceTarget, Location);

    public sealed record CombatStepInfo_Melee(bool ShouldUserFaceTarget, TileLocationInfo Location, DamageInfo DamageInfo, int BaseAccuracy) : CombatStepInfo(ShouldUserFaceTarget, Location);

    public sealed record CombatStepInfo_Push(bool ShouldUserFaceTarget, TileLocationInfo Location, HitRulesInfo HitRules, PushDirection Direction, int TilesToPush) : CombatStepInfo(ShouldUserFaceTarget, Location);

    public sealed record CombatStepInfo_ModifyStat(bool ShouldUserFaceTarget, TileLocationInfo Location, ActionInvolvedUnit WhoseStatsToUse, StatType StatToModify, ExpirationInfoGroup ExpirationInfo, HitInfo HitInfo) : CombatStepInfo(ShouldUserFaceTarget, Location);

    public sealed record CombatStepInfo_Heal(bool ShouldUserFaceTarget, TileLocationInfo Location, HitInfo HitInfo) : CombatStepInfo(ShouldUserFaceTarget, Location);

    public sealed record CombatStepInfo_AddPassive(bool ShouldUserFaceTarget, TileLocationInfo Location, string PassiveId, NCalcFunctionInfo StacksToAddCalcFunction, HitRulesInfo HitRules) : CombatStepInfo(ShouldUserFaceTarget, Location);

    public sealed record CombatStepInfo_AddRemoveNonStackableEffect(bool ShouldUserFaceTarget, TileLocationInfo Location, HitRulesInfo HitRules, string NonStackableEffectId, AddRemoveType AddRemoveType) : CombatStepInfo(ShouldUserFaceTarget, Location);

    public enum PushDirection
    {
        AwayFromSource = 0,
    }

    public enum AddRemoveType
    {
        Add = 1,
        Remove = 2,
    }

    public static class CombatStepInfoExtensions
    {
        public static bool DoesUseSelection(this CombatStepInfo info)
        {
            TileLocationInfo locationInfo = info.Location;
            TileLocation tileLocation = locationInfo.TileLocation;
            return tileLocation == TileLocation.MostRecentlySelected;
        }
    }
}