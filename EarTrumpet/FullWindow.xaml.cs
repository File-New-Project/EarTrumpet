using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet
{
    public partial class FullWindow : Window
    {
        FullWindowViewModel _viewModel;
        IAudioDeviceManager _manager;

        public FullWindow(IAudioDeviceManager manager)
        {
            _manager = manager;
            InitializeComponent();

            Title = Properties.Resources.FullWindowTitleText;
            _viewModel = new FullWindowViewModel(manager);
            DataContext = _viewModel;

            ThemeService.ThemeChanged += UpdateTheme;

            SourceInitialized += (s, e) =>
            {
                UpdateTheme();
            };

            this.StateChanged += FullWindow_StateChanged;
        }

        private void FullWindow_StateChanged(object sender, System.EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void UpdateTheme()
        {
            ThemeService.UpdateThemeResources(Resources);
        }

        private void ToggleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            item.IsChecked = !item.IsChecked;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
