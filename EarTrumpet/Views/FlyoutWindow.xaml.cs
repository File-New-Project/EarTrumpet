using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace EarTrumpet.Views
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
            _viewModel.StateChanged += OnStateChanged;
            _viewModel.WindowSizeInvalidated += (_, __) => UpdateWindowBounds();
            _viewModel.AppExpanded += (_, e) => AppPopup.PositionAndShow(this, e);
            _viewModel.AppCollapsed += (_, __) => AppPopup.HideWithAnimation();

            DataContext = _viewModel;

            AppPopup.Closed += (_, __) => _viewModel.CollapseApp();
            Deactivated += (_, __) => _viewModel.BeginClose();

            this.FlowDirection = UserSystemPreferencesService.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            SourceInitialized += (s, e) =>
            {
                this.Cloak();

                var themeManager = (ThemeManager)App.Current.Resources["ThemeManager"];
                themeManager.RegisterForThemeChanges(new WindowInteropHelper(this).Handle);
                themeManager.ThemeChanged += () => UpdateTheme();
                UpdateTheme();

                _rawListener = new RawInputListener(this);
                _rawListener.MouseWheel += RawListener_MouseWheel;
                MouseEnter += (_, __) => _rawListener.Stop();
            };

            // Disable Alt+F4 because we hide instead.
            Closing += (_, e) =>
            {
                e.Cancel = true;
                _viewModel.BeginClose();
            };

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (s, e) => Dispatcher.SafeInvoke(() => _viewModel.BeginClose());

            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            Show();
            Hide();

            _viewModel.ChangeState(FlyoutViewModel.ViewState.Hidden);
        }

        private void RawListener_MouseWheel(object sender, int e)
        {
            if (_viewModel.Devices.Any())
            {
                _viewModel.Devices.Last().Volume += Math.Sign(e) * 2;
            }
        }

        private void OnStateChanged(object sender, FlyoutViewModel.CloseReason e)
        {
            switch (_viewModel.State)
            {
                case FlyoutViewModel.ViewState.Opening:
                    _rawListener.Start();
                    Show();
                    UpdateWindowBounds();
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

        private void UpdateTheme()
        {
            this.SetWindowBlur(UserSystemPreferencesService.IsTransparencyEnabled && !SystemParameters.HighContrast);
        }

        private void UpdateWindowBounds()
        {
            var taskbarState = WindowsTaskbar.Current;

            if (taskbarState.ContainingScreen == null)
            {
                // we're not ready to lay out. (e.g. RDP transition)
                return;
            }

            double newHeight = 0;
            if (_viewModel.IsEmpty)
            {
                var NoItemsPaneHeight = (double)App.Current.Resources["NoItemsPaneHeight"];
                var NoItemsPaneMargin = (Thickness)App.Current.Resources["NoItemsPaneMargin"];

                newHeight = NoItemsPaneHeight + NoItemsPaneMargin.Bottom + NoItemsPaneMargin.Top;
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

            bool isRTL = UserSystemPreferencesService.IsRTL;
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
