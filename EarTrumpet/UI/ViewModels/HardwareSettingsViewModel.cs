using System;
using System.Collections.ObjectModel;
using EarTrumpet.DataModel.MIDI;
using EarTrumpet.UI.Helpers;
using System.Windows;
using EarTrumpet.UI.Views;
using System.Windows.Input;
using System.Collections.Generic;

namespace EarTrumpet.UI.ViewModels
{
    class HardwareSettingsViewModel : BindableBase
    {
        private DeviceCollectionViewModel _devices;
        private DeviceViewModel _selectedDevice = null;
        private Boolean _modeSelectionEnabled = false;
        private String _selectedMode;
        private String _selectedCommand;
        private Boolean _indexesApplicationsSelectionEnabled = false;
        private ObservableCollection<String> _applicationIndexesNames = new ObservableCollection<string>();
        private List<MidiInDevice> _availableMidiInDevices;

        private WindowHolder _midiControlWizardWindow;
        private MidiControlConfiguration _midiControlConfiguration = null;
        private CommandControlMappingElement _commandControlMappingElement = null;

        private EarTrumpetHardwareControlsPageViewModel _hardwareControls = null;

        public ICommand SaveCommandControlMappingCommand { get; }

        public MidiInDevice SelectedMidiInDevice { get; set; }

        public ICommand SelectMidiControlCommand { get; }

        public string SelectedDevice {
            set
            {
                foreach (var dev in _devices.AllDevices)
                {
                    if (dev.DisplayName == value)
                    {
                        _selectedDevice = dev;
                    }
                }

                RefreshApps();
            }

            get
            {
                if (_selectedDevice != null)
                {
                    return _selectedDevice.DisplayName;
                }

                return "";
            }
        }
        
        public Boolean ModeSelectionEnabled {
            set
            {
                _modeSelectionEnabled = value;
                RaisePropertyChanged("ModeSelectionEnabled");
            }

            get
            {
                return _modeSelectionEnabled;
            }
        }

        public string SelectedMode {
            set
            {
                _selectedMode = value;
                RefreshApps();
            }
            get
            {
                return _selectedMode;
            }
        }

        public string SelectedMidi {
            set
            {
                bool deviceFound = false;

                foreach(var dev in _availableMidiInDevices)
                {
                    if(dev.Name == value)
                    {
                        SelectedMidiInDevice = dev;
                        deviceFound = true;
                    }
                }

                if(!deviceFound)
                {
                    // ToDo: Error handling. Should never happen.
                }

            }

            get
            {
                if (SelectedMidiInDevice != null)
                {
                    return SelectedMidiInDevice.Name;
                }

                return "";
            }
        }
        public string SelectedCommand {
            set
            {
                _selectedCommand = value;

                // ToDo: Use localization.
                if("System Volume" == value || "System Mute" == value)
                {
                    // System specific command selected.
                    // -> Disable Mode and Selection ComboBoxes.

                    ModeSelectionEnabled = false;
                    IndexesApplicationsSelectionEnabled = false;
                }
                else if("Application Volume" == value || "Application Mute" == value)
                {
                    // Application specific command selected.
                    // -> Enable Mode and Selection ComboBoxes.

                    ModeSelectionEnabled = true;
                    IndexesApplicationsSelectionEnabled = true;
                }
                else
                {
                    // Invalid selection. Do nothing.
                }
            }

            get
            {
                return _selectedCommand;
            }
        }
        public Boolean IndexesApplicationsSelectionEnabled
        {
            set
            {
                _indexesApplicationsSelectionEnabled = value;
                RaisePropertyChanged("IndexesApplicationsSelectionEnabled");
            }

            get
            {
                return _indexesApplicationsSelectionEnabled;
            }
        }
        public string SelectedIndexesApplications { set; get; }

        private void RefreshApps()
        {
            _applicationIndexesNames.Clear();

            // ToDo: Use localization.
            if ("Application Selection" == SelectedMode)
            {
                foreach (var app in _selectedDevice?.Apps)
                {
                    _applicationIndexesNames.Add(app.DisplayName);
                }
            }
            else if ("Indexed" == SelectedMode)
            {
                // We do not expect more than 20 applications to be addressed.
                for(var i = 0; i < 20; i++)
                {
                    _applicationIndexesNames.Add(i.ToString());
                }
            }
            else
            {
                // Invalid mode. Do nothing.
            }
        }

        private void FillForm(CommandControlMappingElement data)
        {
            void FillApplication()
            {
                switch (data.mode)
                {
                    case CommandControlMappingElement.Mode.Indexed:
                        SelectedMode = "Indexed";
                        break;
                    case CommandControlMappingElement.Mode.ApplicationSelection:
                        SelectedMode = "Application Selection";
                        break;
                }

                SelectedIndexesApplications = data.indexApplicationSelection;
            }
            
            SelectedDevice = data.audioDevice;
                    
            switch (data.command)
            {
                case CommandControlMappingElement.Command.ApplicationMute:
                    SelectedCommand = "Application Mute";
                    FillApplication();
                    break;
                case CommandControlMappingElement.Command.ApplicationVolume:
                    SelectedCommand = "Application Volume";
                    FillApplication();
                    break;
                case CommandControlMappingElement.Command.SystemMute:
                    SelectedCommand = "System Mute";
                    break;
                case CommandControlMappingElement.Command.SystemVolume:
                    SelectedCommand = "System Volume";
                    break;
            }

            SelectedMidi = data.midiDevice;

            _midiControlConfiguration = data.midiControlConfiguration;
        }

        // Constructor
        public HardwareSettingsViewModel(DeviceCollectionViewModel devices, EarTrumpetHardwareControlsPageViewModel earTrumpetHardwareControlsPageViewModel)
        {
            _devices = devices;
            _hardwareControls = earTrumpetHardwareControlsPageViewModel;

            SelectMidiControlCommand = new RelayCommand(SelectMidiControl);
            SaveCommandControlMappingCommand = new RelayCommand(SaveCommandControlMapping);

            _midiControlWizardWindow = new WindowHolder(CreateMIDIControlWizardExperience);
            
            _availableMidiInDevices = MidiIn.GetAllDevices();
            
            switch(_hardwareControls.ItemModificationWay)
            {
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.EDIT_EXISTING:
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_FROM_EXISTING:
                    var selectedMappingElement = MidiAppBinding.Current.GetCommandControlMappings()[_hardwareControls.SelectedIndex];

                    FillForm(selectedMappingElement);
                    break;

                default:
                    // Do not fill widgets.
                    break;
            }
            
        }

        public ObservableCollection<string> AudioDevices
        {
            get
            {
                ObservableCollection<String> availableAudioDevices = new ObservableCollection<string>();
                var devices = _devices.AllDevices;

                foreach (var device in devices)
                {
                    availableAudioDevices.Add(device.DisplayName);
                }

                return availableAudioDevices;
            }
        }
        public ObservableCollection<string> MidiDevices
        {
            get
            {
                _availableMidiInDevices = MidiIn.GetAllDevices();
                ObservableCollection<String> availableMidiDevices = new ObservableCollection<string>();
                
                foreach(var dev in _availableMidiInDevices)
                {
                    availableMidiDevices.Add(dev.Name);
                }

                return availableMidiDevices;
            }
        }
        
        
        public ObservableCollection<string> ApplicationIndexesNames
        {
            get
            {
                return _applicationIndexesNames;
            }
        }

        public ObservableCollection<string> Modes
        {
            get
            {
                // Two modes are supported: "Indexed" and "Application Selection"
                // In "Indexed" mode, the user can assign an application index to a control.
                // In "Application Selection" mode, the user can select from a list of running applications.

                ObservableCollection<String> modes = new ObservableCollection<string>();

                // ToDo: Use localization.
                modes.Add("Indexed");
                modes.Add("Application Selection");

                return modes;
            }
        }

        public ObservableCollection<string> Commands
        {
            get
            {
                ObservableCollection<String> commands = new ObservableCollection<string>();

                // ToDo: Use localization.
                commands.Add("System Volume");
                commands.Add("System Mute");
                commands.Add("Application Volume");
                commands.Add("Application Mute");

                return commands;
            }
        }

        public void SelectMidiControl()
        {
            _midiControlWizardWindow.OpenOrBringToFront();
        }

        public void SaveCommandControlMapping()
        {
            if (_midiControlConfiguration == null)
            {
                // Do nothing if the midi settings were not done yet
                // Todo maybe add an error message
                return;
            }
            CommandControlMappingElement.Command command = CommandControlMappingElement.Command.None;
            CommandControlMappingElement.Mode mode = CommandControlMappingElement.Mode.None;

            switch (SelectedCommand)
            {
                case "System Volume":
                    command = CommandControlMappingElement.Command.SystemVolume;
                    break;
                case "System Mute":
                    command = CommandControlMappingElement.Command.SystemMute;
                    break;
                case "Application Volume":
                    command = CommandControlMappingElement.Command.ApplicationVolume;
                    break;
                case "Application Mute":
                    command = CommandControlMappingElement.Command.ApplicationMute;
                    break;
            }

            switch (SelectedMode)
            {
                case "Indexed":
                    mode = CommandControlMappingElement.Mode.Indexed;
                    break;
                case "Application Selection":
                    mode = CommandControlMappingElement.Mode.ApplicationSelection;
                    break;
            }
            
            _commandControlMappingElement = new CommandControlMappingElement(_midiControlConfiguration, SelectedDevice, command, mode, SelectedIndexesApplications, SelectedMidi);
            // Notify the hardware controls page about the new assignment.
            _hardwareControls.ControlCommandMappingSelectedCallback(_commandControlMappingElement);
        }

        public void MidiControlSelectedCallback(MidiControlConfiguration midiControlConfiguration)
        {
            _midiControlConfiguration = midiControlConfiguration;

            _midiControlWizardWindow.OpenOrClose();
        }

       private Window CreateMIDIControlWizardExperience()
       {
           MIDIControlWizardViewModel viewModel = null;
           
            switch (_hardwareControls.ItemModificationWay)
            {
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_EMPTY:
                    viewModel = new MIDIControlWizardViewModel(EarTrumpet.Properties.Resources.MIDIControlWizardText, this);
                    break;
                
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_FROM_EXISTING:
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.EDIT_EXISTING:
                    var config = MidiAppBinding.Current.GetCommandControlMappings()[_hardwareControls.SelectedIndex]
                        .midiControlConfiguration;
                    viewModel = new MIDIControlWizardViewModel(EarTrumpet.Properties.Resources.MIDIControlWizardText,
                        this, config);
                    break;
            }
            
            return new MIDIControlWizardWindow { DataContext = viewModel};
        }

    }
}