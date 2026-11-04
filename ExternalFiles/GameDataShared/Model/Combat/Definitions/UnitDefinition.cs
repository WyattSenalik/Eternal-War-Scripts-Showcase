// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common.Stats;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public sealed record UnitDefinition(string Id, HealthInfo BaseHealth, AttributeStats BaseAttributes, int BaseMovement, int BaseActionPoints, TeamAffiliation StartingAffiliation, bool CanBeRevived, bool ShouldTakeATurn, bool ShouldBlockOccupiedTile, bool CanBeMoved, bool CanBeTargeted, string MoveCombatActionId, ValueArray<string> CombatActionIds, ValueArray<string> StartingPassiveIds) : IHaveStringId
    {
        public UnitDecisionControllerInfo DecisionControllerInfo { get; init; } = new UnitDecisionControllerInfo_None();
    }

    public enum TeamAffiliation
    {
        Enemy,
        Ally,
        Neutral
    }

    public enum TeamRelation
    {
        Friendly = 0,
        Hostile = 1,
        Neutral = 2
    }

    public static class UnitDefinitionExtensions
    {
        public static CombatStats GetBaseStats(this UnitDefinition unitDefinition)
        {
            return new CombatStats(unitDefinition.BaseHealth, unitDefinition.BaseAttributes, unitDefinition.BaseMovement, unitDefinition.BaseActionPoints);
        }
    }

    public static class TeamAffiliationExtensions
    {
        public static TeamRelation DetermineRelation(this TeamAffiliation thisAffiliation, TeamAffiliation otherAffiliation)
        {
            if (thisAffiliation == otherAffiliation)
                return TeamRelation.Friendly;
            if (thisAffiliation == TeamAffiliation.Neutral || otherAffiliation == TeamAffiliation.Neutral)
                return TeamRelation.Neutral;
            return TeamRelation.Hostile;
        }
    }
}
