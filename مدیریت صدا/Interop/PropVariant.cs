using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct PropArray
    {
        internal uint cElems;
        internal IntPtr pElems;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PropVariant
    {
        [FieldOffset(0)] public VarEnum varType;
        [FieldOffset(2)] public ushort wReserved1;
        [FieldOffset(4)] public ushort wReserved2;
        [FieldOffset(6)] public ushort wReserved3;
        [FieldOffset(8)] public byte bVal;
        [FieldOffset(8)] public sbyte cVal;
        [FieldOffset(8)] public ushort uiVal;
        [FieldOffset(8)] public short iVal;
        [FieldOffset(8)] public uint uintVal;
        [FieldOffset(8)] public int intVal;
        [FieldOffset(8)] public ulong ulVal;
        [FieldOffset(8)] public long lVal;
        [FieldOffset(8)] public float fltVal;
        [FieldOffset(8)] public double dblVal;
        [FieldOffset(8)] public short boolVal;
        [FieldOffset(8)] public IntPtr pclsidVal;
        [FieldOffset(8)] public IntPtr pszVal;
        [FieldOffset(8)] public IntPtr pwszVal;
        [FieldOffset(8)] public IntPtr punkVal;
        [FieldOffset(8)] public PropArray ca;
        [FieldOffset(8)] public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
    }
}
