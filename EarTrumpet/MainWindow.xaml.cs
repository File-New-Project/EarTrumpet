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
using System.Windows.Media.Effects;

namespace EarTrumpet
{
    public partial class MainWindow
    {
        private readonly IAudioDeviceManager _deviceService;
        private readonly MainViewModel _viewModel;
        private readonly TrayViewModel _trayViewModel;
        private readonly TrayIcon _trayIcon;
        private Popup _popup;

        public MainWindow()
        {
            InitializeComponent();
            _deviceService = new AudioDeviceManager(Dispatcher);
            _viewModel = new MainViewModel(_deviceService);
            _viewModel.StateChanged += _viewModel_StateChanged;
            _viewModel.AppExpanded += _viewModel_AppExpanded;
            _viewModel.AppCollapsed += _viewModel_AppCollapsed;
            _trayViewModel = new TrayViewModel(_deviceService);
            _trayIcon = new TrayIcon(_deviceService, _trayViewModel);
            _trayIcon.Invoked += TrayIcon_Invoked;

            DataContext = _viewModel;

            SourceInitialized += (s, e) =>
            {
                ThemeService.RegisterForThemeChanges(new WindowInteropHelper(this).Handle);

                _popup = (Popup)Resources["AppPopup"];
            };

            ThemeService.ThemeChanged += () => UpdateTheme();

            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            CreateAndHideWindow();

            var Hotkey = SettingsService.Hotkey;
            HotkeyService.Register(Hotkey.Modifiers, Hotkey.Key);

            ContentGrid.SizeChanged += (s, e) => UpdateWindowPosition();
        }

        private void _viewModel_AppCollapsed(object sender, object e)
        {
            LayoutRoot.Children.Remove(_popup);
            _popup.IsOpen = false;

            MainViewModel.ExpandedApp.IsExpanded = false;
            MainViewModel.ExpandedApp = null;

            UpdateLayout();
        }

        private void _viewModel_AppExpanded(object sender, ListViewItem container)
        {
            var selectedApp = MainViewModel.ExpandedApp;

            _popup.DataContext = selectedApp;
            LayoutRoot.Children.Add(_popup);

            Point relativeLocation = container.TranslatePoint(new Point(0, 0), this);

            // TODO: This is fudge factor that we can at least partially calculate.
            double HEADER_SIZE = 44;
            double ITEM_SIZE = 50;
            relativeLocation.Y -= HEADER_SIZE;

            var popupHeight = HEADER_SIZE + (selectedApp.ChildApps.Count * ITEM_SIZE);

            if (relativeLocation.Y + popupHeight > ActualHeight)
            {
                relativeLocation.Y = ActualHeight - popupHeight;
            }

            _popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute;
            _popup.HorizontalOffset = this.PointToScreen(new Point(0, 0)).X;
            _popup.VerticalOffset = this.PointToScreen(new Point(0, 0)).Y + relativeLocation.Y;

            _popup.Width = ActualWidth;
            _popup.Height = popupHeight;

            _popup.AllowsTransparency = true;

            _popup.IsOpen = true;
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
                    this.Visibility = Visibility.Hidden;
                    _viewModel.ChangeState(ViewState.Hidden);
                    break;
            }
        }

        private void CreateAndHideWindow()
        {
            Opacity = 0;
            Height = 1;
            UpdateTheme();
            Show();
            Hide();
            Opacity = 0.5;

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
            ThemeService.LoadCurrentTheme();

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

            LayoutRoot.Measure(new Size(double.PositiveInfinity, taskbarState.TaskbarScreen.WorkingArea.Height));

            double newHeight = LayoutRoot.DesiredSize.Height;
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

            this.Move(newTop, newLeft, newHeight, Width);
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

        private void LightDismissBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.OnAppCollapsed();
            e.Handled = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.OnAppCollapsed();
        }

        private void MoveToAnotherDevice_Click(object sender, RoutedEventArgs e)
        {
            var selectedApp = MainViewModel.ExpandedApp;
            var vm = MainViewModel.Instance;
            var persistedDevice = selectedApp.PersistedOutputDevice;

            var moveMenu = new ContextMenu();

            MenuItem defaultItem = null;
            foreach (var dev in selectedApp.Devices)
            {
                var newItem = new MenuItem { Header = dev.DisplayName };
                newItem.Click += (_, __) =>
                {
                    AudioPolicyConfigService.SetDefaultEndPoint(dev.Id, selectedApp.Session.ProcessId);
                };

                newItem.IsCheckable = true;
                newItem.IsChecked = (dev.Id == persistedDevice.Id);

                if (dev.IsDefault)
                {
                    defaultItem = newItem;
                }

                moveMenu.Items.Add(newItem);
            }

            moveMenu.Items.Remove(defaultItem);
            moveMenu.Items.Insert(0, defaultItem);
            moveMenu.Items.Insert(1, new Separator());

            moveMenu.PlacementTarget = (UIElement)sender;
            moveMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            moveMenu.IsOpen = true;
        }
    }
}
