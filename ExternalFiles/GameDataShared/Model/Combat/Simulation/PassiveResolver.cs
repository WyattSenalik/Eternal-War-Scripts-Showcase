// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Helpers.Pooling;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Common.Stats;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Combat.Simulation.Handlers;
using EternalWar.Core.Model.Combat.Simulation.Handlers.Passive.Action;
using EternalWar.Core.Model.Combat.Simulation.Handlers.Passive.Condition;
using EternalWar.Core.Model.Combat.Simulation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Model.Combat.Simulation
{
    internal sealed class PassiveResolver(CombatCommandProcessor processor) : ICombatEventObserver, IDisposable
    {
        private GameDatabase Database => _processor.Database;
        private CombatInstance CombatInstance => _processor.CombatInstance;
        private CombatState CurrentState => _processor.CurrentState;

        private readonly CombatCommandProcessor _processor = processor;
        private readonly PooledHashSet<(CombatUnitUniqueIdentifier OwnerIdentifier, string PassiveId)> _currentlyActivating = new();

        public void HandleEventRecorded(CombatEvent combatEvent)
        {
            foreach (CombatUnitUniqueIdentifier unitIdentifier in CurrentState.UnitMap.Keys)
            {
                ResolvePassivesForUnit(combatEvent, unitIdentifier);
            }
        }

        public void Dispose()
        {
            _currentlyActivating.Dispose();
        }

        private void ResolvePassivesForUnit(CombatEvent combatEvent, CombatUnitUniqueIdentifier unitIdentifier)
        {
            UnitState initialUnit = CurrentState.GetUnit(unitIdentifier);
            ValueArray<PassiveState> passives = initialUnit.ActivePassives;

            using PooledList<PassiveState> sortedPassives = new(passives);
            sortedPassives.Sort(_ComparePriorities);

            foreach (PassiveState passive in sortedPassives)
            {
                string passiveId = passive.PassiveDefinitionId;

                UnitState currentUnit = CurrentState.GetUnit(unitIdentifier);
                int passiveIndex = currentUnit.ActivePassives.FindIndexOfPassive(passiveId);
                // An earlier passive may have removed this passive
                if (passiveIndex < 0)
                    continue;

                ResolvePassiveActivation(combatEvent, unitIdentifier, passiveId);
                ResolvePassiveExpiration(combatEvent, unitIdentifier, passiveId);
            }

            int _ComparePriorities(PassiveState a, PassiveState b)
            {
                PassiveDefinition aDefinition = Database.GetPassiveDefinition(a.PassiveDefinitionId);
                PassiveDefinition bDefinition = Database.GetPassiveDefinition(b.PassiveDefinitionId);
                // Descending order (so b vs a instead of a vs b)
                return Comparer<int>.Default.Compare(bDefinition.Priority, aDefinition.Priority);
            }
        }

        private void ResolvePassiveActivation(CombatEvent combatEvent, CombatUnitUniqueIdentifier unitIdentifier, string passiveId)
        {
            PassiveDefinition passiveDefinition = Database.GetPassiveDefinition(passiveId);
            PassiveActivateTime activateTime = passiveDefinition.ActivateTime;

            UnitState unit = CurrentState.GetUnit(unitIdentifier);
            PassiveState passive = unit.ActivePassives.FindPassive(passiveId);
            if (unit.IsDead() && !passiveDefinition.CanActivateWhenDead)
                return;
            if (passiveDefinition.HasActivationLimitPerEncounter && passive.Activations >= passiveDefinition.MaxActivationsPerEncounter)
                return;

            // Translate events into owner and source
            CombatUnitUniqueIdentifier ownerIdentifier;
            CombatUnitUniqueIdentifier sourceIdentifier;
            switch (combatEvent)
            {
                case CombatEvent_DamageApplied damageAppliedEvent:
                    if (activateTime != PassiveActivateTime.OnHit)
                        return;
                    ownerIdentifier = damageAppliedEvent.TargetIdentifier;
                    sourceIdentifier = damageAppliedEvent.SourceIdentifier;
                    break;
                case CombatEvent_UnitTurnStarted turnStartedEvent:
                    if (activateTime != PassiveActivateTime.TurnStart)
                        return;
                    ownerIdentifier = turnStartedEvent.UnitIdentifier;
                    sourceIdentifier = turnStartedEvent.UnitIdentifier;
                    break;
                case CombatEvent_UnitTurnEnded turnEndedEvent:
                    if (activateTime != PassiveActivateTime.TurnEnd)
                        return;
                    ownerIdentifier = turnEndedEvent.UnitIdentifier;
                    sourceIdentifier = turnEndedEvent.UnitIdentifier;
                    break;
                case CombatEvent_PassiveAdded passiveAddedEvent:
                    if (activateTime != PassiveActivateTime.OnApply)
                        return;
                    if (passiveAddedEvent.PassiveId != passiveId)
                        return;
                    ownerIdentifier = passiveAddedEvent.TargetIdentifier;
                    sourceIdentifier = passiveAddedEvent.SourceIdentifier;
                    break;
                default:
                    return;
            }
            // Check if real owner (unit) is owner from event, if not, we're done.
            if (unitIdentifier != ownerIdentifier)
                return;

            // Check if the condition is satisfied
            PassiveConditionHandlerContext conditionContext = new(passiveDefinition.ConditionInfo, CurrentState, ownerIdentifier, combatEvent, sourceIdentifier);
            IPassiveConditionHandler conditionHandler = CombatHandlersRegistration.GetHandler(passiveDefinition.ConditionInfo);
            if (!conditionHandler.DoesSatisfy(conditionContext))
                return;

            // Check if its already activating
            (CombatUnitUniqueIdentifier OwnerIdentifier, string PassiveId) key = new(ownerIdentifier, passiveId);
            if (!_currentlyActivating.Add(key))
                return;

            try
            {
                // Apply the command.
                CombatCommand_ActivatePassive passiveActivatedCommand = new(ownerIdentifier, sourceIdentifier, passiveId, activateTime, passive with { Activations = passive.Activations + 1 });
                ValueArray<CombatEvent> activationEvents = _processor.Apply(passiveActivatedCommand);
                CombatEvent_PassiveActivated activationEvent = (CombatEvent_PassiveActivated)activationEvents[0];

                // Execute the activation
                PassiveActionHandlerContext actionContext = new(_processor, ownerIdentifier, sourceIdentifier, PassiveStacks: passive.Stacks);
                PassiveActionInfo passiveActionInfo = passiveDefinition.ActionInfo;
                IPassiveActionHandler actionHandler = CombatHandlersRegistration.GetHandler(passiveActionInfo);
                actionHandler.Execute(actionContext, passiveActionInfo);

                // Resolve passive-specific expiration after the activation action and its reactions.
                ResolvePassiveExpiration(activationEvent, unitIdentifier, passiveId, afterActivationActionCompleted: true);
            }
            finally
            {
                _currentlyActivating.Remove(key);
            }
        }

        private void ResolvePassiveExpiration(CombatEvent combatEvent, CombatUnitUniqueIdentifier unitIdentifier, string passiveId, bool afterActivationActionCompleted = false)
        {
            PassiveDefinition passiveDefinition = Database.GetPassiveDefinition(passiveId);
            ExpirationInfoGroup stackReductionExpirationInfo = passiveDefinition.StackReductionExpirationInfo;
            ValueArray<ExpirationInfo> expirationInfos = stackReductionExpirationInfo.Group;

            // Figure out if the event is capable of effecting expriation and how
            ExpirationTime[] expirationTimesFromEvent = DetermineExpirationTimeFromEvent(combatEvent, passiveId, out CombatUnitUniqueIdentifier? ownerIdentifier, out CombatUnitUniqueIdentifier? sourceIdentifier, afterActivationActionCompleted);
            if (expirationTimesFromEvent.Length <= 0 || ownerIdentifier == null || sourceIdentifier == null || ownerIdentifier != unitIdentifier)
                return;

            // The activation action can remove the passive before its deferred expiration is resolved.
            UnitState unit = CurrentState.GetUnit(unitIdentifier);
            ValueArray<PassiveState> activePassives = unit.ActivePassives;
            int passiveIndex = activePassives.FindIndexOfPassive(passiveId);
            if (passiveIndex < 0)
                return;

            for (int ruleIndex = 0; ruleIndex < expirationInfos.Count; ++ruleIndex)
            {
                // Check if this rule is one that will expire with the event
                ExpirationInfo currentRule = expirationInfos[ruleIndex];
                if (!expirationTimesFromEvent.Contains(currentRule.ExpirationTime))
                    continue;

                unit = CurrentState.GetUnit(unitIdentifier);
                activePassives = unit.ActivePassives;
                PassiveState passive = activePassives.FindPassive(passiveId);

                // Increment the occurrences of the rule
                int nextOccurrenceCount = GetOccurrenceCount(passive, ruleIndex) + 1;
                int requiredOccurrences = Math.Max(1, currentRule.OccurrencesBeforeExpiration);
                if (nextOccurrenceCount < requiredOccurrences)
                {
                    PassiveState progressedPassive = passive with
                    {
                        ExpirationProgresses = WithOccurrenceCount(passive.ExpirationProgresses, ruleIndex, nextOccurrenceCount)
                    };
                    CombatCommand_ProgressPassiveExpiration passiveExpirationProgressedCommand = new(ownerIdentifier, passiveId, progressedPassive);
                    _processor.Apply(passiveExpirationProgressedCommand);
                    continue;
                }

                // Need to expire the passive
                CombatStats ownerStats = CombatStatHelpers.CalculateStats(Database, CombatInstance, CurrentState, ownerIdentifier);
                int currentPassiveStacks = passive.Stacks;
                ValueFunctionContext valueFunctionContext = new(ownerStats, currentPassiveStacks);
                NCalcFunctionInfo stackReductionFunction = passiveDefinition.StackReductionNCalcFunction;
                float stacksToReduceRaw = stackReductionFunction.Evaluate(valueFunctionContext);
                int stacksToReduce = Math.Clamp((int)MathF.Floor(stacksToReduceRaw), 0, currentPassiveStacks);
                ValueArray<PassiveExpirationProgress> resetProgresses = WithOccurrenceCount(passive.ExpirationProgresses, ruleIndex, 0);

                // Usually would post stacks reduced, but if there is no stack reduction, just post the expiration.
                if (stacksToReduce == 0)
                {
                    PassiveState progressedPassive = passive with
                    {
                        ExpirationProgresses = resetProgresses
                    };
                    CombatCommand_ProgressPassiveExpiration passiveExpirationProgressedCommand = new(ownerIdentifier, passiveId, progressedPassive);
                    _processor.Apply(passiveExpirationProgressedCommand);
                    continue;
                }

                // Post stack reduction
                int resultingStacks = passive.Stacks - stacksToReduce;
                PassiveState? resultingPassive = resultingStacks > 0
                    ? passive with
                    {
                        Stacks = resultingStacks,
                        ExpirationProgresses = resetProgresses
                    } : null;
                CombatCommand_ReducePassiveStacks stacksReducedCommand = new(ownerIdentifier, passiveId, stacksToReduce, resultingPassive);
                _processor.Apply(stacksReducedCommand);

                // Handle the passive expirting action if it has one
                if (resultingPassive == null)
                {
                    ExecuteExpirationAction(ownerIdentifier, sourceIdentifier, passiveDefinition.SeparateExpirationActionInfo);
                    break;
                }
            }
        }

        private void ExecuteExpirationAction(CombatUnitUniqueIdentifier ownerIdentifier, CombatUnitUniqueIdentifier sourceIdentifier, PassiveActionInfo actionInfo)
        {
            PassiveActionHandlerContext actionContext = new(_processor, ownerIdentifier, sourceIdentifier);
            IPassiveActionHandler actionHandler = CombatHandlersRegistration.GetHandler(actionInfo);
            actionHandler.Execute(actionContext, actionInfo);
        }

        private static ExpirationTime[] DetermineExpirationTimeFromEvent(CombatEvent combatEvent, string passiveId, out CombatUnitUniqueIdentifier? ownerIdentifier, out CombatUnitUniqueIdentifier? sourceIdentifier, bool afterActivationActionCompleted)
        {
            switch (combatEvent)
            {
                case CombatEvent_UnitTurnEnded unitTurnEnded:
                    ownerIdentifier = unitTurnEnded.UnitIdentifier;
                    sourceIdentifier = unitTurnEnded.UnitIdentifier;
                    return _One(ExpirationTime.TurnEnd);

                case CombatEvent_UnitTurnStarted unitTurnStarted:
                    ownerIdentifier = unitTurnStarted.UnitIdentifier;
                    sourceIdentifier = unitTurnStarted.UnitIdentifier;
                    return _One(ExpirationTime.TurnStart);

                case CombatEvent_Moved moved:
                    ownerIdentifier = moved.EffectedUnitIdentifier;
                    sourceIdentifier = moved.UserIdentifier;
                    return _Moved(moved);

                case CombatEvent_DamageApplied damagedApplied:
                    ownerIdentifier = damagedApplied.TargetIdentifier;
                    sourceIdentifier = damagedApplied.SourceIdentifier;
                    return _One(ExpirationTime.OnHit);

                case CombatEvent_PassiveActivated passiveActivated:
                    ownerIdentifier = passiveActivated.OwnerIdentifier;
                    sourceIdentifier = passiveActivated.SourceIdentifier;
                    if (!afterActivationActionCompleted || passiveActivated.PassiveId != passiveId)
                        return Array.Empty<ExpirationTime>();
                    return _One(ExpirationTime.AfterActivate_PassiveSpecific);

                default:
                    ownerIdentifier = null;
                    sourceIdentifier = null;
                    return Array.Empty<ExpirationTime>();
            }

            static ExpirationTime[] _Moved(CombatEvent_Moved movedEvent)
            {
                return movedEvent.MovementType switch
                {
                    MovementType.Walk => _One(ExpirationTime.SingleTileMoved),
                    MovementType.Push => movedEvent.IsFirstMove
                    ? new ExpirationTime[] { ExpirationTime.OnPushed, ExpirationTime.SingleTileMoved }
                    : _One(ExpirationTime.SingleTileMoved),
                    _ => Array.Empty<ExpirationTime>()
                };
            }

            static ExpirationTime[] _One(ExpirationTime time) => new ExpirationTime[] { time };
        }

        private static int GetOccurrenceCount(PassiveState passiveState, int ruleIndex)
        {
            foreach (PassiveExpirationProgress progress in passiveState.ExpirationProgresses)
            {
                if (progress.RuleIndex == ruleIndex)
                    return progress.Occurrences;
            }
            return 0;
        }

        private static ValueArray<PassiveExpirationProgress> WithOccurrenceCount(ValueArray<PassiveExpirationProgress> progressArray, int ruleIndex, int occurrences)
        {
            int existingIndex = -1;
            for (int i = 0; i < progressArray.Count; ++i)
            {
                if (progressArray[i].RuleIndex == ruleIndex)
                {
                    existingIndex = i;
                    break;
                }
            }

            if (occurrences <= 0)
                return existingIndex < 0 ? progressArray : progressArray.RemoveAt(existingIndex);

            PassiveExpirationProgress progress = new(ruleIndex, occurrences);
            return existingIndex < 0 ? progressArray.Add(progress) : progressArray.ReplaceAt(existingIndex, progress);
        }
    }
}

