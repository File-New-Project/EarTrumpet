using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.ViewModels
{
    public class FullWindowViewModel
    {
        public ObservableCollection<DeviceViewModel> Devices { get; private set; }

        AudioDeviceManager _manager;

        public FullWindowViewModel(AudioDeviceManager manager)
        {
            _manager = manager;
            Devices = new ObservableCollection<DeviceViewModel>();

            _manager.Devices.CollectionChanged += Devices_CollectionChanged;
            PopuldateDevices();
        }

        void PopuldateDevices()
        {
            foreach(var device in _manager.Devices)
            {
                Devices.AddSorted(new DeviceViewModel(device), DeviceViewModelComparer.Instance);
            }
        }

        private void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    Devices.AddSorted(new DeviceViewModel((IAudioDevice)e.NewItems[0]), DeviceViewModelComparer.Instance);
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
