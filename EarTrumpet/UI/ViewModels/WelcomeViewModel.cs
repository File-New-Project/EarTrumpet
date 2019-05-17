using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class WelcomeViewModel
    {
        public string VisibleTitle => ""; // We have a header instead
        public string Title { get; }
        public ICommand Close { get; set; }
        public ICommand LearnMore { get; }

        public WelcomeViewModel()
        {
            // Used for the window title.
            Title = Properties.Resources.WelcomeDialogHeaderText;
            LearnMore = new RelayCommand(() =>
            {
                ProcessHelper.StartNoThrow("https://github.com/File-New-Project/EarTrumpet");
            });
        }
    }
}
