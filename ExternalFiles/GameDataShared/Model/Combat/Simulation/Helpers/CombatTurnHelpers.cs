// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Helpers
{
    public static class CombatTurnHelpers
    {
        public static bool IsItClientsTurn(GameDatabase database, CombatRuntime combatRuntime, ClientInfo clientInfo)
        {
            CombatInstance combatInstance = combatRuntime.Instance;
            CombatState combatState = combatRuntime.State;

            TurnState turnState = DeriveTurnState(database, combatInstance, combatState);
            CombatUnitUniqueIdentifier? currentTurnUnitIdentifier = turnState.CurrentTurnUnitIdentifier;
            return currentTurnUnitIdentifier != null && CombatUnitOwnershipHelpers.DoesClientOwnUnit(clientInfo, currentTurnUnitIdentifier);
        }

        public static TurnState DeriveInitialTurnState(GameDatabase database, CombatInstance combatInstance)
        {
            ValueArray<UnitTurnState> turnOrder = DetermineInitialTurnOrder(database, combatInstance);
            CombatUnitUniqueIdentifier? currentTurnUnitIdentifier = turnOrder.Count > 0 ? turnOrder[0].Identifier : null;
            return new TurnState(turnOrder, currentTurnUnitIdentifier);
        }

        public static TurnState DeriveTurnState(GameDatabase database, CombatRuntime combatRuntime) => DeriveTurnState(database, combatRuntime.Instance, combatRuntime.State);
        public static TurnState DeriveTurnState(GameDatabase database, CombatInstance combatInstance, CombatState combatState)
        {
            CombatUnitUniqueIdentifier? currentTurnUnitIdentifier = null;
            ValueArray<UnitTurnState> turnOrder = DetermineTurnOrder(database, combatInstance, combatState);
            foreach (UnitTurnState unitTurnState in turnOrder)
            {
                CombatUnitUniqueIdentifier unitIdentifier = unitTurnState.Identifier;
                UnitState unit = combatState.GetUnit(unitIdentifier);
                if (unit.HasTakenTurn || unit.HasSkipTurnNonStackableEffect(database))
                    continue;
                currentTurnUnitIdentifier = unitIdentifier;
                break;
            }
            return new TurnState(turnOrder, currentTurnUnitIdentifier);
        }

        private static ValueArray<UnitTurnState> DetermineInitialTurnOrder(GameDatabase database, CombatInstance combatInstance)
        {
            using PooledList<UnitTurnState> turnOrder = new();
            ValueDictionary<CombatUnitUniqueIdentifier, UnitBuild> unitMap = combatInstance.UnitMap;
            foreach ((CombatUnitUniqueIdentifier unitIdentifier, UnitBuild unitBuild) in unitMap)
            {
                if (!_CanUnitTakeTurn(unitIdentifier))
                    continue;
                UnitTurnState unitTurnState = new(unitIdentifier, ShouldSkipTurn: false);
                turnOrder.Add(unitTurnState);
            }
            SortInitialUnitsBySpeed(database, combatInstance, turnOrder);
            return turnOrder.ToValueArray();

            bool _CanUnitTakeTurn(CombatUnitUniqueIdentifier unitIdentifier)
            {
                UnitDefinition unitDefinition = database.GetUnitDefinition(unitIdentifier);
                return unitDefinition.ShouldTakeATurn;
            }
        }

        private static ValueArray<UnitTurnState> DetermineTurnOrder(GameDatabase database, CombatInstance combatInstance, CombatState combatState)
        {
            using PooledList<UnitTurnState> turnOrder = new();
            ValueDictionary<CombatUnitUniqueIdentifier, UnitState> unitMap = combatState.UnitMap;
            foreach ((CombatUnitUniqueIdentifier unitIdentifier, UnitState unit) in unitMap)
            {
                UnitDefinition unitDefinition = database.GetUnitDefinition(unitIdentifier);
                if (!unitDefinition.ShouldTakeATurn)
                    continue;

                UnitTurnState unitTurnState = new(unitIdentifier, ShouldSkipTurn: unit.IsDead());
                turnOrder.Add(unitTurnState);
            }
            SortUnitsBySpeed(database, combatInstance, combatState, turnOrder);
            return turnOrder.ToValueArray();
        }

        private static void SortInitialUnitsBySpeed(GameDatabase database, CombatInstance combatInstance, PooledList<UnitTurnState> unitTurnStatesList)
        {
            unitTurnStatesList.Sort(_CompareTo);

            int _CompareTo(UnitTurnState a, UnitTurnState b)
            {
                CombatStats aStats = CombatStatHelpers.CalculateInitialStats(database, combatInstance, a.Identifier);
                CombatStats bStats = CombatStatHelpers.CalculateInitialStats(database, combatInstance, b.Identifier);

                return TryCompareSpeedStats(aStats, bStats, out int comparison) ? comparison
                    : CompareIdentifiers(a.Identifier, b.Identifier);
            }
        }

        private static void SortUnitsBySpeed(GameDatabase database, CombatInstance combatInstance, CombatState combatState, PooledList<UnitTurnState> unitTurnStatesList)
        {
            unitTurnStatesList.Sort(_CompareTo);

            int _CompareTo(UnitTurnState a, UnitTurnState b)
            {
                CombatStats aStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, a.Identifier);
                CombatStats bStats = CombatStatHelpers.CalculateStats(database, combatInstance, combatState, b.Identifier);

                return TryCompareSpeedStats(aStats, bStats, out int comparison) ? comparison
                    : CompareIdentifiers(a.Identifier, b.Identifier);
            }
        }

        private static bool TryCompareSpeedStats(CombatStats a, CombatStats b, out int comparison)
        {
            int aSpeed = a.Get(StatType.Speed);
            int bSpeed = b.Get(StatType.Speed);

            comparison = bSpeed.CompareTo(aSpeed);
            return comparison != 0;
        }

        /// <summary>Should never return 0. These are supposed to be unique after all.</summary>
        private static int CompareIdentifiers(CombatUnitUniqueIdentifier a, CombatUnitUniqueIdentifier b)
        {
            CombatUnitOwner aOwner = a.Owner;
            CombatUnitOwner bOwner = b.Owner;
            if (TryCompareOwner(aOwner, bOwner, out int comparison))
                return comparison;

            string aId = a.DefinitionId;
            string bId = b.DefinitionId;
            if (aId != bId)
                return string.Compare(bId, aId, StringComparison.Ordinal);

            int aIndex = a.Index;
            int bIndex = b.Index;
            int returnValue = aIndex.CompareTo(bIndex);
            return returnValue != 0 ? returnValue : throw new Exception($"2 unit identifiers were identical ({a}) and ({b})");
        }

        private static bool TryCompareOwner(CombatUnitOwner a, CombatUnitOwner b, out int comparison)
        {
            comparison = a switch
            {
                CombatUnitOwner_Player aPlayer => b switch
                {
                    CombatUnitOwner_Player bPlayer => _ComparePlayers(aPlayer, bPlayer),
                    CombatUnitOwner_AI => -1, // Player always goes before AI in case of a tie
                    _ => throw new NotImplementedException($"Unhandled {nameof(CombatUnitOwner)}: ({b.GetType()})")
                },
                CombatUnitOwner_AI aAI => b switch
                {
                    CombatUnitOwner_Player => 1, // Player always goes before AI in case of a tie
                    CombatUnitOwner_AI bAI => 0, // All AI are the same
                    _ => throw new NotImplementedException($"Unhandled {nameof(CombatUnitOwner)}: ({b.GetType()})")
                },
                _ => throw new NotImplementedException($"Unhandled {nameof(CombatUnitOwner)}: ({a.GetType()})")
            };

            return comparison != 0;


            static int _ComparePlayers(CombatUnitOwner_Player aPlayer, CombatUnitOwner_Player bPlayer)
            {
                ClientInfo aClient = aPlayer.PlayerClient;
                ClientInfo bClient = bPlayer.PlayerClient;
                if (aClient == bClient)
                    return 0;
                int aIndex = aClient.PlayerIndex;
                int bIndex = bClient.PlayerIndex;
                return aIndex != bIndex ? bIndex.CompareTo(aIndex) : throw new Exception($"2 different clients have the same player index: ({aClient}) and ({bClient})");
            }
        }
    }
}
