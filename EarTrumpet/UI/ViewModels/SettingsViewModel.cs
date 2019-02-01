using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EarTrumpet.Extensibility;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsViewModel : BindableBase, IWindowHostedViewModel, IWindowHostedViewModelInternal
    {
        public static IAddonSettingsPage[] AddonItems { get; internal set; }

        public string Title { get; private set; }

        SettingsCategoryViewModel _selected;
#pragma warning disable CS0067
        public event Action Close;
        public event Action<object> HostDialog;

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
                    if (_selected != null && _selected is IWindowHostedViewModel)
                    {
                        ((IWindowHostedViewModel)_selected).HostDialog += (d) => HostDialog(d);
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

        public void OnClosing()
        {
            foreach(var cat in Categories)
            {
                if (cat is IWindowHostedViewModel)
                {
                    ((IWindowHostedViewModel)cat).OnClosing();
                }
            }
        }

        public void OnPreviewKeyDown(KeyEventArgs e)
        {

        }

        void IWindowHostedViewModelInternal.HostDialog(object dialog) => HostDialog(dialog);
    }
}
