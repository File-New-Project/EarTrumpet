using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace EarTrumpet.UI.Views
{
    public partial class VolumeControlPopup : Popup
    {
        public bool IsPopupVisible { get { return (bool)GetValue(DeviceProperty); } set { SetValue(DeviceProperty, value); } }
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("IsPopupVisible", typeof(bool), typeof(VolumeControlPopup), new PropertyMetadata(new PropertyChangedCallback(OnIsVisibleChanged)));

        private bool _hiding;

        public VolumeControlPopup()
        {
            InitializeComponent();

            AllowsTransparency = true;
            StaysOpen = false;

            Opened += VolumeControlPopup_Opened;
            Closed += VolumeControlPopup_Closed;
        }

        private void VolumeControlPopup_Closed(object sender, EventArgs e)
        {
            IsPopupVisible = false;
        }

        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VolumeControlPopup)d;

            if (self.IsPopupVisible)
            {
                self.Child.Opacity = 0;
                self.IsOpen = true;
            }
            else
            {
                self.HideWithAnimation();
            }
        }

        private void VolumeControlPopup_Opened(object sender, EventArgs e)
        {
            AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Settings"), Interop.User32.AccentFlags.None);

            PositionAndShow();
        }

        private void ShowWithAnimation()
        {
            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 0,
                To = 1,
            };

            fadeAnimation.Completed += (_, __) =>
            {
                Child.Focus();
                Child.Opacity = 1;
            };

            Child.Opacity = (double)fadeAnimation.From;
            Child.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        public void HideWithAnimation()
        {
            if (!_hiding && IsOpen)
            {
                _hiding = true;

                var fadeAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                    From = 1,
                    To = 0.25,
                };

                fadeAnimation.Completed += (_, __) =>
                {
                    Opacity = 0;
                    IsOpen = false;
                    _hiding = false;
                };

                Child.BeginAnimation(OpacityProperty, fadeAnimation);
            }
        }

        public void PositionAndShow()
        {
            var container = (FrameworkElement)Tag;
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

            if ((string)container.Tag == DeviceView.DeviceListItemKey)
            {
                // No adjustment.
            }
            else if (container is FlyoutWindow)
            {
                // No adjustment.
            }
            else
            {
                // Adjust for the title bar, top border and top margin on the app list.
                offsetFromWindow.Y -= (HEADER_SIZE + volumeListMargin.Bottom + PopupBorderSize.Top);
            }

            var root = ((FrameworkElement)Child);
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

            Width = ((FrameworkElement)Tag).ActualWidth;
            Height = popupHeight;

            ShowWithAnimation();
        }
    }
}
