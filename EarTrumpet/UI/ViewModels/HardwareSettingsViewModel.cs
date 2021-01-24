using System;
using System.Collections.ObjectModel;
using EarTrumpet.DataModel.MIDI;
using EarTrumpet.UI.Helpers;
using System.Windows;
using EarTrumpet.UI.Views;
using System.Windows.Input;
using EarTrumpet.DataModel.Hardware;
using EarTrumpet.Extensions;
using EarTrumpet.DataModel.Deej;
using System.Windows.Forms;

namespace EarTrumpet.UI.ViewModels
{
    class HardwareSettingsViewModel : BindableBase
    {
        private DeviceCollectionViewModel _devices;
        private DeviceViewModel _selectedDevice = null;
        private Boolean _modeSelectionEnabled = false;
        private String _selectedMode;
        private String _selectedCommand;
        private string _selectedDeviceType;
        private Boolean _indexesApplicationsSelectionEnabled = false;
        private ObservableCollection<String> _applicationIndexesNames = new ObservableCollection<string>();

        private WindowHolder _midiControlWizardWindow;
        private WindowHolder _deejControlWizardWindow;

        private MidiControlConfiguration _midiControlConfiguration = null;
        private DeejConfiguration _deejConfiguration = null;

        private CommandControlMappingElement _commandControlMappingElement = null;

        private EarTrumpetHardwareControlsPageViewModel _hardwareControls = null;

        public ICommand SaveCommandControlMappingCommand { get; }

        public ICommand SelectControlCommand { get; }

        public string SelectedDevice {
            set
            {
                if (Properties.Resources.AllAudioDevicesSelectionText == value)
                {
                    _selectedDevice = null;
                }
                else
                {
                    foreach (var dev in _devices.AllDevices)
                    {
                        if (dev.DisplayName == value)
                        {
                            _selectedDevice = dev;
                        }
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

                return Properties.Resources.AllAudioDevicesSelectionText;
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

        public string SelectedCommand {
            set
            {
                _selectedCommand = value;

                if(Properties.Resources.AudioDeviceVolume == value || Properties.Resources.AudioDeviceMute == value)
                {
                    // Audio device specific command selected.
                    // -> Disable Mode and Selection ComboBoxes.

                    ModeSelectionEnabled = false;
                    IndexesApplicationsSelectionEnabled = false;
                }
                else if(Properties.Resources.ApplicationVolume == value || Properties.Resources.ApplicationMute == value)
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

            if (Properties.Resources.ApplicationSelection == SelectedMode)
            {
                if (Properties.Resources.AllAudioDevicesSelectionText == SelectedDevice)
                {
                    foreach (var dev in _devices.AllDevices)
                    {
                        foreach(var app in dev.Apps)
                        {
                            if(!_applicationIndexesNames.Contains(app.DisplayName))
                            {
                                _applicationIndexesNames.Add(app.DisplayName);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var app in _selectedDevice?.Apps)
                    {
                        _applicationIndexesNames.Add(app.DisplayName);
                    }
                }
            }
            else if (Properties.Resources.IndexedSelection == SelectedMode)
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
                        SelectedMode = Properties.Resources.IndexedSelection;
                        break;
                    case CommandControlMappingElement.Mode.ApplicationSelection:
                        SelectedMode = Properties.Resources.ApplicationSelection;
                        break;
                }

                SelectedIndexesApplications = data.indexApplicationSelection;
            }

            SelectedDevice = data.audioDevice;

            switch (data.command)
            {
                case CommandControlMappingElement.Command.ApplicationMute:
                    SelectedCommand = Properties.Resources.ApplicationMute;
                    FillApplication();
                    break;
                case CommandControlMappingElement.Command.ApplicationVolume:
                    SelectedCommand = Properties.Resources.ApplicationVolume;
                    FillApplication();
                    break;
                case CommandControlMappingElement.Command.SystemMute:
                    SelectedCommand = Properties.Resources.AudioDeviceMute;
                    break;
                case CommandControlMappingElement.Command.SystemVolume:
                    SelectedCommand = Properties.Resources.AudioDeviceVolume;
                    break;
            }

            SelectedDeviceType = HardwareManager.Current.GetConfigType(data);

            switch (SelectedDeviceType)
            {
                case "MIDI":
                    _midiControlConfiguration = (MidiControlConfiguration)data.hardwareConfiguration;
                    break;
                case "deej":
                    _deejConfiguration = (DeejConfiguration)data.hardwareConfiguration;
                    break;
                default:
                    System.Windows.Forms.MessageBox.Show(Properties.Resources.UnknownDeviceTypeSelectedMessageText, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
    }

        // Constructor
        public HardwareSettingsViewModel(DeviceCollectionViewModel devices, EarTrumpetHardwareControlsPageViewModel earTrumpetHardwareControlsPageViewModel)
        {
            _devices = devices;
            _hardwareControls = earTrumpetHardwareControlsPageViewModel;

            SelectControlCommand = new RelayCommand(SelectControl);
            SaveCommandControlMappingCommand = new RelayCommand(SaveCommandControlMapping);

            _midiControlWizardWindow = new WindowHolder(CreateMIDIControlWizardExperience);
            _deejControlWizardWindow = new WindowHolder(CreateDeejControlWizardExperience);

            switch(_hardwareControls.ItemModificationWay)
            {
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_EMPTY:
                    
                    // Set default command.
                    SelectedCommand = Properties.Resources.AudioDeviceVolume;

                    // Set default device type.
                    SelectedDeviceType = "MIDI";

                    break;
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.EDIT_EXISTING:
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_FROM_EXISTING:
                    var selectedMappingElement = HardwareManager.Current.GetCommandControlMappings()[_hardwareControls.SelectedIndex];

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

                availableAudioDevices.Add("*All Devices*");

                foreach (var device in devices)
                {
                    availableAudioDevices.Add(device.DisplayName);
                }

                return availableAudioDevices;
            }
        }

        public ObservableCollection<string> DeviceTypes {
            get
            {
                ObservableCollection<String> deviceTypes = new ObservableCollection<string>();
                deviceTypes.AddRange(HardwareManager.Current.GetDeviceTypes());

                return deviceTypes;
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

                modes.Add(Properties.Resources.IndexedSelection);
                modes.Add(Properties.Resources.ApplicationSelection);

                return modes;
            }
        }

        public ObservableCollection<string> Commands
        {
            get
            {
                ObservableCollection<String> commands = new ObservableCollection<string>();

                commands.Add(Properties.Resources.AudioDeviceVolume);
                commands.Add(Properties.Resources.AudioDeviceMute);
                commands.Add(Properties.Resources.ApplicationVolume);
                commands.Add(Properties.Resources.ApplicationMute);

                return commands;
            }
        }

        public void SelectControl()
        {
            switch(SelectedDeviceType)
            {
                case "MIDI":
                    _midiControlWizardWindow.OpenOrBringToFront();
                    break;
                case "deej":
                    _deejControlWizardWindow.OpenOrBringToFront();
                    break;
                default:
                    // Unknown device type, cannot open control wizard.
                    System.Windows.Forms.MessageBox.Show(Properties.Resources.UnknownDeviceTypeSelectedMessageText, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }

        public string SelectedDeviceType 
        {
            get 
            {
                return _selectedDeviceType;
            }

            set
            {
                _selectedDeviceType = value;
                RaisePropertyChanged("SelectedDeviceType");
            }
        }

        public void SaveCommandControlMapping()
        {
            if ((SelectedDeviceType == "MIDI" && _midiControlConfiguration == null) ||
                SelectedDevice == "deej" && _deejConfiguration == null)
            {
                // Do nothing if the settings were not done yet.
                System.Windows.Forms.MessageBox.Show(Properties.Resources.IncompleteDeviceConfigurationText, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CommandControlMappingElement.Command command = CommandControlMappingElement.Command.None;
            CommandControlMappingElement.Mode mode = CommandControlMappingElement.Mode.None;

            if (SelectedCommand == Properties.Resources.AudioDeviceVolume)
            {
                command = CommandControlMappingElement.Command.SystemVolume;
            } else if (SelectedCommand == Properties.Resources.AudioDeviceMute)
            {
                command = CommandControlMappingElement.Command.SystemMute;
            } else if (SelectedCommand == Properties.Resources.ApplicationVolume)
            {
                command = CommandControlMappingElement.Command.ApplicationVolume;
            }
            else if (SelectedCommand == Properties.Resources.ApplicationMute)
            {
                command = CommandControlMappingElement.Command.ApplicationMute;
            }

            if (SelectedMode == Properties.Resources.IndexedSelection)
            {
                mode = CommandControlMappingElement.Mode.Indexed;
            } else if (SelectedMode == Properties.Resources.ApplicationSelection)
            {
                mode = CommandControlMappingElement.Mode.ApplicationSelection;
            }

            switch (SelectedDeviceType)
            {
                case "MIDI":
                    _commandControlMappingElement = new CommandControlMappingElement(_midiControlConfiguration, SelectedDevice, command, mode, SelectedIndexesApplications);
                    break;
                case "deej":
                    _commandControlMappingElement = new CommandControlMappingElement(_deejConfiguration, SelectedDevice, command, mode, SelectedIndexesApplications);
                    break;
                default:
                    // Do not save when selected device type is unknown.
                    System.Windows.Forms.MessageBox.Show(Properties.Resources.UnknownDeviceTypeSelectedMessageText, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
            }

            // Notify the hardware controls page about the new assignment.
            _hardwareControls.ControlCommandMappingSelectedCallback(_commandControlMappingElement);
        }

        public void MidiControlSelectedCallback(MidiControlConfiguration midiControlConfiguration)
        {
            _midiControlConfiguration = midiControlConfiguration;

            _midiControlWizardWindow.OpenOrClose();
        }

        public void DeejControlSelectedCallback(DeejConfiguration deejConfiguration)
        {
            _deejConfiguration = deejConfiguration;

            _deejControlWizardWindow.OpenOrClose();
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
                    var config = HardwareManager.Current.GetCommandControlMappings()[_hardwareControls.SelectedIndex]
                        .hardwareConfiguration;
                    if (config is MidiControlConfiguration)
                    {
                        viewModel = new MIDIControlWizardViewModel(Properties.Resources.MIDIControlWizardText,
                            this, (MidiControlConfiguration)config);
                    }
                    else
                    {
                        viewModel = new MIDIControlWizardViewModel(Properties.Resources.MIDIControlWizardText, this);
                    }
                    
                    break;
            }
            
            return new MIDIControlWizardWindow { DataContext = viewModel};
        }

        private Window CreateDeejControlWizardExperience()
        {
            DeejControlWizardViewModel viewModel = null;

            switch (_hardwareControls.ItemModificationWay)
            {
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_EMPTY:
                    viewModel = new DeejControlWizardViewModel(Properties.Resources.DeejControlWizardText, this);
                    break;

                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_FROM_EXISTING:
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.EDIT_EXISTING:
                    var config = HardwareManager.Current.GetCommandControlMappings()[_hardwareControls.SelectedIndex]
                        .hardwareConfiguration;
                    if (config is DeejConfiguration)
                    {
                        viewModel = new DeejControlWizardViewModel(Properties.Resources.DeejControlWizardText,
                            this, (DeejConfiguration)config);
                    }
                    else
                    {
                        viewModel = new DeejControlWizardViewModel(Properties.Resources.DeejControlWizardText, this);
                    }
                    
                    break;
            }

            return new DeejControlWizardWindow { DataContext = viewModel };
        }

    }
}