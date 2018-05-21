using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace EarTrumpet.Views
{
    public partial class FlyoutWindow
    {
        private readonly MainViewModel _mainViewModel;
        private readonly FlyoutViewModel _viewModel;
        private readonly ThemeService _themeService;
        private VolumeControlPopup _popup;
        private bool _expandOnCloseThenOpen;
        private RawInputListener _rawListener;

        public FlyoutWindow(MainViewModel mainViewModel, IAudioDeviceManager manager, ThemeService themeService)
        {
            _mainViewModel = mainViewModel;
            _themeService = themeService;

            InitializeComponent();

            _viewModel = new FlyoutViewModel(mainViewModel, manager);
            _viewModel.StateChanged += OnStateChanged;
            _viewModel.WindowSizeInvalidated += (_, __) => UpdateWindowBounds();

            _viewModel.AppExpanded += OnAppExpanded;
            _viewModel.AppCollapsed += (_, __) => OnAppCollapsed();

            DataContext = _viewModel;

            _popup = AppPopup;
            _popup.Closed += (_, __) => _viewModel.OnAppCollapsed();

            Deactivated += (_, __) => _viewModel.BeginClose();

            PreviewKeyDown += (_, e) => KeyboardNavigator.OnKeyDown(this, ref e);

            SourceInitialized += (s, e) =>
            {
                this.Cloak();

                themeService.RegisterForThemeChanges(new WindowInteropHelper(this).Handle);

                UpdateTheme();

                _rawListener = new RawInputListener(this);
                _rawListener.MouseWheel += RawListener_MouseWheel;
                MouseEnter += (_, __) => _rawListener.Stop();
            };

            themeService.ThemeChanged += () => UpdateTheme();

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

        private void OnStateChanged(object sender, FlyoutViewModel.ViewState e)
        {
            switch (e)
            {
                case FlyoutViewModel.ViewState.Opening:

                    Show();
                    UpdateWindowBounds();
                    DevicesList.Focus();

                    WindowAnimationLibrary.BeginFlyoutEntranceAnimation(this, () => _viewModel.ChangeState(FlyoutViewModel.ViewState.Open));

                    _rawListener.Start();

                    break;

                case FlyoutViewModel.ViewState.Closing:

                    _rawListener.Stop();

                    var cloakAndMarkHidden = new Action(() =>
                    {
                        this.Cloak();
                        Hide();
                        _viewModel.ChangeState(FlyoutViewModel.ViewState.Hidden);
                    });

                    if (_expandOnCloseThenOpen)
                    {
                        WindowAnimationLibrary.BeginFlyoutExitanimation(this, cloakAndMarkHidden);
                    }
                    else
                    {
                        cloakAndMarkHidden();
                    }

                    break;

                case FlyoutViewModel.ViewState.Hidden:

                    if (_expandOnCloseThenOpen)
                    {
                        _expandOnCloseThenOpen = false;

                        _viewModel.DoExpandCollapse();
                        _viewModel.BeginOpen();
                    }
                    break;
            }
        }

        public void OpenAsFlyout()
        {
            switch (_viewModel.State)
            {
                case FlyoutViewModel.ViewState.Hidden:
                    _viewModel.BeginOpen();
                    break;
                case FlyoutViewModel.ViewState.Open:
                    _viewModel.BeginClose();
                    break;
            }
        }

        private void OnAppExpanded(object sender, AppExpandedEventArgs e)
        {
            _popup.PositionAndShow(this, e);

            LightDismissBorder.Visibility = Visibility.Visible;

            if (!SystemParameters.MenuAnimation)
            {
                return;
            }

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 0,
                To = 1,
            };

            Storyboard.SetTarget(fadeAnimation, LightDismissBorder);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.HoldEnd;
            storyboard.Children.Add(fadeAnimation);
            storyboard.Begin(LightDismissBorder);
        }

        private void OnAppCollapsed()
        {
            _popup.HideWithAnimation();

            if (!SystemParameters.MenuAnimation)
            {
                LightDismissBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 1,
                To = 0,
            };

            Storyboard.SetTarget(fadeAnimation, LightDismissBorder);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(fadeAnimation);
            storyboard.Completed += (s, e) =>
            {
                LightDismissBorder.Visibility = Visibility.Collapsed;
                LightDismissBorder.Opacity = 1;
            };
            storyboard.Begin(LightDismissBorder);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_viewModel.IsShowingModalDialog)
                {
                    _viewModel.OnAppCollapsed();
                }
                else
                {
                    _viewModel.BeginClose();
                }
            }
        }

        private void UpdateTheme()
        {
            this.SetWindowBlur(UserSystemPreferencesService.IsTransparencyEnabled && !SystemParameters.HighContrast);
        }

        private void UpdateWindowBounds()
        {
            var taskbarState = TaskbarService.GetWinTaskbarState();

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
            if (newHeight > taskbarState.TaskbarScreen.WorkingArea.Height)
            {
                newHeight = taskbarState.TaskbarScreen.WorkingArea.Height;
                isOverflowing = true;
            }

            BaseVisual.VerticalScrollBarVisibility = isOverflowing ? System.Windows.Controls.ScrollBarVisibility.Visible : System.Windows.Controls.ScrollBarVisibility.Hidden;

            double newTop = 0;
            double newLeft = 0;
            switch(taskbarState.TaskbarPosition)
            {
                case TaskbarPosition.Left:
                    newLeft = (taskbarState.TaskbarSize.Right / this.DpiWidthFactor());
                    newTop = (taskbarState.TaskbarSize.Bottom / this.DpiHeightFactor()) - newHeight;
                    break;
                case TaskbarPosition.Right:
                    newLeft = (taskbarState.TaskbarSize.Left / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.TaskbarSize.Bottom / this.DpiHeightFactor()) - newHeight;
                    break;
                case TaskbarPosition.Top:
                    newLeft = (taskbarState.TaskbarSize.Right / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.TaskbarSize.Bottom / this.DpiHeightFactor());
                    break;
                case TaskbarPosition.Bottom:
                    newLeft = (taskbarState.TaskbarSize.Right / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.TaskbarSize.Top / this.DpiHeightFactor()) - newHeight;
                    break;
            }

            this.Move(newTop * this.DpiHeightFactor(), newLeft * this.DpiWidthFactor(), newHeight * this.DpiHeightFactor(), Width * this.DpiWidthFactor());
        }

        private void ExpandCollapse_Click(object sender, RoutedEventArgs e)
        {
            _expandOnCloseThenOpen = true;

            _viewModel.BeginClose();
        }

        private void LightDismissBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _popup.HideWithAnimation();
            e.Handled = true;
        }

        private void DeviceAndAppsControl_AppExpanded(object sender, AppVolumeControlExpandedEventArgs e)
        {
            _viewModel.OnAppExpanded(e.ViewModel, e.Container);
        }
    }
}
