using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.Reflection;
using Windows.ApplicationModel;

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

        public string AboutText { get; private set; }

        internal SettingsViewModel()
        {
            Hotkey = SettingsService.Hotkey;
            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
            OpenFeedbackCommand = new RelayCommand(FeedbackService.OpenFeedbackHub);

            string aboutFormat = "EarTrumpet {0}.{1}.{2}.{3}";
            if (App.Current.HasIdentity())
            {
                var ver = Package.Current.Id.Version;
                AboutText = string.Format(aboutFormat, ver.Major, ver.Minor, ver.Build, ver.Revision);
            }
            else
            {
                AboutText = string.Format(aboutFormat, 0, 0, 0, 0);
            }
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
