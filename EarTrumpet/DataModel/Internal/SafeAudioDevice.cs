using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EarTrumpet.DataModel.Internal
{
    // Avoid device invalidation COMExceptions from bubbling up out of devices that have been removed.
    public class SafeAudioDevice : IAudioDevice
    {
        private readonly IAudioDevice _device;

        public SafeAudioDevice(IAudioDevice device)
        {
            _device = device;
            _device.PropertyChanged += Device_PropertyChanged;
        }

        ~SafeAudioDevice()
        {
            _device.PropertyChanged -= Device_PropertyChanged;
        }

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public ObservableCollection<IAudioDeviceSession> Groups => SafeCallHelper.GetValue(() => _device.Groups);

        public string DisplayName => SafeCallHelper.GetValue(() => _device.DisplayName);

        public string Id => SafeCallHelper.GetValue(() => _device.Id);

        public bool IsMuted { get => SafeCallHelper.GetValue(() => _device.IsMuted); set => SafeCallHelper.SetValue(() => _device.IsMuted = value); }
        public float Volume { get => SafeCallHelper.GetValue(() => _device.Volume); set => SafeCallHelper.SetValue(() => _device.Volume = value); }

        public float PeakValue => SafeCallHelper.GetValue(() => _device.PeakValue);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
