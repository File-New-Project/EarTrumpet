using EarTrumpet.DataModel;
using EarTrumpet.Misc;
using EarTrumpet.Services;
using System.Diagnostics;
using System.Reflection;

namespace EarTrumpet.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        IAudioDeviceManager _manager;

        SettingsService.HotkeyData _hotkey;
        public SettingsService.HotkeyData Hotkey
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

        public string AboutText
        {
            get
            {
                var version = Assembly.GetEntryAssembly().GetName().Version;
                return $"EarTrumpet {version}";
            }
        }


        public SettingsViewModel(IAudioDeviceManager manager)
        {
            _manager = manager;

            Hotkey = SettingsService.Hotkey;
            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
            OpenFeedbackCommand = new RelayCommand(FeedbackService.StartAppServiceAndFeedbackHub);
        }

        private void OpenDiagnostics()
        {
            DiagnosticsService.DumpAndShowData(_manager);
        }

        private void OpenAbout()
        {
            Process.Start("http://github.com/File-New-Project/EarTrumpet");
        }        
    }
}
