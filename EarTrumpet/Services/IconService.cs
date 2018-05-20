using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using System;
using System.Drawing;
using System.Windows.Media;

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

        public static ImageSource GetIconFromFileAsImageSource(string path)
        {
            return System.Drawing.Icon.ExtractAssociatedIcon(path).ToImageSource();
        }
    }
}
