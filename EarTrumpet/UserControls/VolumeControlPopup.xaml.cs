using EarTrumpet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace EarTrumpet.UserControls
{
    public partial class VolumeControlPopup : Popup
    {
        public VolumeControlPopup()
        {
            InitializeComponent();
        }

        public void ShowWithAnimation()
        {
            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 0.25,
                To = 1,
            };

            Child.BeginAnimation(OpacityProperty, fadeAnimation);

            IsOpen = true;
        }

        public void HideWithAnimation()
        {
            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 1,
                To = 0.25,
            };

            fadeAnimation.Completed += (_, __) => IsOpen = false;

            Child.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            HideWithAnimation();
        }

        private void MoveToAnotherDevice_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = MainViewModel.Instance;
            var selectedApp = (AppItemViewModel)((FrameworkElement)sender).DataContext;
            var persistedDeviceId = selectedApp.PersistedOutputDevice;

            var moveMenu = new ContextMenu();

            foreach (var dev in viewModel.AllDevices)
            {
                var newItem = new MenuItem { Header = dev.Device.DisplayName };
                newItem.Click += (_, __) =>
                {
                    viewModel.MoveAppToDevice(selectedApp, dev);

                    HideWithAnimation();
                };

                newItem.IsCheckable = true;
                newItem.IsChecked = (dev.Device.Id == persistedDeviceId);

                moveMenu.Items.Add(newItem);
            }

            var defaultItem = new MenuItem { Header = EarTrumpet.Properties.Resources.DefaultDeviceText };
            defaultItem.Click += (_, __) =>
            {
                viewModel.MoveAppToDevice(selectedApp, null);
                HideWithAnimation();
            };
            moveMenu.Items.Insert(0, defaultItem);

            moveMenu.Items.Insert(1, new Separator());

            moveMenu.PlacementTarget = (UIElement)sender;
            moveMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            moveMenu.IsOpen = true;
        }
    }
}
