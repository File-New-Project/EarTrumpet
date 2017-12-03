using EarTrumpet.DataModel;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

            ThemeService.UpdateThemeResources(Resources);
        }

        private void ToggleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            item.IsChecked = !item.IsChecked;
        }
    }
}
