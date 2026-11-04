// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.CombatStep
{
    public interface ICombatStepHandler
    {
        Type StepInfoType { get; }

        void Simulate(CombatStepSimulationContext context, CombatStepInfo stepInfo);
    }

    public abstract class CombatStepHandler<TStepInfo> : ICombatStepHandler where TStepInfo : CombatStepInfo
    {
        public Type StepInfoType => typeof(TStepInfo);

        public void Simulate(CombatStepSimulationContext context, CombatStepInfo stepInfo)
        {
            if (stepInfo is not TStepInfo castedStepInfo)
                throw new ArgumentException($"{GetType().Name} cannot simulate a {stepInfo.GetType().Name}");
            Simulate(context, castedStepInfo);
        }

        protected abstract void Simulate(CombatStepSimulationContext context, TStepInfo stepInfo);
    }
}
