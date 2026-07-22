using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace EarTrumpet.UI.Views
{
    public partial class DeviceView : UserControl
    {
        private const string NvidiaBroadcastAppName = "NVIDIA Broadcast";
        private const string SteamAppName = "Steam";
        private const string SteamExeName = "steam.exe";
        private const string SteamWebHelperExeName = "steamwebhelper.exe";

        public static string DeviceListItemKey = "DeviceListItem";

        public DeviceViewModel Device { get { return (DeviceViewModel)GetValue(DeviceProperty); } set { SetValue(DeviceProperty, value); } }
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceViewModel), typeof(DeviceView), new PropertyMetadata(new PropertyChangedCallback(DeviceChanged)));

        public bool IsDisplayNameVisible { get { return (bool)GetValue(IsDisplayNameVisibleProperty); } set { SetValue(IsDisplayNameVisibleProperty, value); } }
        public static readonly DependencyProperty IsDisplayNameVisibleProperty =
            DependencyProperty.Register("IsDisplayNameVisible", typeof(bool), typeof(DeviceView), new PropertyMetadata(true));

        public bool IsAppListVisible { get { return (bool)GetValue(IsAppListVisibleProperty); } set { SetValue(IsAppListVisibleProperty, value); } }
        public static readonly DependencyProperty IsAppListVisibleProperty =
            DependencyProperty.Register("IsAppListVisible", typeof(bool), typeof(DeviceView), new PropertyMetadata(true));

        private FlyoutViewModel _flyoutViewModel;
        private ListCollectionView _filteredApps;

        public DeviceView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DeviceListItem.PreviewKeyDown += OnPreviewKeyDown;
            DeviceListItem.PreviewMouseRightButtonUp += (_, __) => OpenPopup();
        }

        public void FocusAndRemoveFocusVisual()
        {
            DeviceListItem.Focus();
            RemoveFocusVisual(DeviceListItem);
        }

        private void RemoveFocusVisual(UIElement element)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            var adorners = adornerLayer.GetAdorners(element);
            if (adorners != null)
            {
                foreach (var adorner in adorners)
                {
                    adornerLayer.Remove(adorner);
                }
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.M:
                case Key.OemPeriod:
                    Device.IsMuted = !Device.IsMuted;
                    e.Handled = true;
                    break;

                case Key.Right:
                case Key.OemPlus:
                    Device.Volume++;
                    e.Handled = true;
                    break;

                case Key.Left:
                case Key.OemMinus:
                    Device.Volume--;
                    e.Handled = true;
                    break;

                case Key.Space:
                    OpenPopup();
                    e.Handled = true;
                    break;
            }
        }

        private static void DeviceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DeviceView)d;
            self.GridRoot.DataContext = self;

            if (self.IsLoaded)
            {
                self.UpdateAppListSource();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateAppListSource();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachFromFlyoutViewModel();
        }

        private void UpdateAppListSource()
        {
            DetachFromFlyoutViewModel();
            _filteredApps = null;

            if (Device == null)
            {
                AppList.ItemsSource = null;
                return;
            }

            // Only filter the compact tray flyout. Keep the full mixer unchanged.
            var flyoutWindow = Window.GetWindow(this) as FlyoutWindow;
            _flyoutViewModel = flyoutWindow?.DataContext as FlyoutViewModel;

            if (_flyoutViewModel != null)
            {
                _flyoutViewModel.PropertyChanged += OnFlyoutViewModelPropertyChanged;

                _filteredApps = new ListCollectionView(Device.Apps)
                {
                    Filter = ShouldShowAppInFlyout,
                };

                AppList.ItemsSource = _filteredApps;
            }
            else
            {
                AppList.ItemsSource = Device.Apps;
            }
        }

        private void DetachFromFlyoutViewModel()
        {
            if (_flyoutViewModel != null)
            {
                _flyoutViewModel.PropertyChanged -= OnFlyoutViewModelPropertyChanged;
                _flyoutViewModel = null;
            }
        }

        private void OnFlyoutViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FlyoutViewModel.DefaultDeviceId))
            {
                _filteredApps?.Refresh();
            }
        }

        private bool ShouldShowAppInFlyout(object item)
        {
            var app = item as IAppItemViewModel;
            if (app == null)
            {
                return true;
            }

            // The active/default output keeps every slider, including
            // System Sounds, NVIDIA Broadcast and Steam.
            if (Device.Id == _flyoutViewModel?.DefaultDeviceId)
            {
                return true;
            }

            // On every other device, hide the repeated Windows System Sounds slider.
            // EarTrumpet marks the System Sounds session as non-movable.
            if (!app.IsMovable)
            {
                return false;
            }

            // Hide repeated NVIDIA Broadcast and Steam client sliders on
            // non-default devices.
            if (ContainsNvidiaBroadcast(app.DisplayName) ||
                ContainsNvidiaBroadcast(app.ExeName) ||
                ContainsNvidiaBroadcast(app.AppId) ||
                IsSteamClient(app))
            {
                return false;
            }

            // Keep any genuine application session using this device.
            return true;
        }

        private static bool ContainsNvidiaBroadcast(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                value.IndexOf(NvidiaBroadcastAppName, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsSteamClient(IAppItemViewModel app)
        {
            return EqualsIgnoreCase(app.DisplayName, SteamAppName) ||
                EndsWithExecutable(app.ExeName, SteamExeName) ||
                EndsWithExecutable(app.ExeName, SteamWebHelperExeName) ||
                EqualsIgnoreCase(app.AppId, SteamAppName) ||
                EqualsIgnoreCase(app.AppId, SteamExeName) ||
                EqualsIgnoreCase(app.AppId, SteamWebHelperExeName);
        }

        private static bool EqualsIgnoreCase(string value, string expected)
        {
            return string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EndsWithExecutable(string value, string executableName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmedValue = value.Trim().Trim('"');

            return EqualsIgnoreCase(trimmedValue, executableName) ||
                trimmedValue.EndsWith("\\" + executableName, StringComparison.OrdinalIgnoreCase) ||
                trimmedValue.EndsWith("/" + executableName, StringComparison.OrdinalIgnoreCase);
        }

        private void TouchSlider_TouchUp(object sender, TouchEventArgs e)
        {
            SystemSoundsHelper.PlayBeepSound.Execute(null);
        }

        private void TouchSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SystemSoundsHelper.PlayBeepSound.Execute(null);
            }
        }

        private void OpenPopup()
        {
            var viewModel = Window.GetWindow(DeviceListItem).DataContext as IPopupHostViewModel;
            if (viewModel != null)
            {
                viewModel.OpenPopup(Device, DeviceListItem);
            }
        }
    }
}
