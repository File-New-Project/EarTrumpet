using EarTrumpet.Extensions;
using EarTrumpet.UI.Views;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

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
            var root = ((FrameworkElement)Child);
            if (root == null)
            {
                throw new ArgumentException("Child");
            }

            var container = (FrameworkElement)PlacementTarget;
            if (container == null)
            {
                throw new ArgumentException("PlacementTarget");
            }

            var relativeTo = Window.GetWindow(this);

            Point offsetFromWindow = container.TranslatePoint(new Point(0, 0), relativeTo);
            if ((string)container.Tag != DeviceView.DeviceListItemKey)
            {
                var headerHeight = (double)App.Current.Resources["Mutable_DeviceTitleCellHeight"];
                var popupBorderSize = (Thickness)App.Current.Resources["PopupBorderThickness"];
                var volumeListMargin = (Thickness)App.Current.Resources["VolumeAppListMargin"];

                // Adjust for the title bar, top border and top margin on the app list.
                offsetFromWindow.Y -= (headerHeight + volumeListMargin.Bottom + popupBorderSize.Top);
            }

            Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var screen = Screen.FromHandle(relativeTo.GetHandle());
            var scaledWorkArea = new Rect(screen.WorkingArea.Left / this.DpiX(),
                                          screen.WorkingArea.Top / this.DpiY(),
                                          screen.WorkingArea.Width / this.DpiX(),
                                          screen.WorkingArea.Height / this.DpiY());

            var popupHeight = root.DesiredSize.Height;
            var popupOriginYScreenCoordinates = (relativeTo.PointToScreen(new Point(0, 0)).Y / this.DpiY()) + offsetFromWindow.Y;
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

            Width = ((FrameworkElement)PlacementTarget).ActualWidth;
            Height = popupHeight;
            Placement = PlacementMode.Absolute;
            HorizontalOffset = (relativeTo.PointToScreen(new Point(0, 0)).X / this.DpiX()) + offsetFromWindow.X;
            VerticalOffset = popupOriginYScreenCoordinates;

            Child.Focus();
        }
    }
}
