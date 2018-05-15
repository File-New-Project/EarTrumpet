using EarTrumpet.ViewModels;
using System.Windows;

namespace EarTrumpet
{
    public partial class FullWindow : Window
    {
        MainViewModel _viewModel;

        public FullWindow(MainViewModel viewModel)
        {
            _viewModel = viewModel;

            InitializeComponent();

            Title = Properties.Resources.FullWindowTitleText;

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
