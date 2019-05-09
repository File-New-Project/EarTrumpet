using EarTrumpet.UI.Helpers;


namespace EarTrumpet.UI.ViewModels
{
    // Intended to be used as a base class for every type of settings page
    public class SettingsPageViewModel : BindableBase
    {
        public static readonly string DefaultManagementGroupName = Properties.Resources.DefaultManagementGroupName;

        public string GroupName { get; }
        public string NavigationId { get; }
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

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                }
            }
        }

        public SettingsPageHeaderViewModel Header { get; protected set; }

        public virtual bool NavigatingFrom(NavigationCookie cookie)
        {
            return true;
        }

        public SettingsPageViewModel(string groupName)
        {
            GroupName = groupName;
            Header = new SettingsPageHeaderViewModel(this);
        }

        public void NavigatedTo()
        {

        }

        public override string ToString() => Title;
    }
}
