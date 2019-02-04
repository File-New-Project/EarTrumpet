using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Serialization;
using System;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    public class ActionsCategoryViewModel : SettingsCategoryViewModel
    {
        public ActionsCategoryViewModel()
        {
            // Get a 'fresh' copy so that we can edit the objects and still go back later.
            var actions = Addon.Current.Actions;
            Addon.Current.Actions = Addon.Current.Actions;

            Title = Properties.Resources.MyActionsText;
            Description = Properties.Resources.AddonDescriptionText;
            Glyph = "\xE950";
            Id = Addon.Current.Info.Id;
            Pages = new System.Collections.ObjectModel.ObservableCollection<SettingsPageViewModel>(actions.Select(a => new EarTrumpetActionViewModel(this, a)));

            Pages.Add(new ImportExportPageViewModel(this));
            Pages.Add(new AddonAboutPageViewModel(this));

            Toolbar = new ToolbarItemViewModel[] { new ToolbarItemViewModel{
                Command = new RelayCommand(() =>
                {
                    var vm = new EarTrumpetActionViewModel(this, new EarTrumpetAction { DisplayName = Properties.Resources.NewActionText });
                    Pages.Add(vm);
                    Selected = vm;
                }),
                DisplayName = Properties.Resources.NewActionText,
                Glyph = "\xE948",
                GlyphFontSize = 15,
            } };
        }

        internal void ReloadSavedPages()
        {
            foreach(var item in Pages.Where(p => p is EarTrumpetActionViewModel).ToList())
            {
                Pages.Remove(item);
            }

            Pages.InsertRange(0, new System.Collections.ObjectModel.ObservableCollection<SettingsPageViewModel>(Addon.Current.Actions.Select(a => new EarTrumpetActionViewModel(this, a))));
            Selected = Pages[0];
        }

        public void Delete(EarTrumpetActionViewModel earTrumpetActionViewModel)
        {
            _parent.ShowDialog("Title", "Are you sure", "Delete", "Cancel", () => {

                var actions = Addon.Current.Actions.ToList();
                actions.Remove(item => item.Id == earTrumpetActionViewModel.Id);
                Addon.Current.Actions = actions.ToArray();

                Pages.Remove(earTrumpetActionViewModel);
            },
            () => { });
        }

        public void Save(EarTrumpetActionViewModel earTrumpetActionViewModel)
        {
            var actions = Addon.Current.Actions.ToList();
            actions.Remove(item => item.Id == earTrumpetActionViewModel.Id);
            actions.Insert(0, earTrumpetActionViewModel.GetAction());
            Addon.Current.Actions = actions.ToArray();
            earTrumpetActionViewModel.IsWorkSaved = true;

            Pages.Remove(earTrumpetActionViewModel);
            Pages.Insert(0, earTrumpetActionViewModel);
            Selected = Pages[0];

        }

        public void CompleteNavigation(NavigationCookie cookie)
        {
            _parent.CompleteNavigation(cookie);
        }
    }
}
