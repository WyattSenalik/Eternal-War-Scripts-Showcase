// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    /// <summary>Helpers for determining if targets are valid.</summary>
    public static class CombatTargetingRulesHelpers
    {
        // TODO: If we introduce things like stealth, line or sight, etc. And then rename this to simply IsSatisfied or something
        public static bool AreHitRulesSatisfied(HitRulesInfo hitRules, UnitState user, UnitState target, GameDatabase database)
        {
            CombatUnitUniqueIdentifier targetIdentifier = target.UniqueIdentifier;
            string targetDefinitionId = targetIdentifier.DefinitionId;
            UnitDefinition targetDefinition = database.GetUnitDefinition(targetDefinitionId);

            bool canTargetBeTargeted = targetDefinition.CanBeTargeted;
            if (!canTargetBeTargeted)
                return false;

            bool canHitSelf = hitRules.CanHitSelf;
            if (user == target)
                return canHitSelf;

            bool canHitAllies = hitRules.CanHitAllies;
            bool canHitEnemies = hitRules.CanHitEnemies;
            bool canHitNeutral = hitRules.CanHitNeutral;
            TeamAffiliation userAffiliation = user.CurrentAffiliation;
            TeamAffiliation targetAffiliation = target.CurrentAffiliation;
            TeamRelation relation = userAffiliation.DetermineRelation(targetAffiliation);
            return relation switch
            {
                TeamRelation.Friendly => canHitAllies,
                TeamRelation.Hostile => canHitEnemies,
                TeamRelation.Neutral => canHitNeutral,
                _ => throw new NotImplementedException($"Unhandled {nameof(TeamRelation)}: {relation}")
            };
        }
    }
}