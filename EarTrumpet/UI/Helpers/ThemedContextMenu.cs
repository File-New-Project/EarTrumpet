using EarTrumpet.DataModel;
using EarTrumpet.Interop;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace EarTrumpet.UI.Helpers
{
    public class ThemedContextMenu
    {
        public static ContextMenu CreateThemedContextMenu()
        {
            var cm = new ContextMenu { };
            cm.ItemContainerTemplateSelector = new MenuItemTemplateSelector();
            cm.UsesItemContainerTemplate = true;
            cm.FontSize = 12;
            cm.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            cm.Opened += ContextMenu_Opened;
            cm.Closed += ContextMenu_Closed;
            cm.StaysOpen = true; // To be removed on open.
            return cm;
        }

        private static void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("ThemedContextMenu ContextMenu_Closed");
        }

        private static void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("ThemedContextMenu ContextMenu_Opened");
            var cm = (ContextMenu)sender;
            User32.SetForegroundWindow(((HwndSource)HwndSource.FromVisual(cm)).Handle);
            cm.Focus();
            cm.StaysOpen = false;
            ((Popup)cm.Parent).PopupAnimation = PopupAnimation.None;
        }
    }
}
