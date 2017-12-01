using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.Services
{
    public sealed class IconService
    {
        private class Interop
        {
            [DllImport("shell32.dll")]
            public static extern IntPtr ExtractIcon(IntPtr instanceHandle, string path, int iconIndex);

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibraryEx(string path, IntPtr reserved, LoadLibraryFlags flags);

            [DllImport("kernel32.dll")]
            public static extern IntPtr FreeLibrary(IntPtr moduleHandle);

            [Flags]
            public enum LoadLibraryFlags
            {
                LOAD_LIBRARY_AS_DATAFILE = 0x02,
                LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20
            }

            [DllImport("user32.dll")]
            public static extern bool DestroyIcon(IntPtr iconHandle);

            [DllImport("comctl32.dll")]
            public static extern int LoadIconMetric(IntPtr instanceHandle, IntPtr iconId, LoadIconDesiredMetric desiredMetric, ref IntPtr icon);

            public enum LoadIconDesiredMetric
            {
                Small,
                Large,
            }
        }

        public static Icon GetIconFromFile(string path, int iconOrdinal = 0)
        {
            var moduleHandle = Interop.LoadLibraryEx(path, IntPtr.Zero,
                Interop.LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE | Interop.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE);

            IntPtr iconHandle = IntPtr.Zero;
            Interop.LoadIconMetric(moduleHandle, new IntPtr(iconOrdinal), Interop.LoadIconDesiredMetric.Small, ref iconHandle);

            
            return Icon.FromHandle(iconHandle);
        }

        public static ImageSource GetIconFromFileAsImageSource(string path, int iconIndex = 0)
        {
            var iconHandle = Interop.ExtractIcon(Process.GetCurrentProcess().Handle, path, iconIndex);
            var image = Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Interop.DestroyIcon(iconHandle);

            return image;
        }
    }
}
