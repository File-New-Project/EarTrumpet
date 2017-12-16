using EarTrumpet.DataModel;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System.Windows;
using System;
using System.Windows.Controls;
using EarTrumpet.Extensions;

namespace EarTrumpet
{
    public partial class SettingsWindow : Window
    {
        public static SettingsWindow Instance;

        SettingsViewModel _viewModel;

        public SettingsWindow(IAudioDeviceManager manager)
        {
            InitializeComponent();

            Title = Properties.Resources.SettingsWindowText;
            _viewModel = new SettingsViewModel(manager);
            DataContext = _viewModel;

            ThemeService.ThemeChanged += UpdateTheme;

            SourceInitialized += (s, e) => UpdateTheme();

            Instance = this;
            Closing += (s, e) =>
            {
                 Instance = null;
                _viewModel.Save();
            };
        }

        ~SettingsWindow()
        {
            ThemeService.ThemeChanged -= UpdateTheme;
        }

        void UpdateTheme()
        {
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

        internal void RaiseWindow()
        {
            Topmost = true;
            Activate();
            Topmost = false;
        }

        private void AddApp_Click(object sender, RoutedEventArgs e)
        {
            var cm = new ContextMenu();

            foreach (var app in _viewModel.EnumerateApps())
            {
                var cmItem = new MenuItem { Header = app.DisplayName };
                cmItem.Header = app.DisplayName;
                cmItem.Click += (s, _) => _viewModel.AddApp(app.Session);
                cm.Items.Add(cmItem);
            }

            if (cm.Items.Count == 0)
            {
                var cmItem = new MenuItem { Header = Properties.Resources.NoAppsPlayingSoundText };
                cmItem.IsEnabled = false;
                cm.Items.Add(cmItem);
            }

            cm.PlacementTarget = (UIElement)sender;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            cm.IsOpen = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RemoveApp_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DefaultApps.Remove((AudioSessionViewModel)((Button)sender).DataContext);
        }

        private void HotkeySelect_Click(object sender, RoutedEventArgs e)
        {
            var win = new HotkeySelectionWindow(_viewModel.Hotkey);
            win.Owner = this;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ShowDialog();
            _viewModel.Hotkey = win.Hotkey;
        }
    }
}
