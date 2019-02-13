using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
                Comctl32.LoadIconMetric(moduleHandle, new IntPtr(iconOrdinal), useLargeIcon ? Comctl32.LI_METRIC.LIM_LARGE : Comctl32.LI_METRIC.LIM_SMALL, ref iconHandle);
            }
            finally
            {
                Kernel32.FreeLibrary(moduleHandle);
            }

            return Icon.FromHandle(iconHandle);
        }

        public static ImageSource GetIconAsImageSourceFromFile(string path, int iconIndex = 0)
        {
            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                iconHandle = Shell32.ExtractIcon(Process.GetCurrentProcess().Handle, path, iconIndex);
                return Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                User32.DestroyIcon(iconHandle);
            }
        }

        public static Icon ColorIcon(Icon originalIcon, System.Windows.Media.Color newColor)
        {
            using (var bitmap = originalIcon.ToBitmap())
            {
                originalIcon.Dispose();

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);

                        if (pixel.R == 255)
                        {
                            bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel.A, newColor.R, newColor.G, newColor.B));
                        }
                    }
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
        }
    }
}
