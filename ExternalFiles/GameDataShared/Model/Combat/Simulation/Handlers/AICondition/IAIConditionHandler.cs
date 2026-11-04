// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Handlers.AITargetSelector;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AICondition
{
    public sealed record AIConditionHandlerContext(GameDatabase Database, CombatInstance CombatInstance, CombatState PreviousState, CombatSimulationResult CandidateSimulationResult, CombatUnitUniqueIdentifier UserIdentifier, CombatUnitUniqueIdentifier SubjectIdentifier);

    internal interface IAIConditionHandler
    {
        Type ConditionType { get; }

        bool DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo conditionInfo);
    }

    public abstract class AIConditionHandler<TConditionInfo> : IAIConditionHandler where TConditionInfo : AIConditionInfo
    {
        public Type ConditionType => typeof(TConditionInfo);

        bool IAIConditionHandler.DoesSatisfy(AIConditionHandlerContext context, AIConditionInfo conditionInfo)
        {
            return conditionInfo is TConditionInfo castedConditionInfo
                ? DoesSatisfy(context, castedConditionInfo)
                : throw new ArgumentException($"{GetType().Name} cannot evaluate a {conditionInfo.GetType().Name}");
        }

        protected abstract bool DoesSatisfy(AIConditionHandlerContext context, TConditionInfo conditionInfo);
    }

    public abstract class AIConditionHandler_HasSubject<TConditionInfo_HasSubject> : AIConditionHandler<TConditionInfo_HasSubject> where TConditionInfo_HasSubject : AIConditionInfo_HasSubject
    {
        protected sealed override bool DoesSatisfy(AIConditionHandlerContext context, TConditionInfo_HasSubject conditionInfo)
        {
            CombatUnitUniqueIdentifier? subjectIdentifier = GetSubjectIdentifier(conditionInfo, context);
            return subjectIdentifier != null && DoesSatisfy(context, conditionInfo, subjectIdentifier);
        }

        protected abstract bool DoesSatisfy(AIConditionHandlerContext context, TConditionInfo_HasSubject conditionInfo, CombatUnitUniqueIdentifier subjectIdentifier);

        private static CombatUnitUniqueIdentifier? GetSubjectIdentifier(AIConditionInfo_HasSubject hasSubjectConditionInfo, AIConditionHandlerContext context)
        {
            GameDatabase database = context.Database;
            CombatInstance combatInstance = context.CombatInstance;
            CombatState combatState = context.PreviousState;
            CombatUnitUniqueIdentifier userIdentifier = context.UserIdentifier;
            CombatUnitUniqueIdentifier originalSubjectIdentifier = context.SubjectIdentifier;

            AITargetSelectorInfo subjectSelectorInfo = hasSubjectConditionInfo.Subject;
            AITargetSelectionHandlerContext targetSelectionContext = new(database, combatInstance, combatState, userIdentifier, originalSubjectIdentifier);
            return subjectSelectorInfo.GetTargetIdentifier(targetSelectionContext);
        }
    }
}
