using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace EarTrumpet
{
    public partial class MainWindow
    {
        private readonly IAudioDeviceManager _deviceService;
        private readonly MainViewModel _viewModel;
        private readonly TrayViewModel _trayViewModel;
        private readonly TrayIcon _trayIcon;

        public MainWindow()
        {
            InitializeComponent();
            _deviceService = new AudioDeviceManager(Dispatcher);
            _viewModel = new MainViewModel(_deviceService);
            _trayViewModel = new TrayViewModel(_deviceService);
            _trayIcon = new TrayIcon(_deviceService, _trayViewModel);
            _trayIcon.Invoked += TrayIcon_Invoked;

            DataContext = _viewModel;

            // Move keyboard focus to the first element. Disabled this since it is ugly but not sure invisible
            // visuals are preferrable.
            // Activated += (s,e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            SourceInitialized += (s, e) =>
            {
                UpdateTheme();
                ThemeService.RegisterForThemeChanges(new WindowInteropHelper(this).Handle);
            };

            ThemeService.ThemeChanged += () => UpdateTheme();

            CreateAndHideWindow();
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
                this.HideWithAnimation();
                _viewModel.IsVisible = false;
            }
            else
            {
                _viewModel.UpdateInterfaceState();
                UpdateTheme();
                UpdateWindowPosition();
                this.ShowwithAnimation();
                _viewModel.IsVisible = true;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
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

            var taskbarState = TaskbarService.GetWinTaskbarState();
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

        private void PlaybackDevices_Click(object sender, RoutedEventArgs e)
        {
            var cm = new ContextMenu();

            foreach (var dev in _deviceService.Devices)
            {
                var cmItem = new MenuItem { Header = dev.DisplayName };
                cmItem.Click += (s, _) => _deviceService.DefaultPlaybackDevice = dev;
                cmItem.IsChecked = dev.Id == _deviceService.DefaultPlaybackDevice.Id;
                cm.Items.Add(cmItem);
            }

            cm.PlacementTarget = (UIElement)sender;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            cm.IsOpen = true;
        }

        private void CommunicationDevices_Click(object sender, RoutedEventArgs e)
        {
            var cm = new ContextMenu();

            foreach (var dev in _deviceService.Devices)
            {
                var cmItem = new MenuItem { Header = dev.DisplayName };
                cmItem.Click += (s, _) => _deviceService.DefaultCommunicationDevice = dev;
                cmItem.IsChecked = dev.Id == _deviceService.DefaultCommunicationDevice.Id;
                cm.Items.Add(cmItem);
            }

            cm.PlacementTarget = (UIElement)sender;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            cm.IsOpen = true;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsWindow.Instance == null)
            {
                new SettingsWindow(_deviceService).Show();
            }
            else
            {
                SettingsWindow.Instance.RaiseWindow();
            }
        }

        private void PopOut_Click(object sender, RoutedEventArgs e)
        {
            new FullWindow(_deviceService).Show();
        }
    }
}
