using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.Services
{
    public sealed class IconService
    {
        public static Icon GetIconFromFile(string path, int iconOrdinal = 0)
        {
            var moduleHandle = Kernel32.LoadLibraryEx(path, IntPtr.Zero,
                Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE | Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE);

            IntPtr iconHandle = IntPtr.Zero;
            Comctl32.LoadIconMetric(moduleHandle, new IntPtr(iconOrdinal), Comctl32.LoadIconDesiredMetric.Small, ref iconHandle);

            Kernel32.FreeLibrary(moduleHandle);
            return Icon.FromHandle(iconHandle);
        }

        public static ImageSource GetIconFromFileAsImageSource(string path, int iconIndex = 0)
        {
            // TODO: not clear both branches are needed here
            if (Path.GetExtension(path) == ".dll")
            {
                var iconHandle = Shell32.ExtractIcon(Process.GetCurrentProcess().Handle, path, iconIndex);
                var image = Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                User32.DestroyIcon(iconHandle);
                return image;
            }
            else
            {
                return System.Drawing.Icon.ExtractAssociatedIcon(path).ToImageSource();
            }
        }
    }
}
