using EarTrumpet.Services;
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
                To = 0.5,
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
            var selectedApp = (AppItemViewModel)((FrameworkElement)sender).DataContext;
            var persistedDevice = selectedApp.PersistedOutputDevice;

            var moveMenu = new ContextMenu();

            foreach (var dev in MainViewModel.Instance.PlaybackDevices)
            {
                var newItem = new MenuItem { Header = dev.DisplayName };
                newItem.Click += (_, __) =>
                {
                    AudioPolicyConfigService.SetDefaultEndPoint(dev.Id, selectedApp.Session.ProcessId);
                };

                newItem.IsCheckable = true;
                newItem.IsChecked = (dev.Id == persistedDevice.Id);

                moveMenu.Items.Add(newItem);
            }

            moveMenu.Items.Insert(1, new Separator());

            moveMenu.PlacementTarget = (UIElement)sender;
            moveMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            moveMenu.IsOpen = true;
        }
    }
}
