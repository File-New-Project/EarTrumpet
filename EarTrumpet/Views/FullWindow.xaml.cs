using EarTrumpet.Extensions;
using EarTrumpet.UserControls;
using EarTrumpet.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet
{
    public partial class FullWindow : Window
    {
        public static FullWindow Instance { get; private set; }

        private FullWindowViewModel _viewModel;
        private VolumeControlPopup _popup;

        public FullWindow(MainViewModel viewModel)
        {
            Instance = this;

            _viewModel = new FullWindowViewModel(viewModel);
            _viewModel.AppExpanded += (_, e) => _popup.PositionAndShow(this, e);
            _viewModel.AppCollapsed += (_, __) => _popup.IsOpen = false;

            InitializeComponent();

            Title = Properties.Resources.FullWindowTitleText;
            _popup = AppPopup;
            _popup.Closed += (_, __) => _viewModel.OnAppCollapsed();

            LocationChanged += (_, __) => _viewModel.OnAppCollapsed();
            SizeChanged += (_, __) => _viewModel.OnAppCollapsed();

            DataContext = _viewModel;

            this.StateChanged += FullWindow_StateChanged;

            Activated += (_, __) => SizeToContent = SizeToContent.Manual;

            Instance = this;
            Closing += (s, e) =>
            {
                Instance = null;
                _viewModel.Close();
            };

            SourceInitialized += (_, __) => this.SetWindowBlur(true, true);
        }

        private void FullWindow_StateChanged(object sender, System.EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }

            _viewModel.OnAppCollapsed();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeviceAndAppsControl_AppExpanded(object sender, UserControls.AppVolumeControlExpandedEventArgs e)
        {
            _viewModel.OnAppExpanded(e.ViewModel, e.Container);
        }

        private void LightDismissBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _popup.HideWithAnimation();
            e.Handled = true;
        }
    }
}
