using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Serialization;
using EarTrumpet_Actions.ViewModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    public class ActionsCategoryViewModel : SettingsCategoryViewModel
    {
        public ActionsCategoryViewModel()
        {
            Title = "Actions";
            Description = "Create hotkeys and macros to control your audio experience";
            Glyph = "\xE164";

            Pages = new System.Collections.ObjectModel.ObservableCollection<SettingsPageViewModel>(Addon.Current.Actions.Select(a => new EarTrumpetActionViewModel(this, a)));

            Pages.Add(new ImportExportPageViewModel());
            Pages.Add(new AddonAboutPageViewModel(this));

            Toolbar = new ToolbarItemViewModel[] { new ToolbarItemViewModel{
                Command = new RelayCommand(() =>
                {
                    var vm = new EarTrumpetActionViewModel(this, new EarTrumpetAction { DisplayName = Properties.Resources.NewActionText });
                    Pages.Add(vm);
                    Selected = vm;
                }),
                DisplayName = "Add new action",
                Glyph = "\xE948",
                GlyphFontSize = 15,
            } };
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
