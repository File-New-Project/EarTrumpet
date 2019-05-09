using EarTrumpet.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EarTrumpet.UI.ViewModels
{
    public class SettingsCategoryViewModel : BindableBase
    {
        protected ISettingsViewModel _parent;
        SettingsPageViewModel _selected;

        public string Id { get; protected set; }
        public bool IsAd { get; protected set; }

        public SettingsPageViewModel Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    if (_selected != null)
                    {
                        if (!_selected.NavigatingFrom(new NavigationCookie(() => SelectImpl(value))))
                        {
                            RaisePropertyChanged(nameof(Selected));
                            return;
                        }
                    }

                    SelectImpl(value);
                }
            }
        }

        private void SelectImpl(SettingsPageViewModel page)
        {
            if (_selected != null)
            {
                _selected.IsSelected = false;
            }

            _selected = page;
            RaisePropertyChanged(nameof(Selected));

            if (_selected != null)
            {
                _selected.IsSelected = true;
                _selected.NavigatedTo();
            }
        }

        public string Title { get; protected set; }
        public string Glyph { get; protected set; }
        public string Description { get; protected set; }
        public ToolbarItemViewModel[] Toolbar { get; protected set; }
        public ObservableCollection<SettingsPageViewModel> Pages { get; protected set; }

        public SettingsCategoryViewModel(string title, string glyph, string description, string id, IEnumerable<SettingsPageViewModel> pages)
        {
            Title = title;
            Glyph = glyph;
            Description = description;
            Id = id;
            Pages = new ObservableCollection<SettingsPageViewModel>(pages);
        }

        public void NavigatedTo(ISettingsViewModel settingsViewModel)
        {
            _parent = settingsViewModel;
        }

        public bool NavigatingFrom(NavigationCookie cookie)
        {
            if (_selected != null)
            {
                return _selected.NavigatingFrom(cookie);
            }
            return true;
        }

        public void ShowDialog(string title, string description, string btn1, Action btn1Clicked, string btn2, Action btn2Clicked)
        {
            _parent.ShowDialog(title, description, btn1, btn2, btn1Clicked, btn2Clicked);
        }

        public override string ToString() => $"{Title}\n{Description}";
    }
}
