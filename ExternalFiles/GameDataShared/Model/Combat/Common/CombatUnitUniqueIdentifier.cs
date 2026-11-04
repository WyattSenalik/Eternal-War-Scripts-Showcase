// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;

namespace EternalWar.Core.Model.Combat.Common
{
    public sealed record CombatUnitUniqueIdentifier(CombatUnitOwner Owner, string DefinitionId, int Index)
    {
        public override string ToString()
        {
            string ownerString;
            if (Owner is CombatUnitOwner_AI)
                ownerString = "AI";
            else if (Owner is CombatUnitOwner_Player playerOwner)
                ownerString = $"Player {playerOwner.PlayerClient.PlayerIndex}";
            else
                ownerString = "???";
            
            return $"{ownerString} {DefinitionId}{(Index == 0 ? string.Empty : $" #{Index}")}";
        }
    }

    public static class CombatUnitUniqueIdentifierExtensions
    {
        public static UnitDefinition GetUnitDefinition(this GameDatabase database, CombatUnitUniqueIdentifier unitIdentifier)
        {
            string unitId = unitIdentifier.DefinitionId;
            return database.GetUnitDefinition(unitId);
        }
    }
}