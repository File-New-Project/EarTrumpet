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
        using var hModule = PInvoke.LoadLibraryEx(path, LOAD_LIBRARY_FLAGS.DONT_RESOLVE_DLL_REFERENCES);
        unsafe
        {
            var rawModuleHandle = new HMODULE(hModule.DangerousGetHandle().ToPointer());
            HICON iconHandle;
            PInvoke.LoadIconWithScaleDown(rawModuleHandle, new PCWSTR((char*)iconOrdinal), cx, cy, &iconHandle);
            return Icon.FromHandle(iconHandle).AsDisposableIcon();
        }
    }

    public static Icon ColorIcon(Icon originalIcon, double fillPercent, System.Windows.Media.Color newColor)
    {
        using var bitmap = originalIcon.ToBitmap();
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width * fillPercent; x++)
            {
                var pixel = bitmap.GetPixel(x, y);

                if (pixel.R > 220)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(pixel.A, newColor.R, newColor.G, newColor.B));
                }
            }
        }

        return Icon.FromHandle(bitmap.GetHicon()).AsDisposableIcon();
    }
}
