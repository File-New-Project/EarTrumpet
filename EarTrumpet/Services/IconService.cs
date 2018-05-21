using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using System;
using System.Drawing;
using System.Windows.Media;

namespace EarTrumpet.Services
{
    sealed class IconService
    {
        public static Icon GetIconFromFile(string path, int iconOrdinal = 0)
        {
            var moduleHandle = Kernel32.LoadLibraryEx(path, IntPtr.Zero,
                Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE | Kernel32.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE);

            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                Comctl32.LoadIconMetric(moduleHandle, new IntPtr(iconOrdinal), Comctl32.LI_METRIC.LIM_SMALL, ref iconHandle);
            }
            finally
            {
                Kernel32.FreeLibrary(moduleHandle);
            }

            return Icon.FromHandle(iconHandle);
        }

        public static ImageSource GetIconFromFileAsImageSource(string path)
        {
            return System.Drawing.Icon.ExtractAssociatedIcon(path).ToImageSource();
        }
    }
}
