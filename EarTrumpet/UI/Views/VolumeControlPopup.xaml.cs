﻿using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace EarTrumpet.UI.Views
{
    public partial class VolumeControlPopup : Popup
    {
        private bool _useDarkTheme = false;
        private MainViewModel _viewModel;

        public VolumeControlPopup()
        {
            InitializeComponent();

            AllowsTransparency = true;
            StaysOpen = false;

            Opened += VolumeControlPopup_Opened;
        }

        private void VolumeControlPopup_Opened(object sender, EventArgs e)
        {
            AccentPolicyLibrary.SetWindowBlur(this, isEnabled: true, enableBorders: false);
            AppItems.Focus();
        }

        private void ShowWithAnimation()
        {
            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 0.25,
                To = 1,
            };

            Opacity = (double)fadeAnimation.From;
            IsOpen = true;

            Child.BeginAnimation(OpacityProperty, fadeAnimation);
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
            var selectedApp = (IAppItemViewModel)((FrameworkElement)sender).DataContext;
            var persistedDeviceId = selectedApp.PersistedOutputDevice;

            var moveMenu = new ContextMenu();
            if (_useDarkTheme)
            {
                moveMenu.Style = (Style)Application.Current.FindResource("ContextMenuDarkOnly");
            }

            var menuItemStyle = (Style)Application.Current.FindResource("MenuItemDarkOnly");
            foreach (var dev in _viewModel.AllDevices)
            {
                var newItem = new MenuItem { Header = dev.DisplayName };
                if(_useDarkTheme)
                {
                    newItem.Style = menuItemStyle;
                }
                newItem.Click += (_, __) =>
                {
                    _viewModel.MoveAppToDevice(selectedApp, dev);

                    HideWithAnimation();
                };

                newItem.IsCheckable = true;
                newItem.IsChecked = (dev.Id == persistedDeviceId);

                moveMenu.Items.Add(newItem);
            }

            var defaultItem = new MenuItem { Header = EarTrumpet.Properties.Resources.DefaultDeviceText };
            if (_useDarkTheme)
            {
                defaultItem.Style = menuItemStyle;
            }
            defaultItem.IsCheckable = true;
            defaultItem.IsChecked = (string.IsNullOrWhiteSpace(persistedDeviceId));
            defaultItem.Click += (_, __) =>
            {
                _viewModel.MoveAppToDevice(selectedApp, null);
                HideWithAnimation();
            };
            moveMenu.Items.Insert(0, defaultItem);

            var newSeparator = new Separator();
            if (_useDarkTheme)
            {
                newSeparator.Style = (Style)Application.Current.FindResource("MenuItemSeparatorDarkOnly");
            }
            moveMenu.Items.Insert(1, newSeparator);

            moveMenu.Opened += (_, __) =>
            {
                ((Popup)moveMenu.Parent).PopupAnimation = PopupAnimation.None;
            };

            moveMenu.PlacementTarget = (UIElement)sender;
            moveMenu.Placement = PlacementMode.Bottom;
            moveMenu.IsOpen = true;
        }

        public void PositionAndShow(MainViewModel viewModel, Window relativeTo, AppExpandedEventArgs e)
        {
            _viewModel = viewModel;

            if (relativeTo == null)
            {
                throw new ArgumentException("relativeTo");
            }

            if (e.ViewModel == null)
            {
                throw new ArgumentException("ViewModel");
            }

            if (e.Container == null)
            {
                throw new ArgumentException("Container");
            }

            var taskbarState = WindowsTaskbar.Current;
            if (taskbarState.ContainingScreen == null)
            {
                throw new ArgumentException("taskbarState.ContainingScreen");
            }

            var HEADER_SIZE = (double)App.Current.Resources["DeviceTitleCellHeight"];
            var ITEM_SIZE = (double)App.Current.Resources["AppItemCellHeight"];
            var PopupBorderSize = (Thickness)App.Current.Resources["PopupBorderThickness"];
            var volumeListMargin = (Thickness)App.Current.Resources["VolumeAppListMargin"];

            DataContext = e.ViewModel;

            var contextTheme = (string)relativeTo.TryFindResource("ContextMenuTheme");
            _useDarkTheme = contextTheme != null && contextTheme.Equals("DarkOnly");

            Point offsetFromWindow = e.Container.TranslatePoint(new Point(0, 0), relativeTo);
            // Adjust for the title bar, top border and top margin on the app list.
            offsetFromWindow.Y -= (HEADER_SIZE + volumeListMargin.Bottom + PopupBorderSize.Top);

            var popupHeight = (HEADER_SIZE + (e.ViewModel.ChildApps.Count * ITEM_SIZE) + volumeListMargin.Bottom + volumeListMargin.Top);
            var popupOriginYScreenCoordinates = (relativeTo.PointToScreen(new Point(0, 0)).Y / this.DpiHeightFactor()) + offsetFromWindow.Y;

            var scaledWorkArea = new Rect(taskbarState.ContainingScreen.WorkingArea.Left / this.DpiWidthFactor(),
                taskbarState.ContainingScreen.WorkingArea.Top / this.DpiHeightFactor(),
                taskbarState.ContainingScreen.WorkingArea.Width / this.DpiWidthFactor(),
                taskbarState.ContainingScreen.WorkingArea.Height / this.DpiHeightFactor());

            // If we flow off the bottom
            if (popupOriginYScreenCoordinates + popupHeight > scaledWorkArea.Bottom)
            {
                popupOriginYScreenCoordinates = scaledWorkArea.Bottom - popupHeight;

                // If we also flow off the top
                if (popupOriginYScreenCoordinates < scaledWorkArea.Top)
                {
                    popupOriginYScreenCoordinates = scaledWorkArea.Top;
                    popupHeight = scaledWorkArea.Bottom - scaledWorkArea.Top;
                }
            }

            Placement = PlacementMode.Absolute;
            HorizontalOffset = (relativeTo.PointToScreen(new Point(0, 0)).X / this.DpiWidthFactor()) + offsetFromWindow.X;
            VerticalOffset = popupOriginYScreenCoordinates;

            Width = ((FrameworkElement)e.Container).ActualWidth;
            Height = popupHeight;

            ShowWithAnimation();
        }
    }
}
