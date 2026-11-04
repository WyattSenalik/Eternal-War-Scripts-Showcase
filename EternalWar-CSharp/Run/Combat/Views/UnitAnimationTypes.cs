// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Client.Application.Run.Combat.Views
{
    public enum UnitOneShotAnimationType
    {
        AttackSwing = 0,
        Flinch = 1,
        Dodge = 2,
        Die = 3,
        FallAsleep = 4,
        WakeUp = 5,
        Push = 6,
    }

    public enum UnitLoopAnimationType
    {
        Idle = 0,
        Walking = 1,
        Pushed = 2,
        Dead = 3,
        Asleep = 4,
    }

    public static class UnitLoopAnimationTypeExtensions
    {
        public static int GetPriority(this UnitLoopAnimationType loopType)
        {
            return loopType switch
            {
                UnitLoopAnimationType.Idle => 0,
                UnitLoopAnimationType.Walking => 2,
                UnitLoopAnimationType.Pushed => 3,
                UnitLoopAnimationType.Dead => 4,
                UnitLoopAnimationType.Asleep => 1,
                _ => throw new NotImplementedException($"Unhandled {nameof(UnitLoopAnimationType)}: {loopType}")
            };
        }
    }
}
