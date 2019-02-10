namespace EarTrumpet_Actions.ViewModel
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
