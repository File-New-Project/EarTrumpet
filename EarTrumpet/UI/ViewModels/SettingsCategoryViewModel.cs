using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class SettingsCategoryViewModel : BindableBase, IWindowHostedViewModel
    {
        protected ISettingsViewModel _parent;
        SettingsPageViewModel _selected;
#pragma warning disable CS0067
        public event Action Close;
        public event Action<object> HostDialog;
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
                        if (!_selected.NavigatingFrom(new NavigationCookie(() =>
                        {
                            SelectImpl(value);
                        })))
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
            _selected = page;

            RaisePropertyChanged(nameof(Selected));

            if (_selected != null)
            {
                _selected.NavigatedTo();
            }
        }

        public string Title { get; protected set; }
        public string Glyph { get; protected set; }
        public string Description { get; protected set; }
        public ToolbarItemViewModel[] Toolbar { get;  protected set; }

        public ObservableCollection<SettingsPageViewModel> Pages { get; protected set; }

        public SettingsCategoryViewModel() { }

        public SettingsCategoryViewModel(string title, string glyph, string description, string id, IEnumerable<SettingsPageViewModel> pages)
        {
            Title = title;
            Glyph = glyph;
            Description = description;
            Id = id;
            Pages = new ObservableCollection<SettingsPageViewModel>(pages);
        }

        public virtual void OnClosing()
        {
            if (Pages == null) return;

            foreach (var page in Pages)
            {
                if (page is IWindowHostedViewModel)
                {
                    ((IWindowHostedViewModel)page).OnClosing();
                }
            }
        }

        public void OnPreviewKeyDown(KeyEventArgs e)
        {
    
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
    }
}
