using EarTrumpet.Extensions;
using EarTrumpet.UI.Tray;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    public class IconUtils
    {
        public static Icon GetIconFromFile(string path, int iconOrdinal = 0, bool useLargeIcon = false)
        {
            var moduleHandle = Kernel32.LoadLibraryEx(path, IntPtr.Zero,
                Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE | Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE);

            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                Comctl32.LoadIconWithScaleDown(moduleHandle, new IntPtr(iconOrdinal), SystemInformation.SmallIconSize.Width, SystemInformation.SmallIconSize.Height, ref iconHandle);
            }
            finally
            {
                Kernel32.FreeLibrary(moduleHandle);
            }

            return Icon.FromHandle(iconHandle);
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

        public static Icon ColorIcon(Icon originalIcon, IconKind kind, System.Windows.Media.Color newColor)
        {
            using (var bitmap = originalIcon.ToBitmap())
            {
                originalIcon.Dispose();

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width * (kind == IconKind.NoDevice ? 0.4 : 1); x++)
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
