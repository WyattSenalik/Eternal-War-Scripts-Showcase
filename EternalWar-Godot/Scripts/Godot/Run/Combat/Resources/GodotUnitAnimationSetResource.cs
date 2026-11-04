// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EternalWar.Engine.Godot.Run.Combat.Resources
{
    [GlobalClass]
    public sealed partial class GodotUnitAnimationSetResource : Resource
    {
        public IReadOnlyList<GodotUnitLoopAnimationResource> Loops => _loops;
        public IReadOnlyList<GodotUnitOneShotAnimationResource> OneShots => _oneShots;

        [Export] private GodotUnitLoopAnimationResource[] _loops = Array.Empty<GodotUnitLoopAnimationResource>();
        [Export] private GodotUnitOneShotAnimationResource[] _oneShots = Array.Empty<GodotUnitOneShotAnimationResource>();
    }

    public static class GodotUnitAnimationSetResourceExtensions
    {
        public static bool TryFindBestFit(this GodotUnitAnimationSetResource setResource, UnitLoopAnimationRequest loopRequest, [NotNullWhen(true)] out GodotUnitLoopAnimationResource? bestFit)
        {
            bestFit = null;
            int bestSimilarityScore = int.MinValue;
            foreach (GodotUnitLoopAnimationResource candidateAnimationResource in setResource.Loops)
            {
                int candidateSimilarity = candidateAnimationResource.DetermineSimilarity(loopRequest);
                if (candidateSimilarity > bestSimilarityScore)
                {
                    bestFit = candidateAnimationResource;
                    bestSimilarityScore = candidateSimilarity;
                }
            }
            return bestFit != null;
        }

        public static bool TryFindBestFit(this GodotUnitAnimationSetResource setResource, UnitLoopAnimationRequest activeLoopRequest, UnitOneShotAnimationRequest oneShotRequest, [NotNullWhen(true)] out GodotUnitOneShotAnimationResource? bestFit)
        {
            bestFit = null;
            int bestSimilarityScore = int.MinValue;
            foreach (GodotUnitOneShotAnimationResource candidateAnimationResource in setResource.OneShots)
            {
                int candidateSimilarity = candidateAnimationResource.DetermineSimilarity(activeLoopRequest, oneShotRequest);
                if (candidateSimilarity > bestSimilarityScore)
                {
                    bestFit = candidateAnimationResource;
                    bestSimilarityScore = candidateSimilarity;
                }
            }
            return bestFit != null;
        }
    }
}
