using EarTrumpet.Extensibility.Hosting;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class AddonViewModel
    {
        public ICommand Remove { get; set; }
        public string DisplayName => _addon.DisplayName;

        private Addon _addon;

        public AddonViewModel(Addon addon)
        {
            _addon = addon;
        }
    }
}
