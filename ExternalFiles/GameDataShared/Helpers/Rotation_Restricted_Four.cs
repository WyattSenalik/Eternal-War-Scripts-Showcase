// Original Authors - Wyatt Senalik
using System;

namespace EternalWar.Core.Helpers
{
    public enum Rotation_Restricted_Four
    {
        _0,
        _90,
        _180,
        _270
    }

    public static class Rotation_Restricted_FourExtensions
    {
        public static float GetEulerAngle(this Rotation_Restricted_Four rotation)
        {
            return rotation switch
            {
                Rotation_Restricted_Four._0 => 0.0f,
                Rotation_Restricted_Four._90 => 90.0f,
                Rotation_Restricted_Four._180 => 180.0f,
                Rotation_Restricted_Four._270 => 270.0f,
                _ => throw new NotImplementedException($"Unhandled {nameof(Rotation_Restricted_Four)}: {rotation}"),
            };
        }
    }
}
