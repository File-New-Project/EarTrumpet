using System.Collections.Specialized;
using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    // This device follows the default device so a client does not need to pay attention to default device change events.
    public class VirtualDefaultAudioDevice : IAudioDevice, IVirtualDefaultAudioDevice
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        IAudioDevice m_device;
        IAudioDeviceManager m_manager;

        public VirtualDefaultAudioDevice(IAudioDeviceManager manager)
        {
            m_manager = manager;

            Setup();

            manager.DefaultDeviceChanged += (_, __) =>
            {
                if (m_device != null)
                {
                    m_device.Sessions.Sessions.CollectionChanged -= Sessions_CollectionChanged;
                    m_device.PropertyChanged -= Device_PropertyChanged;
                }

                Setup();

                CollectionChanged?.Invoke(null, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDevicePresent"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Volume"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMuted"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayName"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Id"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Sessions"));
            };
        }

        private void Setup()
        {
            m_device = m_manager.DefaultDevice;

            if (m_device != null)
            {
                m_device.PropertyChanged += Device_PropertyChanged;
                m_device.Sessions.Sessions.CollectionChanged += Sessions_CollectionChanged;
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

        public bool IsDevicePresent => m_device != null;

        public string DisplayName => m_device != null ? m_device.DisplayName : null;

        public string Id => m_device != null ? m_device.Id : null;

        public bool IsMuted { get => m_device != null ? m_device.IsMuted : false; set => m_device.IsMuted = value; }

        public IAudioDeviceSessionCollection Sessions => m_device != null ? m_device.Sessions : null;

        public float Volume { get => m_device != null ? m_device.Volume : 0; set => m_device.Volume = value; }

        public float PeakValue { get => m_device != null ? m_device.PeakValue : 0; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
