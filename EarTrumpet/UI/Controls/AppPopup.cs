using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Views;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace EarTrumpet.UI.Controls
{
    public class AppPopup : Popup
    {
        public AppPopup()
        {
            Opened += OnOpened;
        }

        // This is effectively custom placement, but not using PlacementMode.Custom.
        // - Display popup over target
        // - Calculate offset dynamically based on whether the popup is on a device or an app.
        // - Consider the monitor WorkArea when positioning, and do not cover the taskbar other other docked windows.
        private void OnOpened(object sender, EventArgs e)
        {
            var container = (FrameworkElement)PlacementTarget;
            if (container == null)
            {
                throw new ArgumentException("container");
            }

            var relativeTo = Window.GetWindow(this);
            if (relativeTo == null)
            {
                throw new ArgumentException("relativeTo");
            }

            var taskbarState = WindowsTaskbar.Current;
            if (taskbarState.ContainingScreen == null)
            {
                throw new ArgumentException("taskbarState.ContainingScreen");
            }

            var HEADER_SIZE = (double)App.Current.Resources["Mutable_DeviceTitleCellHeight"];
            var PopupBorderSize = (Thickness)App.Current.Resources["PopupBorderThickness"];
            var volumeListMargin = (Thickness)App.Current.Resources["VolumeAppListMargin"];

            Point offsetFromWindow = container.TranslatePoint(new Point(0, 0), relativeTo);

            if ((string)container.Tag != DeviceView.DeviceListItemKey)
            {
                // Adjust for the title bar, top border and top margin on the app list.
                offsetFromWindow.Y -= (HEADER_SIZE + volumeListMargin.Bottom + PopupBorderSize.Top);
            }

            var root = ((FrameworkElement)Child);
            if (root == null)
            {
                throw new ArgumentException("root");
            }
            root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var popupHeight = root.DesiredSize.Height;

            var popupOriginYScreenCoordinates = (relativeTo.PointToScreen(new Point(0, 0)).Y / this.DpiHeightFactor()) + offsetFromWindow.Y;

            var scaledWorkArea = new Rect(taskbarState.ContainingScreen.WorkingArea.Left / this.DpiWidthFactor(),
                taskbarState.ContainingScreen.WorkingArea.Top / this.DpiHeightFactor(),
                taskbarState.ContainingScreen.WorkingArea.Width / this.DpiWidthFactor(),
                taskbarState.ContainingScreen.WorkingArea.Height / this.DpiHeightFactor());

            // If we flow off the bottom
            if (popupOriginYScreenCoordinates + popupHeight > scaledWorkArea.Bottom)
            {
                popupOriginYScreenCoordinates = scaledWorkArea.Bottom - popupHeight;

                // If we also flow off the top
                if (popupOriginYScreenCoordinates < scaledWorkArea.Top)
                {
                    popupOriginYScreenCoordinates = scaledWorkArea.Top;
                    popupHeight = scaledWorkArea.Bottom - scaledWorkArea.Top;
                }
            }

            Placement = PlacementMode.Absolute;
            HorizontalOffset = (relativeTo.PointToScreen(new Point(0, 0)).X / this.DpiWidthFactor()) + offsetFromWindow.X;
            VerticalOffset = popupOriginYScreenCoordinates;

            Width = ((FrameworkElement)PlacementTarget).ActualWidth;
            Height = popupHeight;
            Child.Focus();
        }
    }
}
