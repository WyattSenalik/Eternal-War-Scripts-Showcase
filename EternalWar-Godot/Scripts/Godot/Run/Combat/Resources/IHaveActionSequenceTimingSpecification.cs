// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Simulation.Events;

namespace EternalWar.Engine.Godot.Run.Combat.Resources
{
    public interface IHaveActionSequenceTimingSpecification
    {
        public string? ActionSequenceId { get; }
        public int? StepIndex { get; }
    }

    public static class IHaveActionSequenceTimingSpecificationExtensions
    {
        private const int MAX_DIFFERENCE_VALUE = 100;

        public static bool IsSame(this IHaveActionSequenceTimingSpecification specification, ActionSequenceTimingContext? actionTimingContext) => specification.DetermineSimilarity(actionTimingContext) == MAX_DIFFERENCE_VALUE;

        public static int DetermineDifference(this IHaveActionSequenceTimingSpecification specification, ActionSequenceTimingContext? actionTimingContext) => MAX_DIFFERENCE_VALUE - specification.DetermineSimilarity(actionTimingContext);

        public static int DetermineSimilarity(this IHaveActionSequenceTimingSpecification specification, ActionSequenceTimingContext? actionTimingContext)
        {
            if (specification.ActionSequenceId == null)
            {
                if (actionTimingContext == null)
                    return MAX_DIFFERENCE_VALUE; // Both have no specified action sequence timing
                return MAX_DIFFERENCE_VALUE - 2; // Timing context is valid, but this would be an ok fallback
            }
            // We don't want an animation specified for a specific action sequence playing for a generic request.
            if (actionTimingContext == null)
                return 0;

            // We don't want an animation specified for a specific action sequence playing for the wrong sequence.
            if (specification.ActionSequenceId != actionTimingContext.Id)
                return 0;

            if (specification.StepIndex == null)
                return MAX_DIFFERENCE_VALUE - 1; // Timing context has a step, but this would be an ok fallback

            // We don't want an animation for a specific step playing for the wrong step.
            if (specification.StepIndex != actionTimingContext.Step)
                return 0;

            // Everything matches up perfectly. Exact match.
            return MAX_DIFFERENCE_VALUE;
        }
    }
}
