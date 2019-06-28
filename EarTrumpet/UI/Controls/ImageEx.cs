using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.UI.Controls
{
    public class ImageEx : Image
    {
        public IAppIconSource SourceEx { get => (IAppIconSource)GetValue(SourceExProperty); set => SetValue(SourceExProperty, value); }
        public static readonly DependencyProperty SourceExProperty = DependencyProperty.Register(
          "SourceEx", typeof(IAppIconSource), typeof(ImageEx), new PropertyMetadata(null, new PropertyChangedCallback(OnSourceExChanged)));

        private uint _dpi;

        public ImageEx()
        {
            DpiChanged += OnDpiChanged;
            Loaded += (_, __) => OnSourceExChanged();
        }

        private void OnDpiChanged(object sender, DpiChangedEventArgs e)
        {
            if (IsLoaded)
            {
                var nextDpi = GetWindowDpi();
                if (nextDpi != _dpi)
                {
                    _dpi = nextDpi;
                    OnSourceExChanged();
                }
            }
        }

        private void OnSourceExChanged()
        {
            if (SourceEx != null && IsLoaded)
            {
                Source = LoadImage(SourceEx.IconPath, SourceEx.IsDesktopApp);
            }
        }

        private ImageSource LoadImage(string path, bool isDesktopApp)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    path = Environment.ExpandEnvironmentVariables(path);
                    var scale = GetWindowDpi() / (double)96;
                    if (!isDesktopApp)
                    {
                        return LoadShellIcon(path, isDesktopApp, (int)(Width * scale), (int)(Height * scale));
                    }
                    else
                    {
                        var iconPath = new StringBuilder(path);
                        int iconIndex = Shlwapi.PathParseIconLocationW(iconPath);
                        if (iconIndex != 0)
                        {
                            using (var icon = IconHelper.LoadIconResource(iconPath.ToString(), Math.Abs(iconIndex), (int)(Width * scale), (int)(Height * scale)))
                            {
                                Trace.WriteLine($"ImageEx LoadImage {icon?.Size.Width}x{icon?.Size.Height} {path}");
                                return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            }
                        }
                        else
                        {
                            return LoadShellIcon(path, isDesktopApp, (int)(Width * scale), (int)(Height * scale));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ImageEx LoadImage Failed: {path} {ex}");
                }
            }
            return null;
        }

        public static ImageSource LoadShellIcon(string path, bool isDesktopApp, int cx, int cy)
        {
            var item = isDesktopApp ? Shell32.SHCreateItemFromParsingName(path, IntPtr.Zero, typeof(IShellItem2).GUID) :
                Shell32.SHCreateItemInKnownFolder(FolderIds.AppsFolder, Shell32.KF_FLAG_DONT_VERIFY, path, typeof(IShellItem2).GUID);

            ((IShellItemImageFactory)item).GetImage(new SIZE { cx = cx, cy = cy }, SIIGBF.SIIGBF_RESIZETOFIT, out var bmp);
            try
            {
                var ret = Imaging.CreateBitmapSourceFromHBitmap(bmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Trace.WriteLine($"ImageEx LoadShellIcon {cx}x{cy} {path}");
                return ret;
            }
            finally
            {
                Gdi32.DeleteObject(bmp);
            }
        }

        private uint GetWindowDpi() => User32.GetDpiForWindow(((HwndSource)PresentationSource.FromVisual(this)).Handle);
        private static void OnSourceExChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((ImageEx)d).OnSourceExChanged();
    }
}
