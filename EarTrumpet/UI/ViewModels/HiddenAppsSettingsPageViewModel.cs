using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class EarTrumpetHiddenAppsSettingsPageViewModel : SettingsPageViewModel
    {
        public ObservableCollection<SettingsAppItemViewModel> HiddenApps { get; }
        public ICommand RemoveApp { get; set; }

        public EarTrumpetHiddenAppsSettingsPageViewModel(AppSettings settings) : base(null)
        {
            Title = Properties.Resources.HiddenAppsSettingsPageText;
            Glyph = "\xE890";

            HiddenApps = new ObservableCollection<SettingsAppItemViewModel>();
            HiddenApps.AddRange(settings.HiddenApps
                .Select(app => new SettingsAppItemViewModel
                {
                    AppId = app.AppId,
                    DisplayName = app.DisplayName,
                    IconText = default,
                    IconPath = app.IconPath,
                    Background = app.Background
                })
            );

            RemoveApp = new RelayCommand<SettingsAppItemViewModel>((appItem) =>
            {
                settings.RemoveHiddenApp(appItem.AppId);
                HiddenApps.Remove(appItem);
            });
        }

        public override bool NavigatingFrom(NavigationCookie cookie)
        {
            //Addon.Current.HiddenApps = Apps.Select(a => a.AppId).ToArray();
            return base.NavigatingFrom(cookie);
        }
    }
}