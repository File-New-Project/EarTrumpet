using EarTrumpet.Extensions;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet
{
    public partial class MainWindow
    {
        private readonly AudioMixerViewModel _viewModel;

		private readonly TrayIcon _trayIcon;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new AudioMixerViewModel();
            _trayIcon = new TrayIcon();
            _trayIcon.Invoked += TrayIcon_Invoked;

            DataContext = _viewModel;

            // Move keyboard focus to the first element. Disabled this since it is ugly but not sure invisible
            // visuals are preferrable.
            // Activated += (s,e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            SourceInitialized += (s, e) => UpdateTheme();
        }

        void TrayIcon_Invoked()
        {
            if (this.IsWindowVisible())
            {
                this.HideWithAnimation();
            }
            else
            {
                _viewModel.Refresh();
                UpdateTheme();
                UpdateWindowPosition();
                this.ShowwithAnimation();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.HideWithAnimation();
        }
        
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.HideWithAnimation();
            }
        }

        private void Slider_TouchDown(object sender, TouchEventArgs e)
        {
            VisualStateManager.GoToState((FrameworkElement)sender, "Pressed", true);

            var slider = (Slider)sender;
            slider.SetPositionByControlPoint(e.GetTouchPoint(slider).Position);
            slider.CaptureTouch(e.TouchDevice);
            e.Handled = true;
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                VisualStateManager.GoToState((FrameworkElement)sender, "Pressed", true);

                var slider = (Slider)sender;
                slider.SetPositionByControlPoint(e.GetPosition(slider));
                slider.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Slider_TouchUp(object sender, TouchEventArgs e)
        {
            VisualStateManager.GoToState((FrameworkElement)sender, "Normal", true);

            var slider = (Slider)sender;
            slider.ReleaseTouchCapture(e.TouchDevice);
            e.Handled = true;
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var slider = (Slider)sender;
            if (slider.IsMouseCaptured)
            {
                // If the point is outside of the control, clear the hover state.
                Rect rcSlider = new Rect(0, 0, slider.ActualWidth, slider.ActualHeight);
                if (!rcSlider.Contains(e.GetPosition(slider)))
                {
                    VisualStateManager.GoToState((FrameworkElement)sender, "Normal", true);
                }

                ((Slider)sender).ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void Slider_TouchMove(object sender, TouchEventArgs e)
        {
            var slider = (Slider)sender;
            if (slider.AreAnyTouchesCaptured)
            {
                slider.SetPositionByControlPoint(e.GetTouchPoint(slider).Position);
                e.Handled = true;
            }
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            var slider = (Slider)sender;
            if (slider.IsMouseCaptured)
            {
                slider.SetPositionByControlPoint(e.GetPosition(slider));
            }
        }

        private void MouseWheel_Manipulation(object sender, MouseWheelEventArgs e) {
            var slider = (Slider)sender;
            var amount = Math.Sign(e.Delta)*2.0;
            slider.ChangePositionByAmount(amount);
            e.Handled = true;
        }

        private void UpdateTheme()
        {
            // Call UpdateTheme before UpdateWindowPosition in case sizes change with the theme.
            ThemeService.UpdateThemeResources(Resources);
            if (ThemeService.IsWindowTransparencyEnabled)
            {
                this.EnableBlur();
            }
            else
            {
                this.DisableBlur();
            }
        }

        private void UpdateWindowPosition()
        {
            LayoutRoot.UpdateLayout();
            LayoutRoot.Measure(new Size(double.PositiveInfinity, MaxHeight));
            Height = LayoutRoot.DesiredSize.Height;

            var taskbarScreenWorkArea = TaskbarService.TaskbarScreen.WorkingArea;
            var taskbarPosition = TaskbarService.TaskbarPosition;            
            Left = (taskbarPosition == TaskbarPosition.Left) ? taskbarScreenWorkArea.Left : taskbarScreenWorkArea.Right - Width;
            Top = (taskbarPosition == TaskbarPosition.Top) ? taskbarScreenWorkArea.Top : taskbarScreenWorkArea.Bottom - Height;
        }
    }
}
