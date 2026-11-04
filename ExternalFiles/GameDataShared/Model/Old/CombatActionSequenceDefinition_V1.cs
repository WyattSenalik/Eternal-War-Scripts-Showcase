// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using System;

namespace EternalWar.Core.Model.Old
{
    public sealed record CombatActionSequenceDefinition_V1(string Id, int ActionPointCost, ValueArray<CombatStepInfo_V1> Steps) : IHaveStringId;

    public sealed record CombatStepInfo_V1(OldCombatStepType StepType, bool ShouldUserFaceTarget, CombatStepInfo_Move_V1 MoveInfo, CombatStepInfo_Melee_V1 MeleeInfo, CombatStepInfo_Heal_V1 HealInfo, CombatStepInfo_Push_V1 PushInfo, CombatStepInfo_ModifyStat_V1 ModifyStatInfo, CombatStepInfo_AddRemoveNonStackableEffect_V1 AddRemoveNonStackableEffectInfo, CombatStepInfo_AddPassive_V1 AddPassiveInfo)
    {
        public static readonly CombatStepInfo_V1 Empty = new(default, false, CombatStepInfo_Move_V1.Empty, CombatStepInfo_Melee_V1.Empty, CombatStepInfo_Heal_V1.Empty, CombatStepInfo_Push_V1.Empty, CombatStepInfo_ModifyStat_V1.Empty, CombatStepInfo_AddRemoveNonStackableEffect_V1.Empty, CombatStepInfo_AddPassive_V1.Empty);
    }

    public enum OldCombatStepType
    {
        Move = 0,
        Melee = 1,
        Push = 2,
        ModifyStat = 3,
        Heal = 4,
        AddPassive = 6,
        AddRemoveNonStackableEffect = 7,
    }

    public sealed record CombatStepInfo_Move_V1(TileLocationInfo Location) : IHaveTileLocation
    {
        public static readonly CombatStepInfo_Move_V1 Empty = new(TileLocationInfo.Empty);
    }

    public sealed record CombatStepInfo_Melee_V1(TileLocationInfo Location, DamageInfo DamageInfo, int BaseAccuracy) : IHaveTileLocation
    {
        public static readonly CombatStepInfo_Melee_V1 Empty = new(TileLocationInfo.Empty, DamageInfo.Empty, 0);
    }

    public sealed record CombatStepInfo_Heal_V1(TileLocationInfo Location, HitInfo HitInfo) : IHaveTileLocation
    {
        public static readonly CombatStepInfo_Heal_V1 Empty = new(TileLocationInfo.Empty, HitInfo.Empty);
    }

    public sealed record CombatStepInfo_Push_V1(TileLocationInfo Location, HitRulesInfo HitRules, PushDirection Direction, int TilesToPush) : IHaveTileLocation
    {
        public static readonly CombatStepInfo_Push_V1 Empty = new(TileLocationInfo.Empty, HitRulesInfo.Empty, default, 0);
    }

    public sealed record CombatStepInfo_ModifyStat_V1(TileLocationInfo Location, ActionInvolvedUnit WhoseStatsToUse, StatType StatToModify, ExpirationInfoGroup ExpirationInfo, HitInfo HitInfo) : IHaveTileLocation
    {
        public static readonly CombatStepInfo_ModifyStat_V1 Empty = new(TileLocationInfo.Empty, default, default, ExpirationInfoGroup.Empty, HitInfo.Empty);
    }

    public sealed record CombatStepInfo_AddRemoveNonStackableEffect_V1(TileLocationInfo Location, HitRulesInfo HitRules, string NonStackableEffectId, AddRemoveType AddRemoveType) : IHaveTileLocation
    {
        public static readonly CombatStepInfo_AddRemoveNonStackableEffect_V1 Empty = new(TileLocationInfo.Empty, HitRulesInfo.Empty, string.Empty, default);
    }

    public sealed record CombatStepInfo_AddPassive_V1(TileLocationInfo Location, string PassiveId, NCalcFunctionInfo StacksToAddCalcFunction, HitRulesInfo HitRules) : IHaveTileLocation
    {
        public static readonly CombatStepInfo_AddPassive_V1 Empty = new(TileLocationInfo.Empty, string.Empty, NCalcFunctionInfo.Empty, HitRulesInfo.Empty);
    }

    public static class CombatStepInfo_V1_Migration
    {
        public static CombatActionSequenceDefinition Migrate(this CombatActionSequenceDefinition_V1 oldActionSequence)
        {
            string id = oldActionSequence.Id;
            int actionPointCost = oldActionSequence.ActionPointCost;

            using ValueArray<CombatStepInfo>.Builder newStepsBuilder = new();

            ValueArray<CombatStepInfo_V1> steps = oldActionSequence.Steps;
            foreach (CombatStepInfo_V1 combatStep in steps)
            {
                CombatStepInfo newStepInfo = combatStep.Migrate();
                newStepsBuilder.Add(newStepInfo);
            }

            return new CombatActionSequenceDefinition(id, actionPointCost, newStepsBuilder.ToValueArray());
        }

        private static CombatStepInfo Migrate(this CombatStepInfo_V1 oldCombatStepInfo)
        {
            bool shouldUserFaceTarget = oldCombatStepInfo.ShouldUserFaceTarget;

            OldCombatStepType oldStepType = oldCombatStepInfo.StepType;
            return oldStepType switch
            {
                OldCombatStepType.Move => Migrate(oldCombatStepInfo.MoveInfo, shouldUserFaceTarget),
                OldCombatStepType.Melee => Migrate(oldCombatStepInfo.MeleeInfo, shouldUserFaceTarget),
                OldCombatStepType.Push => Migrate(oldCombatStepInfo.PushInfo, shouldUserFaceTarget),
                OldCombatStepType.ModifyStat => Migrate(oldCombatStepInfo.ModifyStatInfo, shouldUserFaceTarget),
                OldCombatStepType.Heal => Migrate(oldCombatStepInfo.HealInfo, shouldUserFaceTarget),
                OldCombatStepType.AddPassive => Migrate(oldCombatStepInfo.AddPassiveInfo, shouldUserFaceTarget),
                OldCombatStepType.AddRemoveNonStackableEffect => Migrate(oldCombatStepInfo.AddRemoveNonStackableEffectInfo, shouldUserFaceTarget),
                _ => throw new NotImplementedException($"Unhandled {nameof(OldCombatStepType)}: ({oldStepType})")
            };
        }

        private static CombatStepInfo_Move Migrate(this CombatStepInfo_Move_V1 oldMoveInfo, bool shouldUserFaceTarget)
        {
            return new CombatStepInfo_Move(shouldUserFaceTarget, oldMoveInfo.Location);
        }

        private static CombatStepInfo_Melee Migrate(this CombatStepInfo_Melee_V1 oldMeleeInfo, bool shouldUserFaceTarget)
        {
            return new CombatStepInfo_Melee(shouldUserFaceTarget, oldMeleeInfo.Location, oldMeleeInfo.DamageInfo, oldMeleeInfo.BaseAccuracy);
        }

        private static CombatStepInfo_Push Migrate(this CombatStepInfo_Push_V1 oldPushInfo, bool shouldUserFaceTarget)
        {
            return new CombatStepInfo_Push(shouldUserFaceTarget, oldPushInfo.Location, oldPushInfo.HitRules, oldPushInfo.Direction, oldPushInfo.TilesToPush);
        }

        private static CombatStepInfo_ModifyStat Migrate(this CombatStepInfo_ModifyStat_V1 oldModifyStatInfo, bool shouldUserFaceTarget)
        {
            return new CombatStepInfo_ModifyStat(shouldUserFaceTarget, oldModifyStatInfo.Location, oldModifyStatInfo.WhoseStatsToUse, oldModifyStatInfo.StatToModify, oldModifyStatInfo.ExpirationInfo, oldModifyStatInfo.HitInfo);
        }

        private static CombatStepInfo_Heal Migrate(this CombatStepInfo_Heal_V1 oldHealInfo, bool shouldUserFaceTarget)
        {
            return new CombatStepInfo_Heal(shouldUserFaceTarget, oldHealInfo.Location, oldHealInfo.HitInfo);
        }

        private static CombatStepInfo_AddPassive Migrate(this CombatStepInfo_AddPassive_V1 oldAddPassiveInfo, bool shouldUserFaceTarget)
        {
            return new CombatStepInfo_AddPassive(shouldUserFaceTarget, oldAddPassiveInfo.Location, oldAddPassiveInfo.PassiveId, oldAddPassiveInfo.StacksToAddCalcFunction, oldAddPassiveInfo.HitRules);
        }

        private static CombatStepInfo_AddRemoveNonStackableEffect Migrate(this CombatStepInfo_AddRemoveNonStackableEffect_V1 oldAddRemoveNonStackableEffectInfo, bool shouldUserFaceTarget)
        {
            return new CombatStepInfo_AddRemoveNonStackableEffect(shouldUserFaceTarget, oldAddRemoveNonStackableEffectInfo.Location, oldAddRemoveNonStackableEffectInfo.HitRules, oldAddRemoveNonStackableEffectInfo.NonStackableEffectId, oldAddRemoveNonStackableEffectInfo.AddRemoveType);
        }
    }
}