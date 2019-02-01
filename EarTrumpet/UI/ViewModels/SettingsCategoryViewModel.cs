using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace EarTrumpet.UI.ViewModels
{
    public class SettingsCategoryViewModel : BindableBase
    {
        SettingsPageViewModel _selected;
        public SettingsPageViewModel Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }

        public string Title { get; protected set; }
        public string Glyph { get; protected set; }

        public string Description { get; protected set; }

        public ObservableCollection<SettingsPageViewModel> Pages { get; protected set; }

        public SettingsCategoryViewModel() { }

        public SettingsCategoryViewModel(string title, string glyph, string description, IEnumerable<SettingsPageViewModel> pages)
        {
            Title = title;
            Glyph = glyph;
            Description = description;
            Pages = new ObservableCollection<SettingsPageViewModel>(pages);
        }
    }
}
