using EarTrumpet.DataModel;
using EarTrumpet.UI;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.Actions.ViewModel
{
    public class DeviceViewModel : DeviceViewModelBase
    {
        public IconLoadInfo Icon { get; }

        private IAudioDevice _device;

        public DeviceViewModel(IAudioDevice device)
        {
            _device = device;
            Id = _device.Id;
            DisplayName = _device.DisplayName;
            Icon = new IconLoadInfo { IconPath = _device.IconPath, IsDesktopApp = true };
            Kind = _device.Parent.Kind;

            GroupName = _device.Parent.Kind == AudioDeviceKind.Playback.ToString() ?
                Properties.Resources.PlaybackDeviceGroupText :
                Properties.Resources.RecordingDeviceGroupText;
        }
    }
}
