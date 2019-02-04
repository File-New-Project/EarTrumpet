using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.UI.Helpers;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class AddonAboutPageViewModel : SettingsPageViewModel
    {
        public string DisplayName => _addon.DisplayName;
        public string PublisherName => _addon.PublisherName;
        public string Version => _addon.Version.ToString();

        public ICommand OpenHelpLink { get; }
        public ICommand Uninstall { get; }

        Addon _addon;

        public AddonAboutPageViewModel(object addonObject) : base(DefaultManagementGroupName)
        {
            Title = Properties.Resources.AboutThisAddonText;
            Glyph = "\xE946";
            _addon = AddonManager.Current.FindAddonForObject(addonObject);
            OpenHelpLink = new RelayCommand(() => ProcessHelper.StartNoThrow(_addon.HelpLink));
            Uninstall = new RelayCommand(() => ProcessHelper.StartNoThrow("ms-settings:appsfeatures"));
        }
    }
}
