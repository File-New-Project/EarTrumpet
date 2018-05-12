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

            this.StateChanged += FullWindow_StateChanged;
        }

        private void FullWindow_StateChanged(object sender, System.EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
