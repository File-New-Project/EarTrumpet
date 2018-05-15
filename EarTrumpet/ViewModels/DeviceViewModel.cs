using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet.ViewModels
{
    public class DeviceViewModel : BindableBase
    {
        public AudioSessionViewModel Device { get; private set; }
        public ObservableCollection<AppItemViewModel> Apps { get; private set; }

        public string DeviceIconText { get; private set; }
        public string DeviceIconTextBackground { get; private set; }

        IAudioDevice _device;
        IAudioDeviceManager _deviceService;

        public DeviceViewModel(IAudioDeviceManager deviceService, IAudioDevice device)
        {
            _deviceService = deviceService;
            _device = device;

            Device = new AudioSessionViewModel(device);
            Apps = new ObservableCollection<AppItemViewModel>();

            _device.PropertyChanged += _device_PropertyChanged;
            _device.Sessions.CollectionChanged += Sessions_CollectionChanged;

            Apps.Clear();
            foreach (var session in _device.Sessions)
            {
                Apps.AddSorted(new AppItemViewModel(session), AppItemViewModelComparer.Instance);
            }

            UpdateDeviceText();
        }

        private void _device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_device.IsMuted) ||
                e.PropertyName == nameof(_device.Volume))
            {
                UpdateDeviceText();
            }
        }

        ~DeviceViewModel()
        {
            _device.Sessions.CollectionChanged -= Sessions_CollectionChanged;
        }

        public void TriggerPeakCheck()
        {
            Device.TriggerPeakCheck();

            foreach (var app in Apps) app.TriggerPeakCheck();
        }

        private void UpdateDeviceText()
        {
            DeviceIconTextBackground = "\xE995";

            string icon = "";
            if (_device.IsMuted)
            {
                icon = "\xE74F";
                DeviceIconTextBackground = icon;
            }
            else if (_device.Volume >= 0.65f)
            {
                icon = "\xE995";
            }
            else if (_device.Volume >= 0.33f)
            {
                icon = "\xE994";
            }
            else if (_device.Volume > 0f)
            {
                icon = "\xE993";
            }
            else
            {
                icon = "\xE74F"; // Mute
                DeviceIconTextBackground = icon;
            }

            DeviceIconText = icon;
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
                    Apps.Remove(Apps.First(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }

        private void AddSession(IAudioDeviceSession session)
        {
            var newSession = new AppItemViewModel(session);

            foreach(var a in Apps)
            {
                if (a.DoesGroupWith(newSession))
                {
                    Apps.Remove(a);
                    break;
                }
            }

            Apps.AddSorted(newSession, AppItemViewModelComparer.Instance);
        }

        public void OnAppMovedToDevice(AppItemViewModel app)
        {
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
                Apps.AddSorted(app, AppItemViewModelComparer.Instance);
            }
        }

        public void OnAppMovedFromDevice(AppItemViewModel app)
        {
            Apps.Remove(app);
        }
    }
}
