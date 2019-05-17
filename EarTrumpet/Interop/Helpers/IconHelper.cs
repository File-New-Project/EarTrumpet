using EarTrumpet.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace EarTrumpet.Interop.Helpers
{
    public class IconHelper
    {
        public static Icon LoadSmallIcon(string path)
        {
            var dpi = WindowsTaskbar.Dpi;
            Icon icon = null;
            if (path.StartsWith("pack://"))
            {
                using (var stream = System.Windows.Application.GetResourceStream(new Uri(path)).Stream)
                {
                    icon = new Icon(stream, new Size(
                        User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CXICON, dpi), 
                        User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CYICON, dpi)));
                }
            }
            else
            {
                var iconPath = new StringBuilder(path);
                int iconIndex = Shlwapi.PathParseIconLocationW(iconPath);
                icon = LoadIconWithScaleDown(iconPath.ToString(), iconIndex, 
                    User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CXSMICON, dpi), 
                    User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CYSMICON, dpi));
            }

            Trace.WriteLine($"IconHelper LoadSmallIcon {icon?.Size.Width}x{icon?.Size.Height} {path}");
            return icon;
        }

        private static Icon LoadIconWithScaleDown(string path, int iconOrdinal, int cx, int cy)
        {
            var hModule = Kernel32.LoadLibraryEx(path, IntPtr.Zero, Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE | Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE);
            try
            {
                var groupResInfo = Kernel32.FindResourceW(hModule, new IntPtr(iconOrdinal), Kernel32.RT_GROUP_ICON);
                var groupResData = Kernel32.LockResource(Kernel32.LoadResource(hModule, groupResInfo));
                var iconId = User32.LookupIconIdFromDirectoryEx(groupResData, true, cx, cy, User32.LoadImageFlags.LR_DEFAULTCOLOR);

                var iconResInfo = Kernel32.FindResourceW(hModule, new IntPtr(iconId), Kernel32.RT_ICON);
                var iconResData = Kernel32.LockResource(Kernel32.LoadResource(hModule, iconResInfo));
                var iconResSize = Kernel32.SizeofResource(hModule, iconResInfo);
                var iconHandle = User32.CreateIconFromResourceEx(iconResData, iconResSize, true, User32.IconCursorVersion.Default, cx, cy, User32.LoadImageFlags.LR_DEFAULTCOLOR);
                return Icon.FromHandle(iconHandle).AsDisposableIcon();
            }
            finally
            {
                Kernel32.FreeLibrary(hModule);
            }
        }


        public static Icon ShellExtractIcon(string path, int iconIndex = 0)
        {
            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                return Icon.FromHandle(Shell32.ExtractIcon(Process.GetCurrentProcess().Handle, path, iconIndex)).AsDisposableIcon();
            }
            finally
            {
                User32.DestroyIcon(iconHandle);
            }
        }

        public static Icon ColorIcon(Icon originalIcon, double fillPercent, System.Windows.Media.Color newColor)
        {
            using (var bitmap = originalIcon.ToBitmap())
            {
                originalIcon.Dispose();

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width * fillPercent; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);

                        if (pixel.R > 220)
                        {
                            bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel.A, newColor.R, newColor.G, newColor.B));
                        }
                    }
                }

                return Icon.FromHandle(bitmap.GetHicon()).AsDisposableIcon();
            }
        }
    }
}
