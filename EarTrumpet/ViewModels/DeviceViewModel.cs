using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet.ViewModels
{
    public class DeviceViewModel
    {
        public VolumeControlChannelViewModel Device { get; private set; }
        public ObservableCollection<AppItemViewModel> Apps { get; private set; }

        IAudioDevice _device;

        public DeviceViewModel(IAudioDevice device)
        {
            _device = device;
            Device = new VolumeControlChannelViewModel(device);
            Apps = new ObservableCollection<AppItemViewModel>();

            PopulateAppSessions();

            _device.Sessions.Sessions.CollectionChanged += Sessions_CollectionChanged;
            _device.PropertyChanged += Device_PropertyChanged;
        }

        public void TriggerPeakCheck()
        {
            Device.TriggerPeakCheck();

            foreach (var app in Apps) app.TriggerPeakCheck();
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Sessions")
            {
                _device.Sessions.Sessions.CollectionChanged += Sessions_CollectionChanged;
            }
        }

        private void PopulateAppSessions()
        {
            foreach (var session in _device.Sessions.Sessions)
            {
                Apps.AddSorted(new AppItemViewModel(session), AppItemViewModelComparer.Instance);
            }
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    Apps.AddSorted(new AppItemViewModel((IAudioDeviceSession)e.NewItems[0]), AppItemViewModelComparer.Instance);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    Apps.Remove(Apps.First(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Apps.Clear();
                    PopulateAppSessions();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }
    }
}
