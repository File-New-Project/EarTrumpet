using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EarTrumpet.ViewModels
{
    public class FullWindowViewModel : BindableBase
    {
        public ObservableCollection<DeviceViewModel> Devices { get; private set; }

        bool _showEmptyDevices;
        public bool ShowEmptyDevices
        {
            get => _showEmptyDevices;
            set
            {
                if (_showEmptyDevices != value)
                {
                    _showEmptyDevices = value;
                    RaisePropertyChanged("ShowEmptyDevices");
                    Devices_CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
                }
            }
        }

        bool _isShowingHiddenChannels = false;
        public bool IsShowingHiddenChannels
        {
            get => _isShowingHiddenChannels;
            set
            {
                _isShowingHiddenChannels = value;
                RaisePropertyChanged(nameof(IsShowingHiddenChannels));
                UpdateAllDeviceSettings();
            }
        }

        bool _isShowingExpiredChannels = false;
        public bool IsShowingExpiredChannels
        {
            get => _isShowingExpiredChannels;
            set
            {
                _isShowingExpiredChannels = value;
                RaisePropertyChanged(nameof(IsShowingExpiredChannels));
                UpdateAllDeviceSettings();
            }
        }

        bool _isShowingInactiveChannels = false;
        public bool IsShowingInactiveChannels
        {
            get => _isShowingInactiveChannels;
            set
            {
                _isShowingInactiveChannels = value;
                RaisePropertyChanged(nameof(IsShowingInactiveChannels));
                UpdateAllDeviceSettings();
            }
        }

        IAudioDeviceManager _manager;
        Timer _peakMeterTimer;

        public FullWindowViewModel(IAudioDeviceManager manager)
        {
            // TODO configuration store
            _showEmptyDevices = false;
            _isShowingInactiveChannels = true;

            _manager = manager;
            Devices = new ObservableCollection<DeviceViewModel>();

            _manager.Devices.CollectionChanged += Devices_CollectionChanged;
            PopuldateDevices();

            _peakMeterTimer = new Timer(1000 / 30);
            _peakMeterTimer.AutoReset = true;
            _peakMeterTimer.Elapsed += PeakMeterTimer_Elapsed;
            _peakMeterTimer.Start();
        }

        private void PeakMeterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach(var device in Devices)
            {
                device.TriggerPeakCheck();
            }
        }

        void PopuldateDevices()
        {
            foreach(var device in _manager.Devices) AddDeviceIfApplicable(device);
        }

        void AddDeviceIfApplicable(IAudioDevice device)
        {
            if (!ShowEmptyDevices)
            {
                if (device.Sessions.Sessions.Count == 1) return; // System sounds session
            }

            var newDevice = new DeviceViewModel(device);
            Devices.AddSorted(newDevice, DeviceViewModelComparer.Instance);

            UpdateDeviceSettings(newDevice);
        }

        void UpdateAllDeviceSettings()
        {
            foreach (var device in Devices) UpdateDeviceSettings(device);
        }

        void UpdateDeviceSettings(DeviceViewModel device)
        {
            device.IsShowingExpiredChannels = IsShowingExpiredChannels;
            device.IsShowingHiddenChannels = IsShowingHiddenChannels;
            device.IsShowingInactiveChannels = IsShowingInactiveChannels;
        }

        private void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    AddDeviceIfApplicable((IAudioDevice)e.NewItems[0]);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    Devices.Remove(Devices.First(x => x.Device.Id == ((IAudioDevice)e.OldItems[0]).Id));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Devices.Clear();
                    PopuldateDevices();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }
    }
}
