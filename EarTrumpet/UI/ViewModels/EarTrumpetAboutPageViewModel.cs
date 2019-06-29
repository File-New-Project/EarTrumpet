using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class EarTrumpetAboutPageViewModel : SettingsPageViewModel
    {
        public ICommand OpenDiagnosticsCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand OpenFeedbackCommand { get; }
        public ICommand OpenPrivacyPolicyCommand { get; }
        public string AboutText { get; }

        public bool IsTelemetryEnabled
        {
            get => _settings.IsTelemetryEnabled;
            set => _settings.IsTelemetryEnabled = value;
        }

        private readonly Action _openDiagnostics;
        private readonly AppSettings _settings;

        public EarTrumpetAboutPageViewModel(Action openDiagnostics, AppSettings settings) : base(null)
        {
            _settings = settings;
            _openDiagnostics = openDiagnostics;
            Glyph = "\xE946";
            Title = Properties.Resources.AboutTitle;
            AboutText = $"EarTrumpet {App.PackageVersion}";

            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
            OpenFeedbackCommand = new RelayCommand(OpenFeedbackHub);
            OpenPrivacyPolicyCommand = new RelayCommand(OpenPrivacyPolicy);
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

        private void OpenFeedbackHub() => ProcessHelper.StartNoThrow("windows-feedback:///?appid=40459File-New-Project.EarTrumpet_1sdd7yawvg6ne!EarTrumpet");
        private void OpenAbout() => ProcessHelper.StartNoThrow("https://github.com/File-New-Project/EarTrumpet");
        private void OpenPrivacyPolicy() => ProcessHelper.StartNoThrow("https://github.com/File-New-Project/EarTrumpet/blob/master/PRIVACY.md");
    }
}
