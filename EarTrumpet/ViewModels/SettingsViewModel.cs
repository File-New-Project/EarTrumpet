using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
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
                RaisePropertyChanged(nameof(Hotkey));
                RaisePropertyChanged(nameof(HotkeyText));
            }
        }

        public string HotkeyText => _hotkey.ToString();
        public RelayCommand OpenDiagnosticsCommand { get; }
        public RelayCommand OpenAboutCommand { get; }

        public string AboutText
        {
            get
            {
                var aboutString = Properties.Resources.ContextMenuAboutTitle;
                var version = Assembly.GetEntryAssembly().GetName().Version;
                return $"{aboutString} EarTrumpet {version}";
            }
        }


        public SettingsViewModel(IAudioDeviceManager manager)
        {
            _manager = manager;

            Hotkey = SettingsService.Hotkey;
            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
        }

        public void Save()
        {
            SettingsService.Hotkey = Hotkey;
            HotkeyService.Register(Hotkey.Modifiers, Hotkey.Key);
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
