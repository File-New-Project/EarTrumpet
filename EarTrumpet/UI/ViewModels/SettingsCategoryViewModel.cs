using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class SettingsCategoryViewModel : BindableBase, IWindowHostedViewModel
    {
        SettingsPageViewModel _selected;
#pragma warning disable CS0067
        public event Action Close;
        public event Action<object> HostDialog;
        public string Id { get; protected set; }

        public SettingsPageViewModel Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (_selected != null && _selected is IWindowHostedViewModel)
                    {
                        ((IWindowHostedViewModel)_selected).HostDialog += (d) => HostDialog(d);
                    }

                    RaisePropertyChanged(nameof(Selected));
                }
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
    }
}
