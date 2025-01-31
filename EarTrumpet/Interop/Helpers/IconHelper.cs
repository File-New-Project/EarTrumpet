using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using EarTrumpet.Extensions;
using Windows.Win32;
using Windows.Win32.System.LibraryLoader;
using Windows.Win32.UI.WindowsAndMessaging;

namespace EarTrumpet.Interop.Helpers;

public class IconHelper
{
    public static Icon LoadIconForTaskbar(string path, uint dpi)
    {
        Icon icon = null;
        if (path.StartsWith("pack://"))
        {
            using var stream = System.Windows.Application.GetResourceStream(new Uri(path)).Stream;
            icon = new Icon(stream, new Size(
                PInvoke.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXICON, dpi),
                PInvoke.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CYICON, dpi)));
        }
        else
        {
            var iconIndex = 0;
            var iconPath = path.AsSpan();
            unsafe
            {
                fixed (char* iconPathPtr = iconPath)
                {
                    iconIndex = PInvoke.PathParseIconLocation(iconPathPtr);
                }
            }
            
            icon = LoadIconResource(iconPath.ToString(), iconIndex,
                PInvoke.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXSMICON, dpi),
                PInvoke.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CYSMICON, dpi));
        }
        Trace.WriteLine($"IconHelper LoadIconForTaskbar {path} {icon?.Width}x{icon?.Height}");
        return icon;
    }

    public static Icon LoadIconResource(string path, int iconOrdinal, int cx, int cy)
    {
        using var hModule = PInvoke.LoadLibraryEx(path, LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_AS_IMAGE_RESOURCE);
        unsafe
        {
            var rawModuleHandle = new HMODULE(hModule.DangerousGetHandle().ToPointer());
            var groupResInfo = PInvoke.FindResource(rawModuleHandle, new PCWSTR((char*)iconOrdinal), PInvoke.RT_GROUP_ICON);
            var groupResData = PInvoke.LockResource(PInvoke.LoadResource(hModule, groupResInfo));
            var iconId = PInvoke.LookupIconIdFromDirectoryEx((byte*)groupResData, true, cx, cy, IMAGE_FLAGS.LR_DEFAULTCOLOR);

            var iconResInfo = PInvoke.FindResource(rawModuleHandle, new PCWSTR((char*)iconId), PInvoke.RT_ICON);
            var iconResData = PInvoke.LockResource(PInvoke.LoadResource(hModule, iconResInfo));
            var iconResSize = PInvoke.SizeofResource(hModule, iconResInfo);
            var iconHandle = PInvoke.CreateIconFromResourceEx((byte*)iconResData, iconResSize, true, 0x00030000, cx, cy, IMAGE_FLAGS.LR_DEFAULTCOLOR);

            return Icon.FromHandle(iconHandle).AsDisposableIcon();
        }
    }
}
