using EarTrumpet.DataModel;
using EarTrumpet.Interop;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace EarTrumpet.UI.Helpers
{
    public class ThemedContextMenu
    {
        public static MenuItem CreateThemedMenuItem()
        {
            return new MenuItem
            {
                Style = (Style)Application.Current.FindResource("MenuItemDarkOnly")
            };
        }

        public static MenuItem AddItem(ItemsControl menu, string displayName, ICommand action)
        {
            if (displayName == "-")
            {
                menu.Items.Add(new Separator());
                return null;
            }

            var item = new MenuItem
            {
                Header = displayName,
                Command = action,
                Style = (Style)Application.Current.FindResource("MenuItemDarkOnly")
            };
            menu.Items.Add(item);
            return item;
        }

        public static MenuItem AddItem(ItemsControl menu, MenuItem item)
        {
            item.Style = (Style)Application.Current.FindResource("MenuItemDarkOnly");
            menu.Items.Add(item);
            return item;
        }

        public static ContextMenu CreateThemedContextMenu()
        {
            var cm = new ContextMenu { Style = (Style)Application.Current.FindResource("ContextMenuDarkOnly") };

            cm.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            cm.Opened += ContextMenu_Opened;
            cm.Closed += ContextMenu_Closed;
            cm.StaysOpen = true; // To be removed on open.

            return cm;
        }

        private static void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("TrayIcon ContextMenu_Closed");
        }

        private static void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("TrayIcon ContextMenu_Opened");
            var cm = (ContextMenu)sender;
            User32.SetForegroundWindow(((HwndSource)HwndSource.FromVisual(cm)).Handle);
            cm.Focus();
            cm.StaysOpen = false;
            ((Popup)cm.Parent).PopupAnimation = PopupAnimation.None;
        }
    }
}
