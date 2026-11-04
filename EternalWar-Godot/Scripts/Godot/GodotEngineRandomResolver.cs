// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Engine.Godot
{
    public sealed class GodotEngineRandomResolver
    {
        private readonly Random _random = new();

        public float RollWeight(float weight)
        {
            return (float)_random.NextDouble() * weight;
        }
    }
}
