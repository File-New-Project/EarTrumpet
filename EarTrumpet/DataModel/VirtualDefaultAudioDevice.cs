using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    // This device follows the default device so a client does not need to pay attention to default device change events.
    public class VirtualDefaultAudioDevice : IAudioDevice, IVirtualDefaultAudioDevice
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        IAudioDevice _device;
        IAudioDeviceManager _manager;

        public VirtualDefaultAudioDevice(IAudioDeviceManager manager)
        {
            _manager = manager;

            Setup();

            manager.DefaultPlaybackDeviceChanged += (_, __) =>
            {
                if (_device != null)
                {
                    _device.Sessions.CollectionChanged -= Sessions_CollectionChanged;
                    _device.PropertyChanged -= Device_PropertyChanged;
                }

                Setup();

                CollectionChanged?.Invoke(null, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDevicePresent)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sessions)));
            };
        }

        private void Setup()
        {
            _device = _manager.DefaultPlaybackDevice;

            if (_device != null)
            {
                _device.PropertyChanged += Device_PropertyChanged;
                _device.Sessions.CollectionChanged += Sessions_CollectionChanged;
            }
        }

        private void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public bool IsDevicePresent => _device != null;

        public string DisplayName => _device != null ? _device.DisplayName : null;

        public string Id => _device != null ? _device.Id : null;

        public bool IsMuted { get => _device != null ? _device.IsMuted : false; set => _device.IsMuted = value; }

        public ObservableCollection<IAudioDeviceSession> Sessions => _device != null ? _device.Sessions : null;

        public float Volume { get => _device != null ? _device.Volume : 0; set => _device.Volume = value; }

        public float PeakValue { get => _device != null ? _device.PeakValue : 0; }

        public void TakeSessionFromOtherDevice(int processId) => _device.TakeSessionFromOtherDevice(processId);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
