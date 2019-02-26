using EarTrumpet.DataModel.WindowsAudio;

namespace EarTrumpet.Actions.ViewModel
{
    class DefaultPlaybackDeviceViewModel : DeviceViewModelBase
    {
        public DefaultPlaybackDeviceViewModel()
        {
            DisplayName = Properties.Resources.DefaultPlaybackDeviceText;
            Kind = AudioDeviceKind.Playback.ToString();
            GroupName = Properties.Resources.PlaybackDeviceGroupText;
        }
    }
}
