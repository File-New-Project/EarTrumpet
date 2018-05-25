using EarTrumpet.DataModel;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.Reflection;

namespace EarTrumpet.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        SettingsService.HotkeyData _hotkey;
        internal SettingsService.HotkeyData Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = value;
                SettingsService.Hotkey = _hotkey;
                RaisePropertyChanged(nameof(Hotkey));
                RaisePropertyChanged(nameof(HotkeyText));
            }
        }

        public string HotkeyText => _hotkey.ToString();
        public RelayCommand OpenDiagnosticsCommand { get; }
        public RelayCommand OpenAboutCommand { get; }
        public RelayCommand OpenFeedbackCommand { get; }

        public bool UseLegacyIcon
        {
            get => SettingsService.UseLegacyIcon;
            set => SettingsService.UseLegacyIcon = value;
        }

        public string AboutText => $"EarTrumpet {Assembly.GetEntryAssembly().GetName().Version}";

        internal SettingsViewModel()
        {
            Hotkey = SettingsService.Hotkey;
            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
            OpenFeedbackCommand = new RelayCommand(FeedbackService.StartAppServiceAndFeedbackHub);
        }

        private void OpenDiagnostics()
        {
            DiagnosticsService.DumpAndShowData();
        }

        private void OpenAbout()
        {
            Process.Start("http://github.com/File-New-Project/EarTrumpet");
        }        
    }
}
