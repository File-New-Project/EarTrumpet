using EarTrumpet.DataModel;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System.Windows;
using System;
using System.Windows.Controls;

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
            Closing += (s, e) => Instance = null;
        }

        

        void UpdateTheme()
        {

        }

        internal void RaiseWindow()
        {
            Topmost = true;
            Activate();
            Topmost = false;
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            var cm = new ContextMenu();

            foreach(var dev in _viewModel.EnumerateDevices())
            {
                var cmItem = new MenuItem { Header = dev.DisplayName };
                cmItem.Click += (s, _) => _viewModel.AddDevice(dev);
                cm.Items.Add(cmItem);
            }

            cm.PlacementTarget = (UIElement)sender;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            cm.IsOpen = true;
        }

        private void AddApp_Click(object sender, RoutedEventArgs e)
        {
            var cm = new ContextMenu();

            foreach (var app in _viewModel.EnumerateApps())
            {
                var cmItem = new MenuItem { Header = app.DisplayName };
                cmItem.Click += (s, _) => _viewModel.AddApp(app);
                cm.Items.Add(cmItem);
            }

            cm.PlacementTarget = (UIElement)sender;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            cm.IsOpen = true;
        }
    }
}
