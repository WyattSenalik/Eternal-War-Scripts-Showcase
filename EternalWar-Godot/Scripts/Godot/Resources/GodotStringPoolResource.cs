// Original Authors - Wyatt Senalik
using Godot;
using System;
using System.Collections.Generic;

namespace EternalWar.Engine.Godot.Resources
{
    [GlobalClass]
    public sealed partial class GodotStringPoolResource : Resource
    {
        public IReadOnlyList<GodotStringPoolElementResource> Elements => _elements;

        [Export] private GodotStringPoolElementResource[] _elements = Array.Empty<GodotStringPoolElementResource>();
    }

    public static class GodotStringPoolResourceExtensions
    {
        public static string RollFromPool(this GodotStringPoolResource poolResource, GodotEngineRandomResolver engineRandomResolver)
        {
            IReadOnlyList<GodotStringPoolElementResource> elements = poolResource.Elements;
            if (elements.Count <= 0)
                return string.Empty;
            if (elements.Count <= 1)
                return elements[0].Value;

            float totalWeight = 0.0f;
            foreach (GodotStringPoolElementResource element in elements)
            {
                totalWeight += element.Weight;
            }

            float chosenWeight = engineRandomResolver.RollWeight(totalWeight);
            float elapsedWeight = 0.0f;
            foreach (GodotStringPoolElementResource element in elements)
            {
                elapsedWeight += element.Weight;
                if (elapsedWeight >= chosenWeight)
                    return element.Value;
            }
            return elements[^1].Value;
        }
    }
}
