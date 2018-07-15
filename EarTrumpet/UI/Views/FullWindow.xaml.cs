using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.UI.Views
{
    public partial class FullWindow : Window
    {
        private static FullWindow Instance;

        private MainViewModel _mainViewModel;
        private FullWindowViewModel _viewModel;
        private bool _isClosing;

        public FullWindow(MainViewModel viewModel)
        {
            _mainViewModel = viewModel;
            Trace.WriteLine("FullWindow .ctor");
            Instance = this;

            InitializeComponent();

            _viewModel = new FullWindowViewModel(viewModel);
            _viewModel.AppExpanded += ViewModel_AppExpanded;
            _viewModel.AppCollapsed += ViewModel_AppCollapsed;

            AppPopup.Closed += AppPopup_Closed;
            SourceInitialized += FullWindow_SourceInitialized;
            LocationChanged += FullWindow_LocationChanged;
            SizeChanged += FullWindow_SizeChanged;
            PreviewKeyDown += FullWindow_PreviewKeyDown;
            Closing += FullWindow_Closing;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            DataContext = _viewModel;

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Trace.WriteLine("FullWindow SystemEvents_DisplaySettingsChanged");

            Dispatcher.BeginInvoke((Action)(() => _viewModel.CollapseApp()));
        }

        private void FullWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Trace.WriteLine("FullWindow FullWindow_Closing");

            Instance = null;
            _viewModel.Close();
        }

        private void FullWindow_SourceInitialized(object sender, EventArgs e)
        {
            Trace.WriteLine("FullWindow FullWindow_SourceInitialized");

            this.Cloak();
            AccentPolicyLibrary.SetWindowBlur(this, true, true);
        }

        private void FullWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _viewModel.CollapseApp();
        }

        private void FullWindow_LocationChanged(object sender, EventArgs e)
        {
            _viewModel.CollapseApp();
        }

        private void AppPopup_Closed(object sender, EventArgs e)
        {
            Trace.WriteLine("FullWindow AppPopup_Closed");

            _viewModel.CollapseApp();
        }

        private void ViewModel_AppCollapsed(object sender, object e)
        {
            Trace.WriteLine("FullWindow ViewModel_AppCollapsed");

            AppPopup.HideWithAnimation();
        }

        private void ViewModel_AppExpanded(object sender, AppExpandedEventArgs e)
        {
            Trace.WriteLine("FullWindow ViewModel_AppExpanded");

            AppPopup.PositionAndShow(_mainViewModel, this, e);
        }

        public static void ActivateSingleInstance(MainViewModel mainViewModel)
        {
            Trace.WriteLine("FullWindow ActivateSingleInstance");
            if (Instance == null)
            {
                var window = new FullWindow(mainViewModel);

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
                    _viewModel.CollapseApp();
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
            Trace.WriteLine("FullWindow CloseButton_Click");
            if (!_isClosing)
            {
                // Ensure we don't double-animate if the user is able to close us multiple ways before the window stops accepting input.
                _isClosing = true;
                WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
            }
        }

        private void DeviceAndAppsControl_AppExpanded(object sender, AppVolumeControlExpandedEventArgs e)
        {
            Trace.WriteLine("FullWindow DeviceAndAppsControl_AppExpanded");

            _viewModel.ExpandApp(e.ViewModel, e.Container);
        }

        private void LightDismissBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AppPopup.HideWithAnimation();
            e.Handled = true;
        }
    }
}
