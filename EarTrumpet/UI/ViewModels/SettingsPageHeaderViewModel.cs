namespace EarTrumpet.UI.ViewModels
{
    public class SettingsPageHeaderViewModel : BindableBase
    {
        public string Title => _settingsPageViewModel.Title;

        private SettingsPageViewModel _settingsPageViewModel;

        public SettingsPageHeaderViewModel(SettingsPageViewModel settingsPageViewModel)
        {
            _settingsPageViewModel = settingsPageViewModel;
        }
    }
}
