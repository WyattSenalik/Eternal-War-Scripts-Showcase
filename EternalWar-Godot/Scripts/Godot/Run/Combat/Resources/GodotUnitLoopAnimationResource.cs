// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using Godot;

namespace EternalWar.Engine.Godot.Run.Combat.Resources
{
    [GlobalClass]
    public sealed partial class GodotUnitLoopAnimationResource : Resource, IHaveActionSequenceTimingSpecification
    {
        public string AnimationName => _animationName ?? string.Empty;
        public UnitLoopAnimationType LoopType => _loopType;
        public string? ActionSequenceId => _isForSpecificActionSequence ? _actionSequenceId : null;
        public int? StepIndex => _isForSpecificActionSequence && _isForSpecificActionSequenceStep ? _step : null;

        [Export] private string? _animationName;
        [Export] private UnitLoopAnimationType _loopType;

        [Export] private bool _isForSpecificActionSequence;
        [Export] private string? _actionSequenceId;
        [Export] private bool _isForSpecificActionSequenceStep;
        [Export] private int _step;
    }

    public static class GodotUnitLoopAnimationResourceExtensions
    {
        private const int MAX_DIFFERENCE_VALUE = 100;

        public static bool IsSame(this GodotUnitLoopAnimationResource resource, UnitLoopAnimationRequest animationRequest) => resource.DetermineSimilarity(animationRequest) == MAX_DIFFERENCE_VALUE;

        public static int DetermineDifference(this GodotUnitLoopAnimationResource resource, UnitLoopAnimationRequest animationRequest) => MAX_DIFFERENCE_VALUE - resource.DetermineSimilarity(animationRequest);

        public static int DetermineSimilarity(this GodotUnitLoopAnimationResource resource, UnitLoopAnimationRequest animationRequest)
        {
            if (resource.LoopType != animationRequest.AnimationType)
            {
                if (resource.LoopType == UnitLoopAnimationType.Idle)
                    return 1; // Weight idle slightly heigher so it's prefered when the requesated loop doesn't exist in the list at all.
                return 0;
            }

            return resource.DetermineSimilarity(animationRequest.ActionTimingContext);
        }
    }
}
