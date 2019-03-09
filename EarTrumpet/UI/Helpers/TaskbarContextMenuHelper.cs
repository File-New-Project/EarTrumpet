using EarTrumpet.DataModel;
using EarTrumpet.Interop;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace EarTrumpet.UI.Helpers
{
    public class TaskbarContextMenuHelper
    {
        public static ContextMenu Create()
        {
            var contextMenu = new ContextMenu
            {
                FontSize = 12,
                FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                StaysOpen = true,
            };
            contextMenu.Opened += ContextMenu_Opened;
            contextMenu.Closed += (_, __) => Trace.WriteLine("TaskbarContextMenuHelper ContextMenu.Closed"); ;
            return contextMenu;
        }

        private static void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("TaskbarContextMenuHelper ContextMenu.Opened");
            var contextMenu = (ContextMenu)sender;

            // Workaround: The framework expects there to already be a WPF window open and thus fails to take focus.
            User32.SetForegroundWindow(((HwndSource)HwndSource.FromVisual(contextMenu)).Handle);
            contextMenu.Focus();
            contextMenu.StaysOpen = false;

            // Disable only the exit animation.
            ((Popup)contextMenu.Parent).PopupAnimation = PopupAnimation.None;
        }
    }
}
