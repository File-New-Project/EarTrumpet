using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Themes;
using EarTrumpet.UI.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EarTrumpet.UI.Views
{
    public partial class FlyoutWindow
    {
        private readonly FlyoutViewModel _viewModel;
        private RawInputListener _rawListener;
        private bool _needsExpandOrCollapse;

        public FlyoutWindow(FlyoutViewModel flyoutViewModel)
        {
            InitializeComponent();

            _viewModel = flyoutViewModel;
            _viewModel.StateChanged += ViewModel_OnStateChanged;
            _viewModel.WindowSizeInvalidated += ViewModel_WindowSizeInvalidated;
            _viewModel.ExpandCollapse = new RelayCommand(() =>
            {
                _needsExpandOrCollapse = true;
                _viewModel.BeginClose();
            });

            DataContext = _viewModel;

            Deactivated += FlyoutWindow_Deactivated;
            SourceInitialized += FlyoutWindow_SourceInitialized;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            Closing += FlyoutWindow_Closing;

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            Show();
            Hide();

            _viewModel.ChangeState(FlyoutViewModel.ViewState.Hidden);
            this.ApplyExtendedWindowStyle(User32.WS_EX_TOOLWINDOW);
        }

        private void FlyoutWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Disable Alt+F4 because we hide instead.
            e.Cancel = true;
            _viewModel.BeginClose();
        }

        ~FlyoutWindow()
        {
            _viewModel.StateChanged -= ViewModel_OnStateChanged;
            _viewModel.WindowSizeInvalidated -= ViewModel_WindowSizeInvalidated;

            Deactivated -= FlyoutWindow_Deactivated;
            SourceInitialized -= FlyoutWindow_SourceInitialized;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => _viewModel.BeginClose()));
        }

        private void FlyoutWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.Cloak();

            Manager.Current.ThemeChanged += ThemeChanged;
            ThemeChanged();

            _rawListener = new RawInputListener(this);
            _rawListener.MouseWheel += RawListener_MouseWheel;
            MouseEnter += FlyoutWindow_MouseEnter;
        }

        private void ThemeChanged()
        {
            EnableBlurIfApplicable();
        }

        private void EnableBlurIfApplicable()
        {
            if ((_viewModel.State == FlyoutViewModel.ViewState.Opening || _viewModel.State == FlyoutViewModel.ViewState.Open) &&
                SystemSettings.IsTransparencyEnabled && !SystemParameters.HighContrast)
            {
                User32.AccentFlags location = User32.AccentFlags.None;

                switch (WindowsTaskbar.Current.Location)
                {
                    case WindowsTaskbar.Position.Left:
                        location = User32.AccentFlags.DrawRightBorder | User32.AccentFlags.DrawTopBorder;
                        break;
                    case WindowsTaskbar.Position.Right:
                        location = User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawTopBorder;
                        break;
                    case WindowsTaskbar.Position.Top:
                        location = User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawBottomBorder;
                        break;
                    case WindowsTaskbar.Position.Bottom:
                        location = User32.AccentFlags.DrawTopBorder | User32.AccentFlags.DrawLeftBorder;
                        break;
                }

                if (SystemSettings.IsRTL)
                {
                    if ((location & User32.AccentFlags.DrawLeftBorder) == User32.AccentFlags.DrawLeftBorder)
                    {
                        location &= ~User32.AccentFlags.DrawLeftBorder;
                        location |= User32.AccentFlags.DrawRightBorder;
                    }
                    else if ((location & User32.AccentFlags.DrawRightBorder) == User32.AccentFlags.DrawRightBorder)
                    {
                        location &= ~User32.AccentFlags.DrawRightBorder;
                        location |= User32.AccentFlags.DrawLeftBorder;
                    }
                }

                AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Flyout"), location);
            }
            else
            {
                DisableAcrylic();
            }
        }

        private void DisableAcrylic()
        {
            AccentPolicyLibrary.DisableAcrylic(this);
        }

        private void FlyoutWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            _rawListener.Stop();
        }

        private void FlyoutWindow_Deactivated(object sender, EventArgs e)
        {
            _viewModel.BeginClose();
        }

        private void ViewModel_WindowSizeInvalidated(object sender, object e)
        {
            UpdateWindowBounds();
        }

        private void RawListener_MouseWheel(object sender, int e)
        {
            if (_viewModel.Devices.Any())
            {
                _viewModel.Devices.Last().Volume += Math.Sign(e) * 2;
            }
        }

        private void ViewModel_OnStateChanged(object sender, object e)
        {
            switch (_viewModel.State)
            {
                case FlyoutViewModel.ViewState.Opening:
                    if (_viewModel.ShowOptions == FlyoutShowOptions.Pointer)
                    {
                        _rawListener.Start();
                    }
                    Show();

                    // We need the theme to be updated on show because the window borders will be set based on taskbar position.
                    ThemeChanged();
                    UpdateWindowBounds();

                    // Update layout otherwise we may display queued state changes
                    UpdateLayout();
                    DevicesList.Focus();

                    WindowAnimationLibrary.BeginFlyoutEntranceAnimation(this, () => _viewModel.ChangeState(FlyoutViewModel.ViewState.Open));
                    break;

                case FlyoutViewModel.ViewState.Closing_Stage1:
                    _rawListener.Stop();

                    if (_needsExpandOrCollapse)
                    {
                        WindowAnimationLibrary.BeginFlyoutExitanimation(this, () =>
                        {
                            this.Cloak();
                            Hide();
                            // NB: Hidden to avoid the stage 2 hide delay, we want to show again immediately.
                            _viewModel.ChangeState(FlyoutViewModel.ViewState.Hidden);
                        });
                    }
                    else
                    {
                        this.Cloak();
                        Hide();
                        DisableAcrylic();
                        _viewModel.ChangeState(FlyoutViewModel.ViewState.Closing_Stage2);
                    }
                    break;
                case FlyoutViewModel.ViewState.Hidden:
                    if (_needsExpandOrCollapse)
                    {
                        _needsExpandOrCollapse = false;

                        _viewModel.DoExpandCollapse();
                        _viewModel.BeginOpen();
                    }
                    break;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_viewModel.Dialog.IsVisible)
                {
                    _viewModel.Dialog.IsVisible = false;
                }
                else
                {
                    _viewModel.BeginClose();
                }
            }
            else
            {
                if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
                {
                    e.Handled = true;
                }
                else
                {
                    KeyboardNavigator.OnKeyDown(this, ref e);
                }
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

            switch(taskbarState.Location)
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

        private void LightDismissBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.Dialog.IsVisible = false;
            e.Handled = true;
        }
    }
}
