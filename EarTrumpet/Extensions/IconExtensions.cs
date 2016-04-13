using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.Extensions
{
    public static class IconExtensions
    {
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("shell32.dll")]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static ImageSource ToImageSource(this Icon icon)
        {
            var bitmap = icon.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();

            ImageSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            return bitmapSource;
        }

        public static ImageSource ExtractIconFromDll(this string path, int iconIndex = 0)
        {
            var iconPtr = ExtractIcon(Process.GetCurrentProcess().Handle, path, iconIndex);
            var image = Imaging.CreateBitmapSourceFromHIcon(iconPtr, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DestroyIcon(iconPtr);
            return image;
        }
    }
}
