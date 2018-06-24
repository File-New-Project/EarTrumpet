using EarTrumpet.Interop;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.Properties;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using EarTrumpet.DataModel;

namespace EarTrumpet.UI.Controls
{
    class TrayIcon
    {
        private readonly System.Windows.Forms.NotifyIcon _trayIcon;
        private readonly TrayViewModel _trayViewModel;

        public TrayIcon(TrayViewModel trayViewModel)
        {
            _trayViewModel = trayViewModel;
            _trayViewModel.PropertyChanged += TrayViewModel_PropertyChanged;

            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.Icon = _trayViewModel.TrayIcon;
            _trayIcon.Text = _trayViewModel.ToolTip;

            App.Current.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        private ContextMenu BuildContextMenu()
        {
            var cm = new ContextMenu { Style = (Style)Application.Current.FindResource("ContextMenuDarkOnly") };

            cm.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            cm.Opened += ContextMenu_Opened;
            cm.Closed += ContextMenu_Closed;
            cm.StaysOpen = true; // To be removed on open.

            var menuItemStyle = (Style)Application.Current.FindResource("MenuItemDarkOnly");
            var AddItem = new Action<string, ICommand>((displayName, action) =>
            {
                cm.Items.Add(new MenuItem
                {
                    Header = displayName,
                    Command = action,
                    Style = menuItemStyle
                });
            });

            // Add devices
            var audioDevices = _trayViewModel.AllDevices.OrderBy(x => x.DisplayName);
            if (!audioDevices.Any())
            {
                cm.Items.Add(new MenuItem
                {
                    Header = Resources.ContextMenuNoDevices,
                    IsEnabled = false,
                    Style = menuItemStyle
                });
            }
            else
            {
                foreach (var device in audioDevices)
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = device.DisplayName,
                        IsChecked = device.Id == _trayViewModel.DefaultDeviceId,
                        Command = new RelayCommand(() => _trayViewModel.ChangeDeviceCommand.Execute(device)),
                        Style = menuItemStyle
                    });
                }
            }

            // Static items
            var separatorStyle = (Style)Application.Current.FindResource("MenuItemSeparatorDarkOnly");

            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem(Resources.FullWindowTitleText, _trayViewModel.OpenEarTrumpetVolumeMixerCommand);
            AddItem(Resources.LegacyVolumeMixerText, _trayViewModel.OpenLegacyVolumeMixerCommand);
            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem(Resources.PlaybackDevicesText, _trayViewModel.OpenPlaybackDevicesCommand);
            AddItem(Resources.RecordingDevicesText, _trayViewModel.OpenRecordingDevicesCommand);
            AddItem(Resources.SoundsControlPanelText, _trayViewModel.OpenSoundsControlPanelCommand);
            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem(Resources.ResetChannelsText, _trayViewModel.ResetChannelsCommand);
            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem(Resources.SettingsWindowText, _trayViewModel.OpenSettingsCommand);
            AddItem(Resources.ContextMenuSendFeedback, _trayViewModel.OpenFeedbackHubCommand);
            AddItem(Resources.ContextMenuExitTitle, _trayViewModel.ExitCommand);

            return cm;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("TrayIcon ContextMenu_Closed");
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("TrayIcon ContextMenu_Opened");
            var cm = (ContextMenu)sender;
            User32.SetForegroundWindow(((HwndSource)HwndSource.FromVisual(cm)).Handle);
            cm.Focus();
            cm.StaysOpen = false;
            ((Popup)cm.Parent).PopupAnimation = PopupAnimation.None;
        }

        public void Show()
        {
            Trace.WriteLine("TrayIcon Show");
            _trayIcon.Visible = true;
            Trace.WriteLine("TrayIcon Shown");
        }

        private void TrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_trayViewModel.TrayIcon))
            {
                _trayIcon.Icon = _trayViewModel.TrayIcon;
            }
            else if (e.PropertyName == nameof(_trayViewModel.ToolTip))
            {
                _trayIcon.Text = _trayViewModel.ToolTip;
            }
        }

        void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Trace.WriteLine($"TrayIcon TrayIcon_MouseClick {e.Button}");

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _trayViewModel.OpenFlyoutCommand.Execute();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var cm = BuildContextMenu();
                cm.Placement = PlacementMode.Mouse;
                cm.IsOpen = true;
                Trace.WriteLine("TrayIcon TrayIcon_MouseClick Right (ContextMenu now open)");
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                _trayViewModel.ToggleMute();
            }
        }
    }
}
