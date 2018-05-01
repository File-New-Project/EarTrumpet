using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace EarTrumpet.ViewModels
{
    public class FullWindowViewModel : BindableBase
    {
        public ObservableCollection<DeviceViewModel> Devices { get; private set; }

        IAudioDeviceManager _manager;
        Timer _peakMeterTimer;

        public FullWindowViewModel(IAudioDeviceManager manager)
        {
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
            foreach(var device in _manager.Devices) AddDevice(device);
        }

        void AddDevice(IAudioDevice device)
        {
            var newDevice = new DeviceViewModel(_manager, device);
            Devices.AddSorted(newDevice, DeviceViewModelComparer.Instance);
        }

        private void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    AddDevice((IAudioDevice)e.NewItems[0]);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    var device = Devices.FirstOrDefault(x => x.Device.Id == ((IAudioDevice)e.OldItems[0]).Id);
                    if (device != null)
                    {
                        Devices.Remove(device);
                    }
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
