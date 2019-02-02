using EarTrumpet.UI.ViewModels;


namespace EarTrumpet.UI.ViewModels
{
    // Intended to be used as a base class for every type of settings page
    public class SettingsPageViewModel : BindableBase
    {
        public string GroupName { get; }
        public string Glyph { get; protected set; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public SettingsPageViewModel(string groupName)
        {
            GroupName = groupName;
        }
    }
}
