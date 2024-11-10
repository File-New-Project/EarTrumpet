using Windows.Win32;
using Windows.Win32.Media.Audio;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;

namespace EarTrumpet.Interop.MMDeviceAPI;

public static class IMMDeviceExtensions
{
    public static T Activate<T>(this IMMDevice device)
    {
        object obj;
        unsafe
        {
            // Can't pass in null PROPVARIANT https://github.com/microsoft/CsWin32/issues/1081
            device.Activate(typeof(T).GUID, CLSCTX.CLSCTX_INPROC_SERVER, new PROPVARIANT_unmanaged(), out obj);
        }
        return (T)obj;
    }
}
