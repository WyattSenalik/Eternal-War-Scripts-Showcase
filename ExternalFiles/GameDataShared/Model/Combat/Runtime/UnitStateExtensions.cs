// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Runtime
{
    public static class UnitStateExtensions
    {
        public static bool HasPassive(this UnitState unit, string passiveId)
        {
            ValueArray<PassiveState> activePassives = unit.ActivePassives;
            foreach (PassiveState passive in activePassives)
            {
                string candidatePassiveId = passive.PassiveDefinitionId;
                if (candidatePassiveId == passiveId)
                    return true;
            }

            return false;
        }

        public static bool HasNonStackableEffect(this UnitState unit, string effectId)
        {
            ValueArray<string> activeEffectIds = unit.ActiveNonStackableEffectIds;
            return activeEffectIds.Contains(effectId);
        }

        public static bool HasSkipTurnNonStackableEffect(this UnitState unit, GameDatabase database)
        {
            ValueArray<string> activeEffectIds = unit.ActiveNonStackableEffectIds;
            foreach (string effectId in activeEffectIds)
            {
                NonStackableEffectDefinition effectDefinition = database.GetNonStackableEffectDefinition(effectId);
                if (effectDefinition.Info is NonStackableEffectInfo_SkipTurn)
                    return true;
            }
            return false;
        }

        public static bool IsDead(this UnitState unit)
        {
            return unit.CurrentHealth <= 0;
        }
    }
}