using System;
using Windows.Win32.UI.Shell.PropertiesSystem;
using Windows.Win32.System.Variant;
using Windows.Win32;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.Devices.Properties;

namespace EarTrumpet.Extensions
{
    public static class IPropertyStoreExtensions
    {
        internal static T GetValue<T>(this IPropertyStore propStore, DEVPROPKEY key)
        {
            return GetValue<T>(propStore, new PROPERTYKEY { pid = key.pid, fmtid = key.fmtid, });
        }

        internal static T GetValue<T>(this IPropertyStore propStore, PROPERTYKEY key)
        {
            unsafe
            {
                var pv = new PROPVARIANT();
                try
                {
                    propStore.GetValue(&key, out pv);
                    switch (pv.Anonymous.Anonymous.vt)
                    {
                        case VARENUM.VT_LPWSTR:
                            var ptr = pv.Anonymous.Anonymous.Anonymous.pwszVal;
                            return (T)Convert.ChangeType(ptr.ToString(), typeof(T));
                        case VARENUM.VT_EMPTY:
                            return default;
                        default: throw new NotImplementedException();
                    }
                }
                finally
                {
                    PInvoke.PropVariantClear(ref pv);
                }
            }
        }
    }
}
