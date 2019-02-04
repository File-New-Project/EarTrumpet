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
            Title = Properties.Resources.MyActionsText;
            Description = Properties.Resources.AddonDescriptionText;
            Glyph = "\xE950";
            Id = Addon.Current.Info.Id;
            Pages = new System.Collections.ObjectModel.ObservableCollection<SettingsPageViewModel>(Addon.Current.Actions.Select(a => new EarTrumpetActionViewModel(this, a)));

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

        internal void Delete(EarTrumpetActionViewModel earTrumpetActionViewModel)
        {
            Pages.Remove(earTrumpetActionViewModel);
        }

        internal void Save(EarTrumpetActionViewModel earTrumpetActionViewModel)
        {
            // TODO
        }

        public override void OnClosing()
        {
            Addon.Current.Actions = Pages.Where(p => p is EarTrumpetActionViewModel).Select(p => ((EarTrumpetActionViewModel)p).GetAction()).ToArray();
        }
    }
}
