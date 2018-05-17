namespace EarTrumpet.Interop
{
    public enum VarType : short
    {
        /// <summary>
        /// BSTR
        /// </summary>
        VT_BSTR = 8,        // BSTR allocated using SysAllocString

        /// <summary>
        /// LPSTR
        /// </summary>
        VT_LPSTR = 30,

        /// <summary>
        /// FILETIME
        /// </summary>
        VT_FILETIME = 64,
    }

    public struct PropVariant
    {
        /// <summary>
        /// Variant type
        /// </summary>
        public VarType vt;

        /// <summary>
        /// unused
        /// </summary>
        public ushort wReserved1;

        /// <summary>
        /// unused
        /// </summary>
        public ushort wReserved2;

        /// <summary>
        /// unused
        /// </summary>
        public ushort wReserved3;

        /// <summary>
        /// union where the actual variant value lives
        /// </summary>
        public PropVariantUnion union;
    }
}
