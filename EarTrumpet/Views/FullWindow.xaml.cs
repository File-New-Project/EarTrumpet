using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace EarTrumpet.Views
{
    public partial class FullWindow : Window
    {
        private static FullWindow Instance;

        private FullWindowViewModel _viewModel;
        private VolumeControlPopup _popup;

        public FullWindow(MainViewModel viewModel)
        {
            Instance = this;

            _viewModel = new FullWindowViewModel(viewModel);
            _viewModel.AppExpanded += OnAppExpanded;
            _viewModel.AppCollapsed += (_, __) => OnAppCollapsed();

            InitializeComponent();

            Title = Properties.Resources.FullWindowTitleText;
            _popup = AppPopup;
            _popup.Closed += (_, __) => _viewModel.OnAppCollapsed();

            LocationChanged += (_, __) => _viewModel.OnAppCollapsed();
            SizeChanged += (_, __) => _viewModel.OnAppCollapsed();
            DataContext = _viewModel;

            this.StateChanged += FullWindow_StateChanged;

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
        
        private void OnAppExpanded(object sender, AppExpandedEventArgs e)
        {
            _popup.PositionAndShow(this, e);

            LightDismissBorder.Visibility = Visibility.Visible;

            if (!SystemParameters.MenuAnimation)
            {
                return;
            }

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.HoldEnd,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 0,
                To = 1,
            };

            Storyboard.SetTarget(fadeAnimation, LightDismissBorder);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.HoldEnd;
            storyboard.Children.Add(fadeAnimation);
            storyboard.Begin(LightDismissBorder);
        }

        private void OnAppCollapsed()
        {
            _popup.HideWithAnimation();

            if (!SystemParameters.MenuAnimation)
            {
                LightDismissBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 1,
                To = 0,
            };

            Storyboard.SetTarget(fadeAnimation, LightDismissBorder);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(fadeAnimation);
            storyboard.Completed += (s, e) =>
            {
                LightDismissBorder.Visibility = Visibility.Collapsed;
                LightDismissBorder.Opacity = 1;
            };
            storyboard.Begin(LightDismissBorder);
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
            WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
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
