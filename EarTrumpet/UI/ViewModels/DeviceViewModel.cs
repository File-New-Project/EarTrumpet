using Bugsnag.Payload;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet.UI.ViewModels
{
    public class DeviceViewModel : AudioSessionViewModel, IDeviceViewModel
    {
        public class DisplayNameComparer : IComparer<DeviceViewModel>
        {
            public int Compare(DeviceViewModel one, DeviceViewModel two)
            {
                return string.Compare(one.DisplayName, two.DisplayName, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public static readonly DisplayNameComparer CompareByDisplayName = new DisplayNameComparer();

        public enum DeviceIconKind
        {
            Mute,
            Bar0,
            Bar1,
            Bar2,
            Bar3,
            Microphone,
        }

        public string DisplayName => _device.DisplayName;
        public string AccessibleName => IsMuted ? Properties.Resources.AppOrDeviceMutedFormatAccessibleText.Replace("{Name}", DisplayName) :
            Properties.Resources.AppOrDeviceFormatAccessibleText.Replace("{Name}", DisplayName).Replace("{Volume}", Volume.ToString());
        public string DeviceDescription => ((IAudioDeviceWindowsAudio)_device).DeviceDescription;
        public string EnumeratorName => ((IAudioDeviceWindowsAudio)_device).EnumeratorName;
        public string InterfaceName => ((IAudioDeviceWindowsAudio)_device).InterfaceName;
        public ObservableCollection<IAppItemViewModel> Apps { get; }

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

        public DeviceIconKind IconKind
        {
            get => _iconKind;
            set
            {
                if (_iconKind != value)
                {
                    _iconKind = value;
                    RaisePropertyChanged(nameof(IconKind));
                }
            }
        }

        protected readonly IAudioDevice _device;
        private readonly AppSettings _settings;
        protected readonly IAudioDeviceManager _deviceManager;
        protected readonly WeakReference<DeviceCollectionViewModel> _parent;
        private bool _isDisplayNameVisible;
        private DeviceIconKind _iconKind;

        public DeviceViewModel(DeviceCollectionViewModel parent, IAudioDeviceManager deviceManager, IAudioDevice device, AppSettings settings) : base(device)
        {
            _deviceManager = deviceManager;
            _device = device;
            _settings = settings;
            _parent = new WeakReference<DeviceCollectionViewModel>(parent);
            Apps = new ObservableCollection<IAppItemViewModel>();

            _device.PropertyChanged += OnDevicePropertyChanged;
            _device.Groups.CollectionChanged += OnCollectionChanged;
            _settings.HiddenAppsChanged += OnHiddenAppsChanged;

            foreach (var session in _device.Groups)
            {
                var appItemViewModel = new AppItemViewModel(this, session);
                appItemViewModel.IsHidden = IsAppConfiguredAsHidden(appItemViewModel);
                appItemViewModel.PropertyChanged += OnAppItemPropertyChanged;
                Apps.AddSorted(appItemViewModel, AppItemViewModel.CompareByExeName);
            }

            UpdateMasterVolumeIcon();
        }

        private void OnHiddenAppsChanged()
        {
            foreach(var app in Apps)
            {
                app.IsHidden = IsAppConfiguredAsHidden(app);
            }
        }

        private bool IsAppConfiguredAsHidden(IAppItemViewModel app)
        {
            return _settings.HiddenApps.Any(hiddenApp => hiddenApp.AppId == app.AppId);
        }

        ~DeviceViewModel()
        {
            _device.PropertyChanged -= OnDevicePropertyChanged;
            _device.Groups.CollectionChanged -= OnCollectionChanged;
        }

        private void OnAppItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is AppItemViewModel appItem)
            {
                if (e.PropertyName == nameof(appItem.IsHidden))
                {
                    RaisePropertyChanged(nameof(Apps));
                }
            }
        }

        private void OnDevicePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_device.IsMuted) ||
                e.PropertyName == nameof(_device.Volume))
            {
                UpdateMasterVolumeIcon();
                RaisePropertyChanged(nameof(AccessibleName));
            }
            else if (e.PropertyName == nameof(_device.DisplayName))
            {
                RaisePropertyChanged(nameof(DisplayName));
                RaisePropertyChanged(nameof(AccessibleName));
            }
        }

        public override void UpdatePeakValueForeground()
        {
            base.UpdatePeakValueForeground();

            foreach (var app in Apps)
            {
                app.UpdatePeakValueForeground();
            }
        }

        private void UpdateMasterVolumeIcon()
        {
            if (_device.Parent.Kind == AudioDeviceKind.Recording.ToString())
            {
                IconKind = DeviceIconKind.Microphone;
            }
            else
            {
                var isOnWindows11 = Environment.OSVersion.IsAtLeast(OSVersions.Windows11);
                if (_device.IsMuted)
                {
                    IconKind = DeviceIconKind.Mute;
                }
                else if (isOnWindows11 && _device.Volume > 0.66f)
                {
                    IconKind = DeviceIconKind.Bar3;
                }
                else if (!isOnWindows11 && _device.Volume >= 0.66f)
                {
                    IconKind = DeviceIconKind.Bar3;
                }
                else if (isOnWindows11 && _device.Volume > 0.33f)
                {
                    IconKind = DeviceIconKind.Bar2;
                }
                else if (!isOnWindows11 && _device.Volume >= 0.33f)
                {
                    IconKind = DeviceIconKind.Bar2;
                }
                else if (_device.Volume > 0.00f)
                {
                    IconKind = DeviceIconKind.Bar1;
                }
                else
                {
                    IconKind = DeviceIconKind.Bar0;
                }
            }
        }

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            newSession.PropertyChanged += OnAppItemPropertyChanged;
            newSession.IsHidden = IsAppConfiguredAsHidden(newSession);

            foreach (var app in Apps)
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
            app.Expired += OnAppExpired;

            foreach (var childApp in app.ChildApps)
            {
                ((IAudioDeviceManagerWindowsAudio)_deviceManager).UnhideSessionsForProcessId(_device.Id, childApp.ProcessId);
            }

            bool hasExistingAppGroup = false;
            foreach (var a in Apps)
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

        private void OnAppExpired(object sender, EventArgs e)
        {
            var app = (TemporaryAppItemViewModel)sender;
            if (Apps.Contains(app))
            {
                app.Expired -= OnAppExpired;
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

        public void MakeDefaultDevice() => _deviceManager.Default = _device;
        public void IncrementVolume(int delta) => Volume += delta;
        public override string ToString() => AccessibleName;
    }
}
