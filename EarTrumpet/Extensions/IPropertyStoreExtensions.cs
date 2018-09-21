using EarTrumpet.Interop;
using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Extensions
{
    public static class IPropertyStoreExtensions
    {
        public static T GetValue<T>(this IPropertyStore propStore, PROPERTYKEY key)
        {
            PropVariant pv = default(PropVariant);
            try
            {
                pv = propStore.GetValue(ref key);
                switch (pv.varType)
                {
                    case VarEnum.VT_LPWSTR:
                        return (T)Convert.ChangeType(Marshal.PtrToStringUni(pv.pwszVal), typeof(T));
                    default: throw new NotImplementedException();
                }
            }
            finally
            {
                Ole32.PropVariantClear(ref pv);
            }
        }
    }
}
