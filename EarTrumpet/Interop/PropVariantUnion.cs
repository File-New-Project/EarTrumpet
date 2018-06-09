using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    //
    // NOTE: Verifiability requires that the
    // fields of these value-types need to be public
    // since PropVariantUnion has explicit layout,
    // and has these value-types as its fields in a way that
    // overlaps with other PropVariantUnion fields
    // (same FieldOffset for multiple fields).
    //

    /// <summary>
    /// CY, used in PropVariantUnion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct CY
    {
        public uint Lo;
        public int Hi;
    }

    /// <summary>
    /// BSTRBLOB, used in PropVariantUnion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct BSTRBLOB
    {
        public uint cbSize;
        public IntPtr pData;
    }

    /// <summary>
    /// BLOB, used in PropVariantUnion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct BLOB
    {
        public uint cbSize;
        public IntPtr pBlobData;
    }

    /// <summary>
    /// CArray, used in PropVariantUnion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct CArray
    {
        public uint cElems;
        public IntPtr pElems;
    }

    /// <summary>
    /// Union portion of PROPVARIANT
    /// </summary>
    /// <remarks>
    /// All fields (or their placeholders) are declared even if
    /// they are not used. This is to make sure that the size of
    /// the union matches the size of the union in
    /// the actual unmanaged PROPVARIANT structure
    /// for all architectures (32-bit/64-bit).
    /// Points to note:
    /// - All pointer type fields are declared as IntPtr.
    /// - CAxxx type fields (like CAC, CAUB, etc.) are all of same
    ///     structural layout, hence not all declared individually
    ///     since they are not used. A placeholder CArray
    ///     is used to represent all of them to account for the
    ///     size of these types. CArray is defined later.
    /// - Rest of the fields are declared with corresponding
    ///     managed equivalent types.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    struct PropVariantUnion
    {
        /// <summary>
        /// CHAR
        /// </summary>
        [FieldOffset(0)]
        public sbyte cVal;

        /// <summary>
        /// UCHAR
        /// </summary>
        [FieldOffset(0)]
        public byte bVal;

        /// <summary>
        /// SHORT
        /// </summary>
        [FieldOffset(0)]
        public short iVal;

        /// <summary>
        /// USHORT
        /// </summary>
        [FieldOffset(0)]
        public ushort uiVal;

        /// <summary>
        /// LONG
        /// </summary>
        [FieldOffset(0)]
        public int lVal;

        /// <summary>
        /// ULONG
        /// </summary>
        [FieldOffset(0)]
        public uint ulVal;

        /// <summary>
        /// INT
        /// </summary>
        [FieldOffset(0)]
        public int intVal;

        /// <summary>
        /// UINT
        /// </summary>
        [FieldOffset(0)]
        public uint uintVal;

        /// <summary>
        /// LARGE_INTEGER
        /// </summary>
        [FieldOffset(0)]
        public Int64 hVal;

        /// <summary>
        /// ULARGE_INTEGER
        /// </summary>
        [FieldOffset(0)]
        public UInt64 uhVal;

        /// <summary>
        /// FLOAT
        /// </summary>
        [FieldOffset(0)]
        public float fltVal;

        /// <summary>
        /// DOUBLE
        /// </summary>
        [FieldOffset(0)]
        public double dblVal;

        /// <summary>
        /// VARIANT_BOOL
        /// </summary>
        [FieldOffset(0)]
        public short boolVal;

        /// <summary>
        /// SCODE
        /// </summary>
        [FieldOffset(0)]
        public int scode;

        /// <summary>
        /// CY
        /// </summary>
        [FieldOffset(0)]
        public CY cyVal;

        /// <summary>
        /// DATE
        /// </summary>
        [FieldOffset(0)]
        public double date;

        /// <summary>
        /// FILETIME
        /// </summary>
        [FieldOffset(0)]
        public System.Runtime.InteropServices.ComTypes.FILETIME filetime;


        /// <summary>
        /// CLSID*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr puuid;

        /// <summary>
        /// CLIPDATA*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pclipdata;

        /// <summary>
        /// BSTR
        /// </summary>
        [FieldOffset(0)]
        public IntPtr bstrVal;

        /// <summary>
        /// BSTRBLOB
        /// </summary>
        [FieldOffset(0)]
        public BSTRBLOB bstrblobVal;

        /// <summary>
        /// BLOB
        /// </summary>
        [FieldOffset(0)]
        public BLOB blob;

        /// <summary>
        /// LPSTR
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pszVal;

        /// <summary>
        /// LPWSTR
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pwszVal;

        /// <summary>
        /// IUnknown*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr punkVal;

        /// <summary>
        /// IDispatch*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pdispVal;

        /// <summary>
        /// IStream*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pStream;

        /// <summary>
        /// IStorage*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pStorage;

        /// <summary>
        /// LPVERSIONEDSTREAM
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pVersionedStream;

        /// <summary>
        /// LPSAFEARRAY
        /// </summary>
        [FieldOffset(0)]
        public IntPtr parray;

        /// <summary>
        /// Placeholder for
        /// CAC, CAUB, CAI, CAUI, CAL, CAUL, CAH, CAUH; CAFLT,
        /// CADBL, CABOOL, CASCODE, CACY, CADATE, CAFILETIME,
        /// CACLSID, CACLIPDATA, CABSTR, CABSTRBLOB,
        /// CALPSTR, CALPWSTR, CAPROPVARIANT
        /// </summary>
        [FieldOffset(0)]
        public CArray cArray;

        /// <summary>
        /// CHAR*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pcVal;

        /// <summary>
        /// UCHAR*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pbVal;

        /// <summary>
        /// SHORT*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr piVal;

        /// <summary>
        /// USHORT*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr puiVal;

        /// <summary>
        /// LONG*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr plVal;

        /// <summary>
        /// ULONG*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pulVal;

        /// <summary>
        /// INT*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pintVal;

        /// <summary>
        /// UINT*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr puintVal;

        /// <summary>
        /// FLOAT*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pfltVal;

        /// <summary>
        /// DOUBLE*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pdblVal;

        /// <summary>
        /// VARIANT_BOOL*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pboolVal;

        /// <summary>
        /// DECIMAL*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pdecVal;

        /// <summary>
        /// SCODE*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pscode;

        /// <summary>
        /// CY*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pcyVal;

        /// <summary>
        /// DATE*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pdate;

        /// <summary>
        /// BSTR*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pbstrVal;

        /// <summary>
        /// IUnknown**
        /// </summary>
        [FieldOffset(0)]
        public IntPtr ppunkVal;

        /// <summary>
        /// IDispatch**
        /// </summary>
        [FieldOffset(0)]
        public IntPtr ppdispVal;

        /// <summary>
        /// LPSAFEARRAY*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pparray;

        /// <summary>
        /// PROPVARIANT*
        /// </summary>
        [FieldOffset(0)]
        public IntPtr pvarVal;
    }

}
