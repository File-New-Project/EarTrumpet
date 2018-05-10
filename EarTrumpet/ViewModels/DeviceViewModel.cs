using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.DataModel.Com;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace EarTrumpet.ViewModels
{
    public class DeviceViewModel : BindableBase
    {
        public AudioSessionViewModel Device { get; private set; }
        public ObservableCollection<AppItemViewModel> Apps { get; private set; }

        public string DeviceIconText { get; private set; }

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
            string icon = "";
            if (_device.IsMuted)
            {
                icon = "\xE74F";
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
                icon = "\xE74F";
            }

            DeviceIconText = icon;
            RaisePropertyChanged(nameof(DeviceIconText));
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    var newSession = new AppItemViewModel((IAudioDeviceSession)e.NewItems[0]);
                    Apps.AddSorted(newSession, AppItemViewModelComparer.Instance);
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

        internal void TakeExternalSession(AudioSessionViewModel vm)
        {
            // Collect all pids for this app.

            var pids = new HashSet<int>();

            if (vm.Children == null)
            {
                pids.Add(vm.Session.ProcessId);
            }
            else
            {
                foreach (var child in vm.Children)
                {
                    pids.Add(child.Session.ProcessId);
                }
            }

            foreach (var pid in pids)
            {
                _device.TakeSessionFromOtherDevice(pid);
            }
        }
    }
}
