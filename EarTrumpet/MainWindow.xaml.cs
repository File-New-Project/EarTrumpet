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

        public MainWindow()
        {
            InitializeComponent();
            _deviceService = new AudioDeviceManager(Dispatcher);
            _viewModel = new MainViewModel(_deviceService);
            _viewModel.StateChanged += _viewModel_StateChanged;
            _trayViewModel = new TrayViewModel(_deviceService);
            _trayIcon = new TrayIcon(_deviceService, _trayViewModel);
            _trayIcon.Invoked += TrayIcon_Invoked;

            DataContext = _viewModel;

            SourceInitialized += (s, e) =>
            {
                UpdateTheme();
                ThemeService.RegisterForThemeChanges(new WindowInteropHelper(this).Handle);
            };

            ContentGrid.SizeChanged += (s, e) => UpdateWindowPosition();

            ThemeService.ThemeChanged += () => UpdateTheme();

            CreateAndHideWindow();

            var Hotkey = SettingsService.Hotkey;
            HotkeyService.Register(Hotkey.Modifiers, Hotkey.Key);
        }

        private void _viewModel_StateChanged(object sender, ViewState e)
        {
            switch (e)
            {
                case ViewState.Opening:
                    UpdateTheme();
                    UpdateWindowPosition();
                    this.ShowwithAnimation(() => _viewModel.ChangeState(ViewState.Open));

                    DefaultDeviceControl.HideFocus();
                    break;

                case ViewState.Closing:
                    this.HideWithAnimation(() => _viewModel.ChangeState(ViewState.Hidden));
                    break;

                case ViewState.Hidden:

                    // Hide the app only after we've completed the animation.
                    if (MainViewModel.ExpandedApp != null)
                    {
                        MainViewModel.ExpandedApp.IsExpanded = false;
                        MainViewModel.ExpandedApp = null;
                    }

                    UpdateWindowPosition();
                    break;
            }
        }

        private void CreateAndHideWindow()
        {
            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            Opacity = 0;
            Show();
            Hide();
            Opacity = 1;

            _viewModel.ChangeState(ViewState.Hidden);
        }

        void TrayIcon_Invoked()
        {
            switch (_viewModel.State)
            {
                case ViewState.Hidden:
                    _viewModel.BeginOpen();
                    break;
                case ViewState.Open:
                    _viewModel.BeginClose();
                    break;
                default:
                    // For transition states, we ignore the event.
                    break;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            _viewModel.BeginClose();
        }
        
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _viewModel.BeginClose();
            }
        }

        private void UpdateTheme()
        {
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
        }

        private void ExpandCollapse_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                // Top of window - don't wrap around.
                e.Handled = true;
            }
        }
    }
}
