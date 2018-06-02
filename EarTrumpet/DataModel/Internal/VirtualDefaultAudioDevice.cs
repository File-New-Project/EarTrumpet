using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace EarTrumpet.DataModel.Internal
{
    // This device follows the default device so a client does not need to pay attention to default device change events.
    class VirtualDefaultAudioDevice : IAudioDevice, IVirtualDefaultAudioDevice
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private IAudioDevice _device;
        private IAudioDeviceManager _manager;

        public VirtualDefaultAudioDevice(IAudioDeviceManager manager)
        {
            _manager = manager;

            Setup();

            _manager.DefaultPlaybackDeviceChanged += Manager_DefaultPlaybackDeviceChanged;
        }

        ~VirtualDefaultAudioDevice()
        {
            _manager.DefaultPlaybackDeviceChanged -= Manager_DefaultPlaybackDeviceChanged;

            if (_device != null)
            {
                _device.Groups.CollectionChanged -= Sessions_CollectionChanged;
                _device.PropertyChanged -= Device_PropertyChanged;
            }
        }

        private void Manager_DefaultPlaybackDeviceChanged(object sender, IAudioDevice e)
        {
            if (_device != null)
            {
                _device.Groups.CollectionChanged -= Sessions_CollectionChanged;
                _device.PropertyChanged -= Device_PropertyChanged;
            }

            Setup();

            CollectionChanged?.Invoke(null, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDevicePresent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Groups)));
        }

        private void Setup()
        {
            _device = _manager.DefaultPlaybackDevice;

            if (_device != null)
            {
                _device.PropertyChanged += Device_PropertyChanged;
                _device.Groups.CollectionChanged += Sessions_CollectionChanged;
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

        public ObservableCollection<IAudioDeviceSession> Groups => _device != null ? _device.Groups : null;

        public float Volume { get => _device != null ? _device.Volume : 0; set => _device.Volume = value; }

        public float PeakValue { get => _device != null ? _device.PeakValue : 0; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
