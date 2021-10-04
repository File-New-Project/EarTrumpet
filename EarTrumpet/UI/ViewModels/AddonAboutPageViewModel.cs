using EarTrumpet.Extensibility;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    internal class AddonAboutPageViewModel : SettingsPageViewModel
    {
        public ICommand OpenHelpLink { get; }
        public string DisplayName => _addon.DisplayName;
        public string PublisherName => _addon.Manifest.PublisherName;
        public string Version => _addon.Manifest.Version;

        private readonly EarTrumpetAddon _addon;

        public AddonAboutPageViewModel(EarTrumpetAddon addon) : base(DefaultManagementGroupName)
        {
            Glyph = "\xE946";
            _addon = addon;

            Title = Properties.Resources.AboutThisAddonText.Replace("{Name}", DisplayName);
            OpenHelpLink = new RelayCommand(() => ProcessHelper.StartNoThrow(_addon.Manifest.HelpLink));
        }
    }
}
