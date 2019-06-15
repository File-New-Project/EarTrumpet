using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsDialogViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Button1Text { get; set; }
        public string Button2Text { get; set; }
        public ICommand Button1Command { get; set; }
        public ICommand Button2Command { get; set; }
    }
}
