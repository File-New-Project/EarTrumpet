using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class EarTrumpetAboutPageViewModel : SettingsPageViewModel
    {
        public RelayCommand OpenDiagnosticsCommand { get; }
        public RelayCommand OpenAboutCommand { get; }
        public RelayCommand OpenFeedbackCommand { get; }
        public string AboutText { get; }

        private Action _openDiagnostics;

        public EarTrumpetAboutPageViewModel(Action openDiagnostics) : base(null)
        {
            _openDiagnostics = openDiagnostics;
            Glyph = "\xE946";
            Title = Properties.Resources.AboutTitle;
            AboutText = $"EarTrumpet {App.Current.GetVersion()}";

            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
            OpenFeedbackCommand = new RelayCommand(OpenFeedbackHub);
        }

        private void OpenDiagnostics()
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Trace.WriteLine($"EarTrumpetAboutPageViewModel OpenDiagnostics - CRASH");
                throw new Exception("This is an intentional crash.");
            }

            _openDiagnostics.Invoke();
        }

        private void OpenFeedbackHub()
        {
            Trace.WriteLine($"EarTrumpetAboutPageViewModel OpenFeedbackHub");
            ProcessHelper.StartNoThrow("windows-feedback:///?appid=40459File-New-Project.EarTrumpet_1sdd7yawvg6ne!EarTrumpet");
        }

        private void OpenAbout() => ProcessHelper.StartNoThrow("https://github.com/File-New-Project/EarTrumpet");
    }
}
