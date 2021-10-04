using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.Actions.DataModel.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.Actions.ViewModel
{
    public class ActionsCategoryViewModel : SettingsCategoryViewModel
    {
        public ActionsCategoryViewModel()
            : base(Properties.Resources.MyActionsText, "\xE950", Properties.Resources.AddonDescriptionText, EarTrumpetActionsAddon.Current.Manifest.Id, new ObservableCollection<SettingsPageViewModel>())
        {
            // Get a 'fresh' copy so that we can edit the objects and still go back later.
            var actions = EarTrumpetActionsAddon.Current.Actions;
            EarTrumpetActionsAddon.Current.Actions = EarTrumpetActionsAddon.Current.Actions;

            Pages.AddRange(actions.Select(a => new EarTrumpetActionViewModel(this, a)));
            Pages.Add(new ImportExportPageViewModel(this));

            Toolbar = new ToolbarItemViewModel[] { new ToolbarItemViewModel{
                Command = new RelayCommand(() =>
                {
                    var vm = new EarTrumpetActionViewModel(this, new EarTrumpetAction { DisplayName = Properties.Resources.NewActionText });
                    vm.IsWorkSaved = false;
                    vm.IsPersisted = false;

                    vm.PropertyChanged += (_, e) =>
                    {
                        if (e.PropertyName == nameof(vm.IsSelected) &&
                            vm.IsSelected && !Pages.Contains(vm))
                        {
                            Pages.Insert(0, vm);
                        }
                    };

                    Selected = vm;
                }),
                DisplayName = Properties.Resources.NewActionText,
                Glyph = "\xE948",
                GlyphFontSize = 15,
            } };

            if (Pages.Count == 2)
            {
                Toolbar[0].Command.Execute(null);
            }
        }

        internal void ReloadSavedPages()
        {
            foreach (var item in Pages.Where(p => p is EarTrumpetActionViewModel).ToList())
            {
                Pages.Remove(item);
            }

            Pages.InsertRange(0, new System.Collections.ObjectModel.ObservableCollection<SettingsPageViewModel>(EarTrumpetActionsAddon.Current.Actions.Select(a => new EarTrumpetActionViewModel(this, a))));
            Selected = Pages[0];
        }

        public void Delete(EarTrumpetActionViewModel earTrumpetActionViewModel, bool promptOverride = false)
        {
            Action doRemove = () =>
            {
                var actions = EarTrumpetActionsAddon.Current.Actions.ToList();
                if (actions.Any(a => a.Id == earTrumpetActionViewModel.Id))
                {
                    actions.Remove(item => item.Id == earTrumpetActionViewModel.Id);
                }
                EarTrumpetActionsAddon.Current.Actions = actions.ToArray();

                if (Pages.Any(a => a == earTrumpetActionViewModel))
                {
                    Pages.Remove(earTrumpetActionViewModel);
                }
            };

            if (earTrumpetActionViewModel.IsPersisted && !promptOverride)
            {
                _parent.ShowDialog(Properties.Resources.DeleteActionDialogTitle, Properties.Resources.DeleteActionDialogText,
                    Properties.Resources.DeleteActionDialogYesText, Properties.Resources.DeleteActionDialogNoText, doRemove, () => { });
            }
            else
            {
                doRemove();
            }
        }

        public void Save(EarTrumpetActionViewModel earTrumpetActionViewModel)
        {
            var actions = EarTrumpetActionsAddon.Current.Actions.ToList();
            if (actions.Any(a => a.Id == earTrumpetActionViewModel.Id))
            {
                actions.Remove(item => item.Id == earTrumpetActionViewModel.Id);
            }
            actions.Insert(0, earTrumpetActionViewModel.GetAction());
            EarTrumpetActionsAddon.Current.Actions = actions.ToArray();
            earTrumpetActionViewModel.IsWorkSaved = true;

            if (Pages.Any(a => a == earTrumpetActionViewModel))
            {
                Pages.Remove(earTrumpetActionViewModel);
            }
            Pages.Insert(0, earTrumpetActionViewModel);
            Selected = Pages[0];
        }

        public void CompleteNavigation(NavigationCookie cookie)
        {
            _parent.CompleteNavigation(cookie);
        }
    }
}
