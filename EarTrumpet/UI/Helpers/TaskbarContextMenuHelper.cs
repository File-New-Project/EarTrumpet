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
            var cm = new ContextMenu
            {
                FontSize = 12,
                FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                StaysOpen = true,
            };
            cm.Opened += ContextMenu_Opened;
            cm.Closed += (_, __) => Trace.WriteLine("TaskbarContextMenu ContextMenu_Closed"); ;
            return cm;
        }

        private static void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("TaskbarContextMenu ContextMenu_Opened");
            var cm = (ContextMenu)sender;
            User32.SetForegroundWindow(((HwndSource)HwndSource.FromVisual(cm)).Handle);
            cm.Focus();
            cm.StaysOpen = false;
            ((Popup)cm.Parent).PopupAnimation = PopupAnimation.None;
        }
    }
}
