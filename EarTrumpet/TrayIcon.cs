﻿using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet
{
    class TrayIcon
    {
        public event Action Invoked = delegate { };

        internal readonly System.Windows.Forms.NotifyIcon _trayIcon;
        private readonly System.Windows.Forms.ContextMenu _contextMenu;
        private readonly TrayViewModel _trayViewModel;
        private IAudioDeviceManager _deviceService;

        public TrayIcon(IAudioDeviceManager deviceService, TrayViewModel trayViewModel)
        {
            _deviceService = deviceService;
            _deviceService.VirtualDefaultDevice.PropertyChanged += VirtualDefaultDevice_PropertyChanged;
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _contextMenu = new System.Windows.Forms.ContextMenu();
            _trayIcon.ContextMenu = _contextMenu;
            _trayViewModel = trayViewModel;
            _trayViewModel.PropertyChanged += TrayViewModel_PropertyChanged;

            HotkeyService.KeyPressed += Hotkey_KeyPressed;

            // (Devices added here later in ContextMenu_Popup)

            _contextMenu.MenuItems.Add("-");

            AddItem(Properties.Resources.FullWindowTitleText, EtVolumeMixer_Click);
            AddItem(Properties.Resources.LegacyVolumeMixerText, OpenlegacyVolumeMixer_Click);

            _contextMenu.MenuItems.Add("-");

            AddItem(Properties.Resources.PlaybackDevicesText, PlaybackDevices_Click);
            AddItem(Properties.Resources.RecordingDevicesText, RecordingDevices_Click);
            AddItem(Properties.Resources.SoundsControlPanelText, SoundsControlPanel_Click);

            _contextMenu.MenuItems.Add("-");

            AddItem(Properties.Resources.SettingsWindowText, SettingsItem_Click);
            AddItem(Properties.Resources.ContextMenuSendFeedback, Feedback_Click);
            AddItem(Properties.Resources.ContextMenuExitTitle, Exit_Click);

            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _contextMenu.Popup += ContextMenu_Popup;

            _trayIcon.Icon = _trayViewModel.TrayIcon;

            UpdateToolTip();

            _trayIcon.Visible = true;
        }

        private void AddItem(string text, EventHandler clickHandler)
        {
            var item = _contextMenu.MenuItems.Add(text);
            item.Click += clickHandler;
        }

        private void SettingsItem_Click(object sender, EventArgs e)
        {
            _trayViewModel.OpenSettingsCommand.Execute();
        }

        private void EtVolumeMixer_Click(object sender, EventArgs e)
        {
            _trayViewModel.OpenEarTrumpetVolumeMixerCommand.Execute();
        }

        private void Hotkey_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            Invoked.Invoke();
        }

        private void PlaybackDevices_Click(object sender, EventArgs e)
        {
            _trayViewModel.OpenPlaybackDevicesCommand.Execute();
        }

        private void RecordingDevices_Click(object sender, EventArgs e)
        {
            _trayViewModel.OpenRecordingDevicesCommand.Execute();
        }

        private void SoundsControlPanel_Click(object sender, EventArgs e)
        {
            _trayViewModel.OpenSoundsControlPanelCommand.Execute();
        }

        private void OpenlegacyVolumeMixer_Click(object sender, EventArgs e)
        {
            _trayViewModel.OpenLegacyVolumeMixerCommand.Execute();
        }

        private void VirtualDefaultDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateToolTip();
        }

        void UpdateToolTip()
        {
            var device = _deviceService.VirtualDefaultDevice;

            if (device.IsDevicePresent)
            {
                var otherText = "EarTrumpet: 100% - ";
                var dev = _deviceService.VirtualDefaultDevice.DisplayName;
                // API Limitation: "less than 64 chars" for the tooltip.
                dev = dev.Substring(0, Math.Min(63 - otherText.Length, dev.Length));
                _trayIcon.Text = $"EarTrumpet: {_deviceService.VirtualDefaultDevice.Volume.ToVolumeInt()}% - {dev}";
            }
            else
            {
                _trayIcon.Text = Properties.Resources.NoDeviceTrayText;
            }
        }

        private void TrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_trayViewModel.TrayIcon))
            {
                _trayIcon.Icon = _trayViewModel.TrayIcon;
            }
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            SetupDeviceMenuItems();
        }

        void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Invoked.Invoke();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                    Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    LegacyVolumeFlyoutService.ShowMenu();
                }
                else
                {
                    // TODO: When we de-couple ContextMenu from NotifyIcon, show it here.
                    // _contextMenu.Show(null, System.Windows.Forms.Cursor.Position);
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                _deviceService.VirtualDefaultDevice.IsMuted = !_deviceService.VirtualDefaultDevice.IsMuted;
            }
        }

        void Feedback_Click(object sender, EventArgs e)
        {
            _trayViewModel.StartAppServiceAndFeedbackHubCommand.Execute();
        }

        public void Exit_Click(object sender, EventArgs e)
        {
            try
            {
                foreach(var proc in Process.GetProcessesByName("EarTrumpet.UWP"))
                {
                    proc.Kill();
                }
            }
            catch
            {
                // We're shutting down, ignore all
            }
            _trayViewModel.CloseAppService();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Current.Shutdown();
        }

        void Device_Click(object sender, EventArgs e)
        {
            var device = (IAudioDevice)((System.Windows.Forms.MenuItem)sender).Tag;
            _trayViewModel.ChangeDeviceCommand.Execute(device);
        }

        internal void SetupDeviceMenuItems()
        {
            for (int i = _trayIcon.ContextMenu.MenuItems.Count - 1; i >= 0; i--)
            {
                var item = _trayIcon.ContextMenu.MenuItems[i];
                if (item.Tag != null)
                {
                    _trayIcon.ContextMenu.MenuItems.Remove(item);
                }
            }

            var audioDevices = _deviceService.Devices.OrderBy(x => x.DisplayName);
            if (audioDevices.Count() == 0)
            {
                var newItem = new System.Windows.Forms.MenuItem(EarTrumpet.Properties.Resources.ContextMenuNoDevices);
                newItem.Enabled = false;
                _trayIcon.ContextMenu.MenuItems.Add(0, newItem);
            }

            int iPos = 0;
            foreach(var device in audioDevices)
            {
                var newItem = new System.Windows.Forms.MenuItem(device.DisplayName, Device_Click);
                newItem.Tag = device;
                newItem.Checked = device == _deviceService.DefaultPlaybackDevice;
                _trayIcon.ContextMenu.MenuItems.Add(iPos++, newItem);
            }
        }
    }
}
