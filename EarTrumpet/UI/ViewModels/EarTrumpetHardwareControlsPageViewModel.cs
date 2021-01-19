using EarTrumpet.UI.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using EarTrumpet.UI.Views;
using EarTrumpet.DataModel.MIDI;

namespace EarTrumpet.UI.ViewModels
{
    class EarTrumpetHardwareControlsPageViewModel : SettingsPageViewModel
    {
        public ICommand AddMidiControlCommand { get; }

        private WindowHolder _hardwareSettingsWindow;
        
        public bool IsTelemetryEnabled
        {
            get => _settings.IsTelemetryEnabled;
            set => _settings.IsTelemetryEnabled = value;
        }

        private readonly AppSettings _settings;
        private DeviceCollectionViewModel _devices;
        
        public EarTrumpetHardwareControlsPageViewModel(AppSettings settings, DeviceCollectionViewModel devices) : base(null)
        {
            _settings = settings;
            _devices = devices;
            Glyph = "\xF8A6";
            Title = Properties.Resources.HardwareControlsTitle;

            AddMidiControlCommand = new RelayCommand(AddMidiControl);
            
            _hardwareSettingsWindow = new WindowHolder(CreateHardwareSettingsExperience);
        }
        
        private Window CreateHardwareSettingsExperience()
        {
            var viewModel = new HardwareSettingsViewModel(_devices, this);
            return new HardwareSettingsWindow {DataContext = viewModel};
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

        private void AddMidiControl()
        {
            _hardwareSettingsWindow.OpenOrBringToFront();
        }

        public void ControlCommandMappingSelectedCallback(CommandControlMappingElement commandControlMappingElement)
        {
            // ToDo: Register mapping element, close hardware settings window.
            MidiAppBinding.Current.AddCommand(commandControlMappingElement);
        }
    }
}
