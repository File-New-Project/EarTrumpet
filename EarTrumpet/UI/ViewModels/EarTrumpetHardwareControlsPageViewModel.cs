using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace EarTrumpet.UI.ViewModels
{
    class EarTrumpetHardwareControlsPageViewModel : SettingsPageViewModel
    {
        public ICommand AddMidiControlCommand { get; }

        public bool IsTelemetryEnabled
        {
            get => _settings.IsTelemetryEnabled;
            set => _settings.IsTelemetryEnabled = value;
        }

        private readonly AppSettings _settings;

        public EarTrumpetHardwareControlsPageViewModel(AppSettings settings) : base(null)
        {
            _settings = settings;
            Glyph = "\xF8A6";
            Title = Properties.Resources.HardwareControlsTitle;

            AddMidiControlCommand = new RelayCommand(AddMidiControl);
        }
        public ObservableCollection<string> HardwareControls
        {
            get
            {
                ObservableCollection<String> a = new ObservableCollection<string>();
                a.Add("Sample 1");
                a.Add("Sample 2");
                return a;
            }
        }

        // ToDo: Open window for MIDI control selection.
        private void AddMidiControl() => ProcessHelper.StartNoThrow("https://github.com/File-New-Project/EarTrumpet");
    }
}
