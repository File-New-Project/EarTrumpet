namespace EarTrumpet.UI.ViewModels
{
    public class EarTrumpetCommunitySettingsPageViewModel : SettingsPageViewModel
    {
        private readonly AppSettings _settings;
        public bool UseLogarithmicVolume
        {
            get => _settings.UseLogarithmicVolume;
            set => _settings.UseLogarithmicVolume = value;
        }

        public bool ShowRecordingDevicesInContextMenu
        {
            get => _settings.ShowRecordingDevicesInContextMenu;
            set => _settings.ShowRecordingDevicesInContextMenu = value;
        }

        public bool ShowDeviceTypeSwitchInFlyout
        {
            get => _settings.ShowDeviceTypeSwitchInFlyout;
            set => _settings.ShowDeviceTypeSwitchInFlyout = value;
        }

        public HotkeyViewModel ToggleShowDeviceTypeSwitchInFlyoutHotkey { get; }
        
        public EarTrumpetCommunitySettingsPageViewModel(AppSettings settings) : base(null)
        {
            _settings = settings;
            Title = Properties.Resources.CommunitySettingsPageText;
            Glyph = "\xE902";
            ToggleShowDeviceTypeSwitchInFlyoutHotkey = new HotkeyViewModel(settings.ToggleShowDeviceTypeSwitchInFlyoutHotkey, (newHotkey) => settings.ToggleShowDeviceTypeSwitchInFlyoutHotkey = newHotkey);
        }
    }
}