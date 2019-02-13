using EarTrumpet.Extensibility;
using EarTrumpet.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsViewModel : BindableBase, IWindowHostedViewModel, ISettingsViewModel
    {
        public static IAddonSettingsPage[] AddonItems { get; internal set; }

        public event Action Close;

        public string Title { get; private set; }
        public ICommand GoHome { get; }
        public BackstackViewModel Backstack { get; } = new BackstackViewModel();
        public ObservableCollection<SettingsCategoryViewModel> Categories { get; private set; }

        private SimpleDialogViewModel _dialog;
        public SimpleDialogViewModel Dialog
        {
            get => _dialog;
            set
            {
                if (_dialog != value)
                {
                    _dialog = value;
                    RaisePropertyChanged(nameof(Dialog));
                }
            }
        }

        public void OnInvoked(object sender, SettingsCategoryViewModel toSelect)
        {
            Selected = toSelect;
        }

        SettingsCategoryViewModel _selected;
        public SettingsCategoryViewModel Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    if (value != null && value is AdvertisedCategorySettingsViewModel)
                    {
                        ((AdvertisedCategorySettingsViewModel)value).Activate();
                        RaisePropertyChanged(nameof(Selected));
                        return;
                    }

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

        public SettingsViewModel(string title, IEnumerable<SettingsCategoryViewModel> categories)
        {
            Title = title;
            Categories = new ObservableCollection<SettingsCategoryViewModel>(categories);
            GoHome = new RelayCommand(() => Selected = null);
        }

        public void InvokeSearchResult(SettingsCategoryViewModel cat, SettingsPageViewModel page)
        {
            if (Selected != null && !Selected.NavigatingFrom(new NavigationCookie(() =>
                {
                    Selected = cat;
                    Selected.Selected = page;
                })))
            {
                return;
            }

            Selected = cat;
            Selected.Selected = page;
        }

        private void SelectImpl(SettingsCategoryViewModel categoryToSelect)
        {
            if (!Backstack.IsDisablingUpdates)
            {
                var oldSelected = _selected;
                var oldPage = _selected == null ? null : _selected.Selected;
                Backstack.Add(() =>
                {
                    Backstack.IsDisablingUpdates = true;
                    Selected = oldSelected;
                    if (Selected != null)
                    {
                        Selected.Selected = oldPage;
                    }
                    Backstack.IsDisablingUpdates = false;
                });
            }

            _selected = categoryToSelect;
            if (_selected != null && _selected.Pages.Count > 0)
            {
                _selected.Selected = _selected.Pages[0];
            }

            RaisePropertyChanged(nameof(Selected));

            if (_selected != null)
            {
                _selected.NavigatedTo(this);
            }
        }

        public void OnClosing()
        {
            if (Selected != null && !Selected.NavigatingFrom(new NavigationCookie(() => Close?.Invoke())))
            {
                return;
            }
            Close?.Invoke();
        }

        public void ShowDialog(string title, string description, string btn1, string btn2, Action btn1Clicked, Action btn2Clicked)
        {
            Dialog = new SimpleDialogViewModel
            {
                Title = title,
                Description = description,
                Button1Text = btn1,
                Button2Text = btn2,
                Button1Command = new RelayCommand(() =>
                {
                    Dialog = null;
                    btn1Clicked();
                }),
                Button2Command = new RelayCommand(() =>
                {
                    Dialog = null;
                    btn2Clicked();
                })
            };
        }

        public void CompleteNavigation(NavigationCookie cookie)
        {
            cookie.Execute();
        }
    }
}
