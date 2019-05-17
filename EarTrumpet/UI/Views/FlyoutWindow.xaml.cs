using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace EarTrumpet.UI.Views
{
    public partial class FlyoutWindow
    {
        private readonly FlyoutViewModel _viewModel;

        public FlyoutWindow(FlyoutViewModel flyoutViewModel)
        {
            InitializeComponent();

            _viewModel = flyoutViewModel;
            _viewModel.StateChanged += OnStateChanged;
            _viewModel.WindowSizeInvalidated += OnWindowSizeInvalidated;

            DataContext = _viewModel;
            Deactivated += OnDeactivated;
            SourceInitialized += OnSourceInitialized;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            Closing += OnClosing;
            FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            Show();
            Hide();
            this.ApplyExtendedWindowStyle(User32.WS_EX_TOOLWINDOW);

            _viewModel.ChangeState(FlyoutViewModel.ViewState.Hidden);
        }

        ~FlyoutWindow()
        {
            _viewModel.StateChanged -= OnStateChanged;
            _viewModel.WindowSizeInvalidated -= OnWindowSizeInvalidated;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            this.Cloak();
            Themes.Manager.Current.ThemeChanged += ThemeChanged;
            ThemeChanged();
        }

        private void OnStateChanged(object sender, object e)
        {
            switch (_viewModel.State)
            {
                case FlyoutViewModel.ViewState.Opening:
                    Show();
                    ThemeChanged();
                    UpdateWindowBounds();

                    // Focus the first device if available.
                    DevicesList.FindVisualChild<DeviceView>()?.FocusAndRemoveFocusVisual();

                    WaitForKeyboardVisuals(() =>
                    {
                        WindowAnimationLibrary.BeginFlyoutEntranceAnimation(this, () => _viewModel.ChangeState(FlyoutViewModel.ViewState.Open));
                    });
                    break;

                case FlyoutViewModel.ViewState.Closing_Stage1:
                    DevicesList.FindVisualChild<DeviceView>()?.FocusAndRemoveFocusVisual();

                    if (_viewModel.IsExpandingOrCollapsing)
                    {
                        WindowAnimationLibrary.BeginFlyoutExitanimation(this, () =>
                        {
                            this.Cloak();
                            // NB: Hidden to avoid the stage 2 hide delay, we want to show again immediately.
                            _viewModel.ChangeState(FlyoutViewModel.ViewState.Hidden);
                        });
                    }
                    else
                    {
                        this.Cloak();
                        DisableAcrylic();
                        WaitForKeyboardVisuals(() =>
                        {
                            Hide();
                            _viewModel.ChangeState(FlyoutViewModel.ViewState.Closing_Stage2);
                        });
                    }
                    break;
            }
        }

        private void UpdateWindowBounds()
        {
            var taskbarState = WindowsTaskbar.Current;

            if (taskbarState.ContainingScreen == null)
            {
                // We're not ready to lay out. (e.g. RDP transition)
                return;
            }

            UpdateLayout();
            LayoutRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double newHeight = LayoutRoot.DesiredSize.Height;

            var scaledWorkAreaHeight = taskbarState.ContainingScreen.WorkingArea.Height / this.DpiHeightFactor();
            if (taskbarState.IsAutoHideEnabled && (taskbarState.Location == WindowsTaskbar.Position.Top || taskbarState.Location == WindowsTaskbar.Position.Bottom))
            {
                scaledWorkAreaHeight -= taskbarState.Size.Bottom - taskbarState.Size.Top;
            }

            if (newHeight > scaledWorkAreaHeight)
            {
                newHeight = scaledWorkAreaHeight;
            }

            bool isRTL = SystemSettings.IsRTL;
            double newTop = 0;
            double newLeft = 0;

            switch (taskbarState.Location)
            {
                case WindowsTaskbar.Position.Left:
                    newLeft = (taskbarState.Size.Right / this.DpiWidthFactor());
                    newTop = (taskbarState.Size.Bottom / this.DpiHeightFactor()) - newHeight;
                    break;
                case WindowsTaskbar.Position.Right:
                    newLeft = (taskbarState.Size.Left / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.Size.Bottom / this.DpiHeightFactor()) - newHeight;
                    break;
                case WindowsTaskbar.Position.Top:
                    newLeft = isRTL ? (taskbarState.Size.Left / this.DpiWidthFactor()) :
                        (taskbarState.Size.Right / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.Size.Bottom / this.DpiHeightFactor());
                    break;
                case WindowsTaskbar.Position.Bottom:
                    newLeft = isRTL ? (taskbarState.Size.Left / this.DpiWidthFactor()) :
                        (taskbarState.Size.Right / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.Size.Top / this.DpiHeightFactor()) - newHeight;
                    break;
            }

            this.Move(newTop * this.DpiHeightFactor(), newLeft * this.DpiWidthFactor(), newHeight * this.DpiHeightFactor(), Width * this.DpiWidthFactor());
        }

        private void EnableBlurIfApplicable()
        {
            if (_viewModel.State == FlyoutViewModel.ViewState.Opening || _viewModel.State == FlyoutViewModel.ViewState.Open)
            {
                AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Flyout"), GetAccentFlags());
            }
            else
            {
                DisableAcrylic();
            }
        }

        private User32.AccentFlags GetAccentFlags()
        {
            switch (WindowsTaskbar.Current.Location)
            {
                case WindowsTaskbar.Position.Left:
                    return User32.AccentFlags.DrawRightBorder | User32.AccentFlags.DrawTopBorder;
                case WindowsTaskbar.Position.Right:
                    return User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawTopBorder;
                case WindowsTaskbar.Position.Top:
                    return User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawBottomBorder;
                case WindowsTaskbar.Position.Bottom:
                    return User32.AccentFlags.DrawTopBorder | User32.AccentFlags.DrawLeftBorder;
            }
            return User32.AccentFlags.None;
        }

        private void WaitForKeyboardVisuals(Action completed)
        {
            // Note: ApplicationIdle is necessary as KeyboardNavigation/ScheduleCleanup will 
            // use ContextIdle to purge the focus visuals.
            Dispatcher.BeginInvoke(completed, DispatcherPriority.ApplicationIdle, null);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _viewModel.UserEscaped();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Disable Alt+F4.
            e.Cancel = true;
            _viewModel.BeginClose(InputType.Keyboard);
        }

        private void OnLightDismissBorderPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.Dialog.IsVisible = false;
            e.Handled = true;
        }

        private void ThemeChanged() => EnableBlurIfApplicable();
        private void DisableAcrylic() => AccentPolicyLibrary.DisableAcrylic(this);
        private void OnDeactivated(object sender, EventArgs e) => _viewModel.BeginClose(InputType.Command);
        private void OnWindowSizeInvalidated(object sender, object e) => UpdateWindowBounds();
        private void OnDisplaySettingsChanged(object sender, EventArgs e) => Dispatcher.BeginInvoke((Action)(() => _viewModel.BeginClose(InputType.Command)));
    }
}
