using EarTrumpet.Extensibility;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class AddonAboutPageViewModel : SettingsPageViewModel
    {
        public string DisplayName => _addon.DisplayName;
        public string PublisherName => _addon.PublisherName;
        public string Version => _addon.AddonVersion.ToString();
        public ICommand OpenHelpLink { get; }
        public ICommand Uninstall { get; }

        AddonInfo _addon;

        public AddonAboutPageViewModel(AddonInfo addon) : base(DefaultManagementGroupName)
        {
            Glyph = "\xE946";
            _addon = addon;
            Title = Properties.Resources.AboutThisAddonText.Replace("{Name}", DisplayName);

            OpenHelpLink = new RelayCommand(() => ProcessHelper.StartNoThrow(_addon.HelpLink));
            Uninstall = new RelayCommand(() => ProcessHelper.StartNoThrow("ms-settings:appsfeatures"));
        }
    }
}
