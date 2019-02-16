using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel
{
    class DefaultPlaybackDeviceViewModel : DeviceViewModelBase
    {
        public DefaultPlaybackDeviceViewModel()
        {
            DisplayName = Properties.Resources.DefaultPlaybackDeviceText;
            Kind = EarTrumpet.DataModel.AudioDeviceKind.Playback;
            GroupName = Properties.Resources.PlaybackDeviceGroupText;
        }
    }
}
