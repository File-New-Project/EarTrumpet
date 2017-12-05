using EarTrumpet.DataModel;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System.Windows;
using System;

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
    }
}
