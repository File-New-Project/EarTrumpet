using System;
using EarTrumpet.UI.Helpers;

namespace EarTrumpet.UI.ViewModels
{
    class HardwareSettingsViewModel : BindableBase
    {
        public string Title { get; private set; }
        
        public HardwareSettingsViewModel(string title)
        {
            Title = title;
        }
    }
}