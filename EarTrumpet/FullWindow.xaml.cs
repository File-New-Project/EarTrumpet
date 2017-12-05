using EarTrumpet.DataModel;
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

            _viewModel = new FullWindowViewModel(manager);
            DataContext = _viewModel;

            ThemeService.ThemeChanged += UpdateTheme;

            UpdateTheme();
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
    }
}
