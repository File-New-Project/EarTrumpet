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

            Icon returnIcon;
            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                Comctl32.LoadIconMetric(moduleHandle, new IntPtr(iconOrdinal), useLargeIcon ? Comctl32.LI_METRIC.LIM_LARGE : Comctl32.LI_METRIC.LIM_SMALL, ref iconHandle);
                returnIcon = (Icon)Icon.FromHandle(iconHandle).Clone();
            }
            finally
            {
                Kernel32.FreeLibrary(moduleHandle);
                User32.DestroyIcon(iconHandle);
            }
            return returnIcon;
        }

        public static ImageSource GetIconAsImageSourceFromFile(string path, int iconIndex = 0)
        {
            ImageSource returnImageSource;
            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                iconHandle = Shell32.ExtractIcon(Process.GetCurrentProcess().Handle, path, iconIndex);
                returnImageSource = Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                User32.DestroyIcon(iconHandle);
            }
            return returnImageSource;
        }
    }
}
