using EarTrumpet.Extensions;
using EarTrumpet.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.Views
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
            _viewModel.AppCollapsed += (_, __) => _popup.HideWithAnimation();

            InitializeComponent();

            Title = Properties.Resources.FullWindowTitleText;
            _popup = AppPopup;
            _popup.Closed += (_, __) => _viewModel.OnAppCollapsed();

            LocationChanged += (_, __) => _viewModel.OnAppCollapsed();
            SizeChanged += (_, __) => _viewModel.OnAppCollapsed();

            DataContext = _viewModel;

            this.StateChanged += FullWindow_StateChanged;

            PreviewKeyDown += FullWindow_PreviewKeyDown;

            Instance = this;
            Closing += (s, e) =>
            {
                Instance = null;
                _viewModel.Close();
            };

            SourceInitialized += (_, __) =>
            {
                this.Cloak();
                this.SetWindowBlur(true, true);
            };

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (s, e) => Dispatcher.SafeInvoke(() => _viewModel.OnAppCollapsed());
        }

        private void FullWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _viewModel.IsShowingModalDialog)
            {
                _viewModel.OnAppCollapsed();
            }
            else
            {
                KeyboardNavigationService.OnKeyDown(this, ref e);
            }
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

        private void DeviceAndAppsControl_AppExpanded(object sender, AppVolumeControlExpandedEventArgs e)
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
