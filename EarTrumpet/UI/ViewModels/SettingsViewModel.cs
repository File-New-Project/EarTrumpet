using System.Collections.Generic;
using System.Collections.ObjectModel;
using EarTrumpet.Extensibility;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsViewModel : BindableBase
    {
        public static IAddonSettingsPage[] AddonItems { get; internal set; }

        public string Title { get; private set; }

        SettingsCategoryViewModel _selected;
        public SettingsCategoryViewModel Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (_selected != null && _selected.Pages.Count > 0)
                    {
                        _selected.Selected = _selected.Pages[0];
                    }
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }

        public ObservableCollection<SettingsCategoryViewModel> Categories { get; private set; }

        public SettingsViewModel(string title, IEnumerable<SettingsCategoryViewModel> categories)
        {
            Title = title;
            Categories = new ObservableCollection<SettingsCategoryViewModel>(categories);
        }
    }
}
