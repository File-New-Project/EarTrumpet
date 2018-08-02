using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal struct PropArray
    {
        internal uint cElems;
        internal IntPtr pElems;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct PropVariant
    {
        [FieldOffset(0)] internal VarEnum varType;
        [FieldOffset(2)] internal ushort wReserved1;
        [FieldOffset(4)] internal ushort wReserved2;
        [FieldOffset(6)] internal ushort wReserved3;
        [FieldOffset(8)] internal byte bVal;
        [FieldOffset(8)] internal sbyte cVal;
        [FieldOffset(8)] internal ushort uiVal;
        [FieldOffset(8)] internal short iVal;
        [FieldOffset(8)] internal uint uintVal;
        [FieldOffset(8)] internal int intVal;
        [FieldOffset(8)] internal ulong ulVal;
        [FieldOffset(8)] internal long lVal;
        [FieldOffset(8)] internal float fltVal;
        [FieldOffset(8)] internal double dblVal;
        [FieldOffset(8)] internal short boolVal;
        [FieldOffset(8)] internal IntPtr pclsidVal;
        [FieldOffset(8)] internal IntPtr pszVal;
        [FieldOffset(8)] internal IntPtr pwszVal;
        [FieldOffset(8)] internal IntPtr punkVal;
        [FieldOffset(8)] internal PropArray ca;
        [FieldOffset(8)] internal System.Runtime.InteropServices.ComTypes.FILETIME filetime;
    }
}
