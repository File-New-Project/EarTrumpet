using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Misc
{
    public static class MarshalHelper
    {
        public static T PtrToStructure<T>(IntPtr ptr)
        {
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }
    }
}
