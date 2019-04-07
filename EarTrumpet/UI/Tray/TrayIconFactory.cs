using EarTrumpet.Extensibility;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace EarTrumpet.UI.Tray
{
    public class TrayIconFactory
    {
        public static IAddonTrayIcon[] BuiltInItems { get; }
        internal static IAddonTrayIcon[] AddonItems { get; set; }

        static TrayIconFactory()
        {
            BuiltInItems = new IAddonTrayIcon[] {
                new HighContrastBuiltIn(),
                new ThemeColorBuiltIn(),
                new LegacyIconBuiltIn()
            };
        }

        public static Icon CreateAndResolveAll(IconKind kind)
        {
            var items = BuiltInItems.ToList();
            if (AddonItems != null)
            {
                items.AddRange(AddonItems);
            }
            items = items.OrderBy(b => b.Priority).ToList();
            var args = new AddonTrayIconEventArgs { Kind = kind, Icon = Create(kind) };
            foreach (var addon in items)
            {
                addon.TrayIconChanging(args);
            }
            return args.Icon;
        }

        public static Icon CreateAndResolveThroughBuiltInAddons(IconKind kind)
        {
            var args = new AddonTrayIconEventArgs { Kind = kind, Icon = Create(kind) };
            foreach(var addon in BuiltInItems.OrderBy(b => b.Priority))
            {
                addon.TrayIconChanging(args);
            }
            return args.Icon;
        }

        public static Icon Create(IconKind kind)
        {
            switch (kind)
            {
                case IconKind.Invalid: throw new InvalidOperationException("invalid icon");
                case IconKind.OriginalIcon:
                    using (var stream = Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Assets/Tray.ico")).Stream)
                    {
                        return new Icon(stream);
                    }
                default:
                    try
                    {
                        return IconUtils.GetIconFromFile(Interop.SndVolSSO.DllPath, (int)kind, WindowsTaskbar.Current.Dpi > 1);
                    }
                    catch(Exception ex)
                    {
                        Trace.WriteLine($"Couldn't load icon: {ex}");
                        return Create(IconKind.OriginalIcon);
                    }
            }

            throw new NotImplementedException();
        }
    }
}
