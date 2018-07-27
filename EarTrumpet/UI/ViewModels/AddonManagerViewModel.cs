using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class AddonManagerViewModel
    {
        public event Action<Addon> Added;
        public event Action<Addon> Removed;

        public string Title => Properties.Resources.AddonManagerTitleText;

        public ICommand Add { get; }

        public ObservableCollection<AddonViewModel> BuiltIn { get; }
        public ObservableCollection<AddonViewModel> ThirdParty { get; }

        public AddonManagerViewModel(AddonManager manager)
        {
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
                    var a = new Addon(dlg.FileName);
                    ThirdParty.Add(Create(a));
                    Added(a);
                }
            });
        }

        private AddonViewModel Create(Addon a)
        {
            var ret = new AddonViewModel(a);
            ret.Remove = new RelayCommand(() =>
            {
                ThirdParty.Remove(ret);
                Removed.Invoke(a);
            });
            return ret;
        }
    }
}
