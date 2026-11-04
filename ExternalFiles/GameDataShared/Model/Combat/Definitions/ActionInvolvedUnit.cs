// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Model.Combat.Definitions
{
    public enum ActionInvolvedUnit
    {
        User,
        Target
    }

    public static class ActionInvolvedUnitExtensions
    {
        public static T GetInvolvedUnit<T>(this ActionInvolvedUnit actionInvolvedUnitChoice, T user, T target)
        {
            return actionInvolvedUnitChoice switch
            {
                ActionInvolvedUnit.User => user,
                ActionInvolvedUnit.Target => target,
                _ => throw new NotImplementedException($"Unhandled {nameof(actionInvolvedUnitChoice)}: ({actionInvolvedUnitChoice})")
            };
        }
    }
}
