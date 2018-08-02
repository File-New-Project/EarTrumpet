using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    public class DeviceViewModel : AudioSessionViewModel
    {
        public string DisplayName => _device.DisplayName;
        public ObservableCollection<IAppItemViewModel> Apps { get; private set; }
        public string DeviceIconText { get; private set; }
        public string DeviceIconTextBackground { get; private set; }

        public bool IsDisplayNameVisible
        {
            get => _isDisplayNameVisible;
            set
            {
                if (_isDisplayNameVisible != value)
                {
                    _isDisplayNameVisible = value;
                    RaisePropertyChanged(nameof(IsDisplayNameVisible));
                }
            }
        }

        private readonly string s_Sound3BarsIcon = "\xE995";
        private readonly string s_Sound2BarsIcon = "\xE994";
        private readonly string s_Sound1BarIcon = "\xE993";
        private readonly string s_SoundMuteIcon = "\xE74F";

        private IAudioDevice _device;
        private IAudioDeviceManager _deviceManager;
        private bool _isDisplayNameVisible;
        private WeakReference<DeviceCollectionViewModel> _parent;

        internal DeviceViewModel(DeviceCollectionViewModel parent, IAudioDeviceManager deviceManager, IAudioDevice device) : base(device)
        {
            _deviceManager = deviceManager;
            _device = device;
            _parent = new WeakReference<DeviceCollectionViewModel>(parent);

            Apps = new ObservableCollection<IAppItemViewModel>();

            _device.PropertyChanged += Device_PropertyChanged;
            _device.Groups.CollectionChanged += Sessions_CollectionChanged;

            foreach (var session in _device.Groups)
            {
                Apps.AddSorted(new AppItemViewModel(this, session), AppItemViewModel.CompareByExeName);
            }

            UpdateMasterVolumeIcon();
        }

        ~DeviceViewModel()
        {
            _device.PropertyChanged -= Device_PropertyChanged;
            _device.Groups.CollectionChanged -= Sessions_CollectionChanged;
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_device.IsMuted) ||
                e.PropertyName == nameof(_device.Volume))
            {
                UpdateMasterVolumeIcon();
            }
            else if (e.PropertyName == nameof(_device.DisplayName))
            {
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        public override void UpdatePeakValueForeground()
        {
            base.UpdatePeakValueForeground();

            foreach (var app in Apps) app.UpdatePeakValueForeground();
        }

        public void UpdatePeakValueBackground()
        {
            // We're in the background so we need to use a snapshot.
            foreach (var app in Apps.ToArray()) app.UpdatePeakValueBackground();

            _device.UpdatePeakValueBackground();
        }

        private void UpdateMasterVolumeIcon()
        {
            if (_device.Parent.DeviceKind == AudioDeviceKind.Recording)
            {
                DeviceIconText = "\xEC72";
                DeviceIconTextBackground = "";
            }
            else
            {
                string icon;
                if (_device.IsMuted)
                {
                    icon = s_SoundMuteIcon;
                }
                else if (_device.Volume >= 0.65f)
                {
                    icon = s_Sound3BarsIcon;
                }
                else if (_device.Volume >= 0.33f)
                {
                    icon = s_Sound2BarsIcon;
                }
                else if (_device.Volume > 0f)
                {
                    icon = s_Sound1BarIcon;
                }
                else
                {
                    icon = s_SoundMuteIcon;
                }

                DeviceIconText = icon;
                DeviceIconTextBackground = (icon == s_SoundMuteIcon) ? s_SoundMuteIcon : s_Sound3BarsIcon;
            }
            RaisePropertyChanged(nameof(DeviceIconText));
            RaisePropertyChanged(nameof(DeviceIconTextBackground));
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    AddSession((IAudioDeviceSession)e.NewItems[0]);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    var existing = Apps.FirstOrDefault(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id);
                    if (existing != null)
                    {
                        Apps.Remove(existing);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void AddSession(IAudioDeviceSession session)
        {
            var newSession = new AppItemViewModel(this, session);

            foreach(var app in Apps)
            {
                if (app.DoesGroupWith(newSession))
                {
                    // Remove the fake app entry after copying any changes the user did.
                    newSession.Volume = app.Volume;
                    newSession.IsMuted = app.IsMuted;
                    Apps.Remove(app);
                    break;
                }
            }

            Apps.AddSorted(newSession, AppItemViewModel.CompareByExeName);
        }

        public void AppMovingToThisDevice(TemporaryAppItemViewModel app)
        {
            app.Expired += App_Expired;

            foreach (var childApp in app.ChildApps)
            {
                _device.UnhideSessionsForProcessId(childApp.ProcessId);
            }

            bool hasExistingAppGroup = false;
            foreach(var a in Apps)
            {
                if (a.DoesGroupWith(app))
                {
                    hasExistingAppGroup = true;
                    break;
                }
            }

            if (!hasExistingAppGroup)
            {
                Apps.AddSorted(app, AppItemViewModel.CompareByExeName);
            }
        }

        private void App_Expired(object sender, EventArgs e)
        {
            var app = (TemporaryAppItemViewModel)sender;
            if (Apps.Contains(app))
            {
                app.Expired -= App_Expired;
                Apps.Remove(app);
            }
        }

        internal void AppLeavingFromThisDevice(IAppItemViewModel app)
        {
            if (app is TemporaryAppItemViewModel)
            {
                Apps.Remove(app);
            }
        }

        public void MakeDefaultDevice()
        {
            _deviceManager.SetDefaultDevice(_device);
        }

        public override string ToString() => string.Format(IsMuted ? Properties.Resources.AppOrDeviceMutedFormatAccessibleText : Properties.Resources.AppOrDeviceFormatAccessibleText, DisplayName, Volume);

        public void OpenPopup(object app, FrameworkElement container)
        {
            if (_parent.TryGetTarget(out var parent))
            {
                parent.OpenPopup(app, container);
            }
        }
    }
}
