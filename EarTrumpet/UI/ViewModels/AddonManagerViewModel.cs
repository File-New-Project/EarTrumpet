using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class AddonManagerViewModel
    {
        public string Title => Properties.Resources.AddonManagerTitleText;

        public ICommand Add { get; }

        public ObservableCollection<AddonViewModel> BuiltIn { get; }
        public ObservableCollection<AddonViewModel> ThirdParty { get; }

        private AddonManager _manager;

        public AddonManagerViewModel(AddonManager manager)
        {
            _manager = manager;
            BuiltIn = new ObservableCollection<AddonViewModel>(manager.BuiltIn.Select(a => Create(a)));
            ThirdParty = new ObservableCollection<AddonViewModel>(manager.ThirdParty.Select(a => Create(a)));

            Add = new RelayCommand(() =>
            {
                var dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = "addon.dll";
                dlg.DefaultExt = ".dll";
                dlg.Filter = $"{Properties.Resources.LoadAddonAddonsText} (.DLL)|*.dll";

                if (dlg.ShowDialog() == true)
                {
                    var newAddon = new Addon(new DirectoryCatalog(Path.GetDirectoryName(dlg.FileName), Path.GetFileName(dlg.FileName)));
                    ThirdParty.Add(Create(newAddon));

                    var paths = _manager.UserDefinedAddons.ToList();
                    paths.Add(newAddon.DisplayName);
                    _manager.UserDefinedAddons = paths.ToArray();
                }
            });
        }

        private AddonViewModel Create(Addon addon)
        {
            var ret = new AddonViewModel(addon);
            ret.Remove = new RelayCommand(() =>
            {
                ThirdParty.Remove(ret);
                _manager.UserDefinedAddons = _manager.UserDefinedAddons.Where(
                    p => p.ToLower() != addon.DisplayName.ToLower()).ToArray();
            });
            return ret;
        }
    }
}
