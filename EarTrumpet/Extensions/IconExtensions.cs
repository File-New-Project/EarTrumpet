using EarTrumpet.Interop;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.Extensions
{
    public static class IconExtensions
    {
        public static ImageSource ToImageSource(this Icon icon)
        {
            var bitmap = icon.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();
            ImageSource bitmapSource;
            try
            {
                bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                Gdi32.DeleteObject(hBitmap);
            }
            return bitmapSource;
        }

        public static Icon AsDisposableIcon(this Icon icon)
        {
            // System.Drawing.Icon does not expose a method to declare
            // ownership of its wrapped handle so we have to use reflection
            // here.

            // See also: https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Icon.cs,71

            icon.GetType().GetField("ownHandle", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(icon, true);
            return icon;
        }
    }
}
