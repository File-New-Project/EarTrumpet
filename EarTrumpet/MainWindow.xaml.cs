using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace EarTrumpet
{
    public partial class MainWindow
    {
        private readonly IAudioDeviceManager _deviceService;
        private readonly MainViewModel _viewModel;
        private readonly TrayViewModel _trayViewModel;
        private readonly TrayIcon _trayIcon;

        private Popup _secondaryUI;

        public MainWindow()
        {
            InitializeComponent();
            _deviceService = new AudioDeviceManager(Dispatcher);
            _viewModel = new MainViewModel(_deviceService);
            _viewModel.SessionPopup += _viewModel_SessionPopup;
            _trayViewModel = new TrayViewModel(_deviceService);
            _trayIcon = new TrayIcon(_deviceService, _trayViewModel);
            _trayIcon.Invoked += TrayIcon_Invoked;

            DataContext = _viewModel;

            SourceInitialized += (s, e) =>
            {
                UpdateTheme();
                ThemeService.RegisterForThemeChanges(new WindowInteropHelper(this).Handle);
            };

            ThemeService.ThemeChanged += () => UpdateTheme();

            CreateAndHideWindow();

            var Hotkey = SettingsService.Hotkey;
            HotkeyService.Register(Hotkey.Modifiers, Hotkey.Key);
        }

        private void _viewModel_SessionPopup(object sender, SessionPopupEventArgs e)
        {
            if (_secondaryUI == null)
            {
                _secondaryUI = (Popup)Resources["SecondaryUIPopup"];
                LayoutRoot.Children.Add(_secondaryUI);
            }

            _secondaryUI.Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute;
            _secondaryUI.HorizontalOffset = this.PointToScreen(new Point(0, 0)).X;
            _secondaryUI.VerticalOffset = this.PointToScreen(new Point(0, 0)).Y;
            _secondaryUI.Height = ActualHeight;
            _secondaryUI.Width = ActualWidth;
            _secondaryUI.DataContext = e.ViewModel;
            _secondaryUI.AllowsTransparency = true;
            
            _secondaryUI.Opened += (_, __) =>
            {
                if (ThemeService.IsWindowTransparencyEnabled && !SystemParameters.HighContrast)
                {
                    _secondaryUI.EnableBlur();
                }
                else
                {
                    _secondaryUI.DisableBlur();
                    OverlayGrid.Background = (Brush)Resources["DimmedBackground"];
                }
            };
            _secondaryUI.IsOpen = true;

        }

        private void CreateAndHideWindow()
        {
            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            Opacity = 0;
            Show();
            Hide();
            Opacity = 1;
        }

        void TrayIcon_Invoked()
        {
            if (this.Visibility == Visibility.Visible)
            {
                DismissSecondaryUI_Click(null, null);

                this.HideWithAnimation();
                _viewModel.IsVisible = false;
            }
            else
            {
                _viewModel.UpdateInterfaceState();
                UpdateTheme();
                UpdateWindowPosition();
                this.ShowwithAnimation();
                DefaultDeviceControl.HideFocus();
                _viewModel.IsVisible = true;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            DismissSecondaryUI_Click(null, null);

            
            this.HideWithAnimation();
            _viewModel.IsVisible = false;
        }
        
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.HideWithAnimation();
                _viewModel.IsVisible = false;
            }
        }

        private void UpdateTheme()
        {
            // Call UpdateTheme before UpdateWindowPosition in case sizes change with the theme.
            ThemeService.UpdateThemeResources(Resources);

            if (ThemeService.IsWindowTransparencyEnabled && !SystemParameters.HighContrast)
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
            var taskbarState = TaskbarService.GetWinTaskbarState();

            LayoutRoot.UpdateLayout();
            LayoutRoot.Measure(new Size(double.PositiveInfinity, taskbarState.TaskbarScreen.WorkingArea.Height));
            Height = LayoutRoot.DesiredSize.Height;

            
            switch(taskbarState.TaskbarPosition)
            {
                case TaskbarPosition.Left:
                    Left = (taskbarState.TaskbarSize.right / this.DpiWidthFactor());
                    Top = (taskbarState.TaskbarSize.bottom / this.DpiHeightFactor()) - Height;
                    break;
                case TaskbarPosition.Right:
                    Left = (taskbarState.TaskbarSize.left / this.DpiWidthFactor()) - Width;
                    Top = (taskbarState.TaskbarSize.bottom / this.DpiHeightFactor()) - Height;
                    break;
                case TaskbarPosition.Top:
                    Left = (taskbarState.TaskbarSize.right / this.DpiWidthFactor()) - Width;
                    Top = (taskbarState.TaskbarSize.bottom / this.DpiHeightFactor());
                    break;
                case TaskbarPosition.Bottom:
                    Left = (taskbarState.TaskbarSize.right / this.DpiWidthFactor()) - Width;
                    Top = (taskbarState.TaskbarSize.top / this.DpiHeightFactor()) - Height;
                    break;
            }            
        }

        private void ExpandCollapse_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DoExpandCollapse();
            UpdateWindowPosition();
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            _trayIcon.Exit_Click(this, null);
        }

        private void DismissSecondaryUI_Click(object sender, RoutedEventArgs e)
        {
            if (_secondaryUI != null)
            {
                _secondaryUI.DataContext = null;
                _secondaryUI.IsOpen = false;
                LayoutRoot.Children.Remove(_secondaryUI);
                _secondaryUI = null;
            }
            OverlayGrid.Background = null;
        }

        private void MoveToAnotherDevice_Click(object sender, RoutedEventArgs e)
        {
            var selectedApp = (AppItemViewModel)_secondaryUI.DataContext;

            var currentDeviceId = selectedApp.Session.Device.Id;

            var moveMenu = new ContextMenu();

            var addDevice = new Action<DeviceViewModel>((device) =>
            {
                if (device.Device.Id == currentDeviceId) return;

                var newItem = new MenuItem { Header = device.Device.DisplayName };
                newItem.Click += (_, __) => device.TakeExternalSession(selectedApp);
                moveMenu.Items.Add(newItem);
            });

            foreach(var device in _viewModel.Devices)
            {
                addDevice(device);
            }

            addDevice(_viewModel.DefaultDevice);

            moveMenu.PlacementTarget = (UIElement)sender;
            moveMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            moveMenu.IsOpen = true;
        }
    }
}
