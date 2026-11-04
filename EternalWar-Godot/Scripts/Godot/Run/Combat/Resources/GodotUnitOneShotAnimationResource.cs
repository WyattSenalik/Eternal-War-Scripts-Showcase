// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Engine.Godot.Resources;
using Godot;
using System;

namespace EternalWar.Engine.Godot.Run.Combat.Resources
{
    [GlobalClass]
    public sealed partial class GodotUnitOneShotAnimationResource : Resource, IHaveActionSequenceTimingSpecification
    {
        public GodotStringPoolResource? AnimationPool => _animationPool;
        public float? AdvanceTimeOverride => _shouldOverrideAdvanceTime ? _advanceTimeOverride : null;
        public UnitOneShotAnimationType OneShotType => _oneShotType;

        public bool IsForSpecificLoop => _isForSpecificLoop;
        public GodotUnitLoopAnimationResource? SpecificLoopAnimationResource => _specificLoopAnimationResource;

        public string? ActionSequenceId => _isForSpecificActionSequence ? _actionSequenceId : null;
        public int? StepIndex => _isForSpecificActionSequence && _isForSpecificActionSequenceStep ? _step : null;

        [Export] private GodotStringPoolResource? _animationPool;
        [Export] private bool _shouldOverrideAdvanceTime;
        [Export] private float _advanceTimeOverride;
        [Export] private UnitOneShotAnimationType _oneShotType;

        [Export] private bool _isForSpecificLoop;
        [Export] private GodotUnitLoopAnimationResource? _specificLoopAnimationResource;

        [Export] private bool _isForSpecificActionSequence;
        [Export] private string? _actionSequenceId;
        [Export] private bool _isForSpecificActionSequenceStep;
        [Export] private int _step;
    }

    public static class GodotUnitOneShotAnimationResourceExtensions
    {
        private const int MAX_DIFFERENCE_VALUE = 100;

        public static string RollAnimation(this GodotUnitOneShotAnimationResource resource, GodotEngineRandomResolver engineRandomResolver)
        {
            GodotStringPoolResource? animationPool = resource.AnimationPool;
            if (animationPool == null)
                throw new Exception($"Unspecified animation pool on resource: {resource.ResourcePath}");
            return animationPool.RollFromPool(engineRandomResolver);
        }

        public static bool IsSame(this GodotUnitOneShotAnimationResource resource, UnitLoopAnimationRequest activeLoopRequest, UnitOneShotAnimationRequest animationRequest) => resource.DetermineSimilarity(activeLoopRequest, animationRequest) == MAX_DIFFERENCE_VALUE;

        public static int DetermineDifference(this GodotUnitOneShotAnimationResource resource, UnitLoopAnimationRequest activeLoopRequest, UnitOneShotAnimationRequest animationRequest) => MAX_DIFFERENCE_VALUE - resource.DetermineSimilarity(activeLoopRequest, animationRequest);

        public static int DetermineSimilarity(this GodotUnitOneShotAnimationResource resource, UnitLoopAnimationRequest activeLoopRequest, UnitOneShotAnimationRequest animationRequest)
        {
            if (resource.OneShotType != animationRequest.AnimationType)
                return 0;

            int loopDifference;
            if (resource.IsForSpecificLoop)
            {
                ArgumentNullException.ThrowIfNull(resource.SpecificLoopAnimationResource, nameof(resource.SpecificLoopAnimationResource));
                loopDifference = resource.SpecificLoopAnimationResource.DetermineDifference(activeLoopRequest);
            }
            else
            {
                loopDifference = 10;
            }
            int oneShotDifference = resource.DetermineDifference(animationRequest.ActionTimingContext);
            int totalDifference = loopDifference + oneShotDifference;

            return MAX_DIFFERENCE_VALUE - totalDifference;
        }
    }
}
