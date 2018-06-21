using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Controls;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.UI.Views
{
    public partial class FlyoutWindow
    {
        private readonly MainViewModel _mainViewModel;
        private readonly FlyoutViewModel _viewModel;
        private RawInputListener _rawListener;

        internal FlyoutWindow(MainViewModel mainViewModel, FlyoutViewModel flyoutViewModel)
        {
            InitializeComponent();

            _mainViewModel = mainViewModel;
            _viewModel = flyoutViewModel;

            _viewModel.StateChanged += ViewModel_OnStateChanged;
            _viewModel.WindowSizeInvalidated += ViewModel_WindowSizeInvalidated;
            _viewModel.AppExpanded += ViewModel_AppExpanded;
            _viewModel.AppCollapsed += ViewModel_AppCollapsed;

            DataContext = _viewModel;

            AppPopup.Closed += AppPopup_Closed;
            Deactivated += FlyoutWindow_Deactivated;
            SourceInitialized += FlyoutWindow_SourceInitialized;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            Closing += FlyoutWindow_Closing;

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            Show();
            Hide();

            _viewModel.ChangeState(FlyoutViewModel.ViewState.Hidden);
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
            _viewModel.AppExpanded -= ViewModel_AppExpanded;
            _viewModel.AppCollapsed -= ViewModel_AppCollapsed;

            AppPopup.Closed -= AppPopup_Closed;
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

            var themeManager = (ThemeManager)App.Current.Resources["ThemeManager"];
            themeManager.ThemeChanged += ThemeChanged;
            ThemeChanged();

            _rawListener = new RawInputListener(this);
            _rawListener.MouseWheel += RawListener_MouseWheel;
            MouseEnter += FlyoutWindow_MouseEnter;
            MouseLeave += FlyoutWindow_MouseLeave;
        }

        private void ThemeChanged()
        {
            AccentPolicyLibrary.SetWindowBlur(this, SystemSettings.IsTransparencyEnabled && !SystemParameters.HighContrast);
        }

        private void FlyoutWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            _rawListener.Start();
        }

        private void FlyoutWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            _rawListener.Stop();
        }

        private void AppPopup_Closed(object sender, EventArgs e)
        {
            _viewModel.CollapseApp();
        }

        private void FlyoutWindow_Deactivated(object sender, EventArgs e)
        {
            _viewModel.BeginClose();
        }

        private void ViewModel_AppCollapsed(object sender, object e)
        {
            AppPopup.HideWithAnimation();
        }

        private void ViewModel_AppExpanded(object sender, AppExpandedEventArgs e)
        {
            AppPopup.PositionAndShow(this, e);
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

        private void ViewModel_OnStateChanged(object sender, FlyoutViewModel.CloseReason e)
        {
            switch (_viewModel.State)
            {
                case FlyoutViewModel.ViewState.Opening:
                    _rawListener.Start();
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

                    if (e == FlyoutViewModel.CloseReason.CloseThenOpen)
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
                        _viewModel.ChangeState(FlyoutViewModel.ViewState.Closing_Stage2);
                    }
                    break;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_viewModel.IsShowingModalDialog)
                {
                    _viewModel.CollapseApp();
                }
                else
                {
                    _viewModel.BeginClose();
                }
            }
            else
            {
                KeyboardNavigator.OnKeyDown(this, ref e);
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

            double newHeight = 0;
            if (_viewModel.IsEmpty)
            {
                newHeight = (double)App.Current.Resources["NoItemsPaneHeight"];
            }
            else
            {
                var DeviceItemCellHeight = (double)App.Current.Resources["DeviceItemCellHeight"];
                var DeviceTitleCellHeight = (double)App.Current.Resources["DeviceTitleCellHeight"];
                var AppItemCellHeight = (double)App.Current.Resources["AppItemCellHeight"];

                var VolumeAppListMargin = (Thickness)App.Current.Resources["VolumeAppListMargin"];
                foreach (var device in _viewModel.Devices)
                {
                    newHeight += DeviceTitleCellHeight + DeviceItemCellHeight;

                    if (device.Apps.Count > 0)
                    {
                        newHeight += VolumeAppListMargin.Bottom + VolumeAppListMargin.Top;
                    }

                    foreach(var app in device.Apps)
                    {
                        newHeight += AppItemCellHeight;
                    }
                }
            }

            bool isOverflowing = false;
            if (newHeight > taskbarState.ContainingScreen.WorkingArea.Height)
            {
                newHeight = taskbarState.ContainingScreen.WorkingArea.Height;
                isOverflowing = true;
            }

            BaseVisual.VerticalScrollBarVisibility = isOverflowing ? System.Windows.Controls.ScrollBarVisibility.Visible : System.Windows.Controls.ScrollBarVisibility.Hidden;

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
            AppPopup.HideWithAnimation();
            e.Handled = true;
        }

        private void DeviceAndAppsControl_AppExpanded(object sender, AppVolumeControlExpandedEventArgs e)
        {
            _viewModel.BeginExpandApp(e.ViewModel, e.Container);
        }
    }
}
