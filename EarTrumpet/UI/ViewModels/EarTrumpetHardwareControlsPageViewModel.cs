using EarTrumpet.UI.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using EarTrumpet.UI.Views;
using EarTrumpet.DataModel.MIDI;
using System.Collections.Generic;

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

        ObservableCollection<String> _commandControlList = new ObservableCollection<string>();
        public ICommand SelectedItemChangedCommand { get; set; }

        public EarTrumpetHardwareControlsPageViewModel(AppSettings settings, DeviceCollectionViewModel devices) : base(null)
        {
            _settings = settings;
            _devices = devices;
            Glyph = "\xF8A6";
            Title = Properties.Resources.HardwareControlsTitle;

            AddMidiControlCommand = new RelayCommand(AddMidiControl);
            
            _hardwareSettingsWindow = new WindowHolder(CreateHardwareSettingsExperience);

            UpdateCommandControlsList(MidiAppBinding.Current.GetCommandControlMappings());

            // The command controls list should have no item selected on startup.
            SelectedIndex = -1;

            SelectedItemChangedCommand = new RelayCommand(SelectedItemChanged);
        }

        public int SelectedIndex { get; set; }

        private Window CreateHardwareSettingsExperience()
        {
            var viewModel = new HardwareSettingsViewModel(_devices, this);
            return new HardwareSettingsWindow {DataContext = viewModel};
        }
        
        public ObservableCollection<string> HardwareControls
        {
            get
            {
                return _commandControlList;
            }

            set
            {
                _commandControlList = value;

                RaisePropertyChanged("HardwareControls");
            }
        }

        private void AddMidiControl()
        {
            _hardwareSettingsWindow.OpenOrBringToFront();
        }

        public void ControlCommandMappingSelectedCallback(CommandControlMappingElement commandControlMappingElement)
        {
            MidiAppBinding.Current.AddCommand(commandControlMappingElement);

            UpdateCommandControlsList(MidiAppBinding.Current.GetCommandControlMappings());

            _hardwareSettingsWindow.OpenOrClose();
        }

        private void UpdateCommandControlsList(List<CommandControlMappingElement> commandControlsList)
        {
            ObservableCollection<String> commandControlsStringList = new ObservableCollection<string>();

            foreach (var item in commandControlsList)
            {
                string commandControlsString = 
                    "Audio Device=" + item.audioDevice + 
                    ", Command=" + item.command + 
                    ", Mode=" + item.mode + 
                    ", Selection=" + item.indexApplicationSelection + 
                    ", MIDI Device=" + item.midiDevice + 
                    ", MIDI Channel=" + item.midiControlConfiguration.Channel + 
                    ", MIDI Controller=" + item.midiControlConfiguration.Controller + 
                    ", MIDI Min Value=" + item.midiControlConfiguration.MinValue + 
                    ", MIDI Max Value=" + item.midiControlConfiguration.MaxValue + 
                    ", MIDI Value Scaling=" + item.midiControlConfiguration.ScalingValue;

                commandControlsStringList.Add(commandControlsString);
            }

            HardwareControls = commandControlsStringList;
        }

        public void SelectedItemChanged()
        {
            var selectedIndex = SelectedIndex;

            if (selectedIndex < 0)
            {
                return;
            }

            const string selectedDesignator = "\u27BD";
            ObservableCollection<string> hardwareControls = HardwareControls;

            // Remove "selected" designator from previously selected item.
            for (var i = 0; i < hardwareControls.Count; i++)
            {
                if (hardwareControls[i].StartsWith(selectedDesignator))
                {
                    hardwareControls[i] = hardwareControls[i].Remove(0, selectedDesignator.Length);
                }
            }

            // Add "selected" designator to selected item.
            hardwareControls[selectedIndex] = selectedDesignator + hardwareControls[selectedIndex];

            HardwareControls = hardwareControls;
        }
    }
}
