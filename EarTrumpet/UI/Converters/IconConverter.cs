using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.UI.Converters
{
    public class IconConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var iconInfo = (IconLoadInfo)value;
            if (!iconInfo.IsLoadComplete)
            {
                iconInfo.IsLoadComplete = true;
                iconInfo.CachedValue = GetIconFromFileImpl(iconInfo.IconPath, iconInfo.IsDesktopApp);
            }
            return iconInfo.CachedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        public static Icon GetDesktopIconFromFile(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                var iconPath = new StringBuilder(path);
                int iconIndex = Shlwapi.PathParseIconLocationW(iconPath);
                if (iconIndex != 0)
                {
                    return IconHelper.ShellExtractIcon(iconPath.ToString(), iconIndex);
                }
                else
                {
                    return System.Drawing.Icon.ExtractAssociatedIcon(path);
                }
            }
            return null;
        }

        private static ImageSource GetIconFromFileImpl(string path, bool isDesktopApp)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            ImageSource ret = null;
            try
            {
                if (isDesktopApp)
                {
                    ret = GetDesktopIconFromFile(path).ToImageSource();
                }
                else
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();
                    ret = bitmap;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to load icon: {ex}");
            }
            return ret;
        }
    }
}
