using System;
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
        private class Interop
        {
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr objectHandle);
        }

        public static ImageSource ToImageSource(this Icon icon)
        {
            var bitmap = icon.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();

            ImageSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            Interop.DeleteObject(hBitmap);
            return bitmapSource;
        }
    }
}
