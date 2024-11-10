using System;

namespace EarTrumpet.Extensions;

public static class ShortExtensions
{
    public static bool HasFlag<T>(this short value, T flag) where T : struct
    {
        var shortFlag = Convert.ToUInt16(flag);
        return (value & shortFlag) == shortFlag;
    }

    public static bool HasFlag<T>(this ushort value, T flag) where T : struct => HasFlag((short)value, flag);
}