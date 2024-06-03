using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;

namespace EarTrumpet.UI.Controls
{
    public class ImageEx : Image
    {
        public IAppIconSource SourceEx { get => (IAppIconSource)GetValue(SourceExProperty); set => SetValue(SourceExProperty, value); }
        public static readonly DependencyProperty SourceExProperty = DependencyProperty.Register(
          "SourceEx", typeof(IAppIconSource), typeof(ImageEx), new PropertyMetadata(null, new PropertyChangedCallback(OnSourceExChanged)));

        private uint _dpi;
        private static readonly string _windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static readonly string _systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);

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
                    path = Environment.ExpandEnvironmentVariables(path.TrimStart('@'));

                    var scale = GetWindowDpi() / (double)96;
                    if (!isDesktopApp)
                    {
                        return LoadShellIcon(path, isDesktopApp, (int)(Width * scale), (int)(Height * scale));
                    }
                    else
                    {
                        var iconIndex = 0;
                        var iconPath = path.AsSpan();
                        unsafe
                        {
                            fixed (char* iconPathPtr = iconPath)
                            {
                                iconIndex = PInvoke.PathParseIconLocation(iconPathPtr);
                            }
                        }

                        if (iconIndex != 0)
                        {
                            using var icon = IconHelper.LoadIconResource(iconPath.ToString(), Math.Abs(iconIndex), (int)(Width * scale), (int)(Height * scale));
                            Trace.WriteLine($"ImageEx LoadImage {icon?.Size.Width}x{icon?.Size.Height} {path}");
                            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }
                        else
                        {
                            // libmpv-based applications, like Plex, may set an invalid indirect icon path
                            // https://github.com/mpv-player/mpv/issues/7269
                            // (e.g. C:\Program Files\Plex\Plex.exe,-IDI_ICON1)
                            //
                            // The legacy volume mixer falls back to enumerating icons in the image and
                            // selecting an icon that 'best fits the current display device'. We will
                            // mimic this behavior by stripping off the invalid resource identifier and
                            // asking the shell for an appropriate icon.

                            if (path.Contains(",-"))
                            {
                                path = path.Remove(path.LastIndexOf(",-", StringComparison.Ordinal));
                            }
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
            path = CanonicalizePath(path);

            IShellItem2 shellItem;
            try
            {
                PInvoke.SHCreateItemInKnownFolder(
                    PInvoke.FOLDERID_AppsFolder,
                    KNOWN_FOLDER_FLAG.KF_FLAG_DONT_VERIFY,
                    path,
                    typeof(IShellItem2).GUID,
                    out var rawShellItem).ThrowOnFailure();
                shellItem = (IShellItem2)rawShellItem;
            }
            catch(Exception)
            {
                if (!isDesktopApp)
                {
                    Trace.WriteLine($"ImageEx LoadShellIcon SHCreateItemInKnownFolder failed for non-desktop app ({path}).");
                }
                PInvoke.SHCreateItemFromParsingName(path, null, typeof(IShellItem2).GUID, out var rawShellItem);
                shellItem = (IShellItem2)rawShellItem;
            }

            var bmp = HBITMAP.Null;
            unsafe
            {
                ((IShellItemImageFactory)shellItem).GetImage(new SIZE { cx = cx, cy = cy }, SIIGBF.SIIGBF_RESIZETOFIT, &bmp);
            }
            try
            {
                var ret = Imaging.CreateBitmapSourceFromHBitmap(bmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Trace.WriteLine($"ImageEx LoadShellIcon {cx}x{cy} {path}");
                return ret;
            }
            finally
            {
                PInvoke.DeleteObject(new HGDIOBJ(bmp));
            }
        }

        private static string CanonicalizePath(string path)
        {
            if (Path.GetDirectoryName(path).StartsWith(_systemPath, StringComparison.InvariantCultureIgnoreCase))
            {
                path = Path.Combine(_windowsPath, "sysnative", path.Substring(_systemPath.Length + 1));
            }

            //
            // Microsoft includes garbage CortanaUI app assets (\MicrosoftWindows.Client.CBS_cw5n1h2txyewy\Cortana.UI\Assets\App)
            // so replace the appid with one that has better (than nothing) assets.
            //
            // Ref: https://github.com/File-New-Project/EarTrumpet/issues/1259
            //
            if (path.Equals("MicrosoftWindows.Client.CBS_cw5n1h2txyewy!CortanaUI", StringComparison.InvariantCultureIgnoreCase))
            {
                path = "MicrosoftWindows.Client.CBS_cw5n1h2txyewy!PackageMetadata";
            }

            return path;
        }

        private uint GetWindowDpi() => PInvoke.GetDpiForWindow(new HWND(((HwndSource)PresentationSource.FromVisual(this)).Handle));
        private static void OnSourceExChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((ImageEx)d).OnSourceExChanged();
    }
}
