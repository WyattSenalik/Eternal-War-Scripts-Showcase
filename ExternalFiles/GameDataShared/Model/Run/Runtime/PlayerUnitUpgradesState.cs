// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;

namespace EternalWar.Core.Model.Run.Runtime
{
    // TODO: Hold artifact state instead of just artifact ids?
    public sealed record PlayerUnitUpgradesState(CombatUnitUniqueIdentifier PlayerUnitIdentifier, PlayerStatUpgradesState StatUpgrades, ValueArray<string> ObtainedArtifactIds, ValueArray<string> ClassPrestigeChoices);

    public sealed record PlayerStatUpgradesState(int Health, int Strength, int Defense, int Speed, int Magic, int Will)
    {
        public static readonly PlayerStatUpgradesState Empty = new(0, 0, 0, 0, 0, 0);
    }

    public static class PlayerUnitUpgradesStateExtensions
    {
        public static string GetCurrentClassUnitDefinitionId(this PlayerUnitUpgradesState upgradesState)
        {
            ValueArray<string> classPrestigeChoices = upgradesState.ClassPrestigeChoices;
            if (classPrestigeChoices.Count <= 0)
            {
                CombatUnitUniqueIdentifier unitIdentifier = upgradesState.PlayerUnitIdentifier;
                return unitIdentifier.DefinitionId;
            }
            return classPrestigeChoices[^1];
        }
    }

    public static class PlayerStatUpgradesStateExtensions
    {
        public static CombatStats CreateStatChanges(this PlayerStatUpgradesState statUpgrades)
        {
            statUpgrades.Deconstruct(out int health, out int strength, out int defense, out int speed, out int magic, out int will);

            HealthInfo healthInfo = new(HasInfiniteHealth: false, health);
            AttributeStats attributes = new(strength, defense, speed, magic, will, Evasion: 0, Accuracy: 0);
            return new CombatStats(healthInfo, attributes, Movement: 0, MaxActionPoints: 0);
        }
    }
}