using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.Views
{
    public partial class FullWindow : Window
    {
        private static FullWindow Instance;

        private FullWindowViewModel _viewModel;

        public FullWindow(MainViewModel viewModel)
        {
            Instance = this;

            _viewModel = new FullWindowViewModel(viewModel);
            _viewModel.AppExpanded += (_, e) => AppPopup.PositionAndShow(this, e);
            _viewModel.AppCollapsed += (_, __) => AppPopup.HideWithAnimation();

            InitializeComponent();

            AppPopup.Closed += (_, __) => _viewModel.OnAppCollapsed();

            LocationChanged += (_, __) => _viewModel.OnAppCollapsed();
            SizeChanged += (_, __) => _viewModel.OnAppCollapsed();
            DataContext = _viewModel;

            PreviewKeyDown += FullWindow_PreviewKeyDown;

            this.FlowDirection = UserSystemPreferencesService.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

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

        public static void ActivateSingleInstance()
        {
            if (Instance == null)
            {
                var window = new FullWindow(MainViewModel.Instance);

                window.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(window, () => { });
            }
            else
            {
                Instance.RaiseWindow();
            }
        }

        private void FullWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_viewModel.IsShowingModalDialog)
                {
                    _viewModel.OnAppCollapsed();
                }
                else
                {
                    CloseButton_Click(null, null);
                }
            }
            else
            {
                KeyboardNavigator.OnKeyDown(this, ref e);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
        }

        private void DeviceAndAppsControl_AppExpanded(object sender, AppVolumeControlExpandedEventArgs e)
        {
            _viewModel.OnAppExpanded(e.ViewModel, e.Container);
        }

        private void LightDismissBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AppPopup.HideWithAnimation();
            e.Handled = true;
        }
    }
}
