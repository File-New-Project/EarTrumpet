using EarTrumpet.DataModel;
using EarTrumpet.UI;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.Actions.ViewModel
{
    public class DeviceViewModel : DeviceViewModelBase
    {
        public IconLoadInfo Icon { get; }
        public string EnumeratorName => _device.EnumeratorName;
        public string InterfaceName => _device.InterfaceName;
        public string DeviceDescription => _device.DeviceDescription;

        private IAudioDevice _device;

        public DeviceViewModel(IAudioDevice device)
        {
            _device = device;
            Id = _device.Id;
            DisplayName = _device.DisplayName;
            Icon = new IconLoadInfo { IconPath = _device.IconPath, IsDesktopApp = true };
            Kind = _device.Parent.DeviceKind;

            GroupName = _device.Parent.DeviceKind == AudioDeviceKind.Playback ?
                Properties.Resources.PlaybackDeviceGroupText :
                Properties.Resources.RecordingDeviceGroupText;
        }
    }
}
