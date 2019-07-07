using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.UI.Helpers;

namespace EarTrumpet.Actions.ViewModel
{
    public class DeviceViewModel : DeviceViewModelBase, IAppIconSource
    {
        public bool IsDesktopApp => true;
        public string IconPath => _device.IconPath;
        public string DeviceDescription => ((IAudioDeviceWindowsAudio)_device).DeviceDescription;
        public string EnumeratorName => ((IAudioDeviceWindowsAudio)_device).EnumeratorName;
        public string InterfaceName => ((IAudioDeviceWindowsAudio)_device).InterfaceName;

        private readonly IAudioDevice _device;

        public DeviceViewModel(IAudioDevice device)
        {
            _device = device;
            Id = _device.Id;
            DisplayName = _device.DisplayName;
            Kind = _device.Parent.Kind;

            GroupName = _device.Parent.Kind == AudioDeviceKind.Playback.ToString() ?
                Properties.Resources.PlaybackDeviceGroupText :
                Properties.Resources.RecordingDeviceGroupText;
        }
    }
}
