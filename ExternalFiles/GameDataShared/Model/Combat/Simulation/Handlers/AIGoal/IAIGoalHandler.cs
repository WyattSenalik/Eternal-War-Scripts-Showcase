// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Events;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.AIGoal
{
    internal sealed record AIGoalHandlerContext(CombatUnitUniqueIdentifier UserIdentifier, IAIGoalTargetLocator TargetLocator, ValueArray<CombatEvent> RelevantEvents, ValueArray<CombatEvent> CandidateEvents, CombatUnitUniqueIdentifier? CurrentSubjectIdentifier);

    internal sealed record EvaluationResult(bool DidSucceed, ValueArray<int> Heuristics) : IComparable<EvaluationResult>
    {
        public static readonly EvaluationResult Failure = new(false, ValueArray<int>.Empty);

        public EvaluationResult(ValueArray<int> heuristics) : this(true, heuristics) { }

        public int CompareTo(EvaluationResult? other)
        {
            if (other == null)
                return 1;
            if (DidSucceed && !other.DidSucceed)
                return 1;
            if (!DidSucceed && other.DidSucceed)
                return -1;

            ValueArray<int> otherHeuristics = other.Heuristics;
            int myCount = Heuristics.Count;
            int otherCount = otherHeuristics.Count;
            int smallerCount = Math.Min(myCount, otherCount);
            for (int i = 0; i < smallerCount; ++i)
            {
                int myH = Heuristics[i];
                int otherH = otherHeuristics[i];
                int comparison = myH.CompareTo(otherH);
                if (comparison == 0)
                    continue;

                return comparison;
            }

            return myCount.CompareTo(otherCount);
        }
    }

    internal interface IAIGoalHandler
    {
        Type GoalInfoType { get; }

        EvaluationResult Evaluate(AIGoalHandlerContext context, AIGoalInfo info);
    }

    internal abstract class AIGoalHandler<TGoalInfo> : IAIGoalHandler where TGoalInfo : AIGoalInfo
    {
        public Type GoalInfoType => typeof(TGoalInfo);

        EvaluationResult IAIGoalHandler.Evaluate(AIGoalHandlerContext context, AIGoalInfo goalInfo)
        {
            return goalInfo is TGoalInfo castedGoalInfo
                ? Evaluate(context, castedGoalInfo)
                : throw new ArgumentException($"{GetType().Name} cannot evaluate a {goalInfo.GetType().Name}");
        }

        protected abstract EvaluationResult Evaluate(AIGoalHandlerContext context, TGoalInfo goalInfo);
    }
}
