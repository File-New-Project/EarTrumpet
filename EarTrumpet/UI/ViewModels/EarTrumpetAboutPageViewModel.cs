using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using System;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class EarTrumpetAboutPageViewModel : SettingsPageViewModel
    {
        public RelayCommand OpenDiagnosticsCommand { get; }
        public RelayCommand OpenAboutCommand { get; }
        public RelayCommand OpenFeedbackCommand { get; }
        public string AboutText { get; }

        public EarTrumpetAboutPageViewModel() : base(null)
        {
            Glyph = "\xE946";
            Title = Properties.Resources.AboutTitle;
            AboutText = $"EarTrumpet {App.Current.GetVersion()}";

            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
            OpenFeedbackCommand = new RelayCommand(FeedbackService.OpenFeedbackHub);
        }

        private void OpenDiagnostics()
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                throw new Exception("This is an intentional crash.");
            }

            DiagnosticsService.DumpAndShowData();
        }

        private void OpenAbout() => ProcessHelper.StartNoThrow("https://github.com/File-New-Project/EarTrumpet");
    }
}
