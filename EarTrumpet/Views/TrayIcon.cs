using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using resx = EarTrumpet.Properties.Resources;

namespace EarTrumpet.Views
{
    class TrayIcon
    {
        private readonly System.Windows.Forms.NotifyIcon _trayIcon;
        private readonly TrayViewModel _trayViewModel;
        private readonly IVirtualDefaultAudioDevice _defaultDevice;

        public TrayIcon(IAudioDeviceManager deviceManager, TrayViewModel trayViewModel)
        {
            _defaultDevice = deviceManager.VirtualDefaultDevice;
            _defaultDevice.PropertyChanged += (_, __) => UpdateToolTip();

            _trayViewModel = trayViewModel;
            _trayViewModel.PropertyChanged += TrayViewModel_PropertyChanged;

            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.Icon = _trayViewModel.TrayIcon;
            UpdateToolTip();

            _trayIcon.Visible = true;

            App.Current.Exit += (_, __) =>
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            };
        }

        private ContextMenu BuildContextMenu()
        {
            var cm = new ContextMenu { Style = Application.Current.FindResource("ContextMenuDarkOnly") as Style };

            cm.FlowDirection = UserSystemPreferencesService.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            cm.Opened += (_, __) =>
            {
                User32.SetForegroundWindow(((HwndSource)HwndSource.FromVisual(cm)).Handle);
                cm.Focus();
                ((Popup)cm.Parent).PopupAnimation = PopupAnimation.None;
            };

            var menuItemStyle = Application.Current.FindResource("MenuItemDarkOnly") as Style;
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
                    Header = resx.ContextMenuNoDevices,
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
                        IsChecked = device.Id == _defaultDevice.Id,
                        Command = new RelayCommand(() => _trayViewModel.ChangeDeviceCommand.Execute(device)),
                        Style = menuItemStyle
                    });
                }
            }

            // Static items
            var separatorStyle = Application.Current.FindResource("MenuItemSeparatorDarkOnly") as Style;

            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem(resx.FullWindowTitleText, _trayViewModel.OpenEarTrumpetVolumeMixerCommand);
            AddItem(resx.LegacyVolumeMixerText, _trayViewModel.OpenLegacyVolumeMixerCommand);
            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem(resx.PlaybackDevicesText, _trayViewModel.OpenPlaybackDevicesCommand);
            AddItem(resx.RecordingDevicesText, _trayViewModel.OpenRecordingDevicesCommand);
            AddItem(resx.SoundsControlPanelText, _trayViewModel.OpenSoundsControlPanelCommand);
            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem(resx.SettingsWindowText, _trayViewModel.OpenSettingsCommand);
            AddItem(resx.ContextMenuSendFeedback, _trayViewModel.OpenFeedbackHubCommand);
            AddItem(resx.ContextMenuExitTitle, _trayViewModel.ExitCommand);

            return cm;
        }

        private void UpdateToolTip()
        {
            if (_defaultDevice.IsDevicePresent)
            {
                var otherText = "EarTrumpet: 100% - ";
                var dev = _defaultDevice.DisplayName;
                // API Limitation: "less than 64 chars" for the tooltip.
                dev = dev.Substring(0, Math.Min(63 - otherText.Length, dev.Length));
                _trayIcon.Text = $"EarTrumpet: {_defaultDevice.Volume.ToVolumeInt()}% - {dev}";
            }
            else
            {
                _trayIcon.Text = resx.NoDeviceTrayText;
            }
        }

        private void TrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_trayViewModel.TrayIcon))
            {
                _trayIcon.Icon = _trayViewModel.TrayIcon;
            }
        }

        void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _trayViewModel.OpenFlyoutCommand.Execute();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var cm = BuildContextMenu();
                cm.Placement = PlacementMode.Mouse;
                cm.IsOpen = true;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                _defaultDevice.IsMuted = !_defaultDevice.IsMuted;
            }
        }
    }
}
