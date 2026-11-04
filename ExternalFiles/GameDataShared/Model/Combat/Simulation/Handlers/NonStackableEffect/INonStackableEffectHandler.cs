// Original Authors - Wyatt Senalik
using EternalWar.Core.Model.Combat.Definitions;
using System;

namespace EternalWar.Core.Model.Combat.Simulation.Handlers.NonStackableEffect
{
    public interface INonStackableEffectHandler
    {
        Type InfoType { get; }
    }

    public abstract class NonStackableEffectHandler<TInfo> : INonStackableEffectHandler where TInfo : NonStackableEffectInfo
    {
        public Type InfoType => typeof(TInfo);
    }

    public sealed class NonStackableEffectHandler_SkipTurn : NonStackableEffectHandler<NonStackableEffectInfo_SkipTurn>
    {
        // TODO. Also move to its own class
    }

    public sealed class NonStackableEffectHandler_DoubleTurn : NonStackableEffectHandler<NonStackableEffectInfo_DoubleTurn>
    {
        // TODO. Also move to its own class
    }

    public sealed class NonStackableEffectHandler_Invincible : NonStackableEffectHandler<NonStackableEffectInfo_Invincible>
    {
        // TODO. Also move to its own class
    }

    public sealed class NonStackableEffectHandler_ImmuneToSpecificPassive : NonStackableEffectHandler<NonStackableEffectInfo_ImmuneToSpecificPassive>
    {
        // TODO. Also move to its own class
    }
}
