using System;
using System.Windows.Forms;
using System.Windows.Input;
using System.Collections.ObjectModel;
using EarTrumpet.UI.Helpers;
using EarTrumpet.DataModel.Hardware;
using EarTrumpet.Extensions;

namespace EarTrumpet.UI.ViewModels
{
    public class HardwareSettingsViewModel : BindableBase
    {
        public ICommand SaveCommandControlMappingCommand { get; }
        public ICommand SelectControlCommand { get; }
        public string SelectedControl { get; set; }
        public string SelectedIndexesApplications { set; get; }
        public string SelectedDevice
        {
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
        public Boolean ModeSelectionEnabled
        {
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
        public string SelectedMode
        {
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
        public string SelectedCommand
        {
            set
            {
                _selectedCommand = value;

                if (Properties.Resources.AudioDeviceVolumeText == value ||
                    Properties.Resources.AudioDeviceMuteText == value ||
                    Properties.Resources.SetAsDefaultDevice == value)
                {
                    // Audio device specific command selected.
                    // -> Disable Mode and Selection ComboBoxes.

                    ModeSelectionEnabled = false;
                    IndexesApplicationsSelectionEnabled = false;
                }
                else if (Properties.Resources.ApplicationVolumeText == value || 
                         Properties.Resources.ApplicationMuteText == value)
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
        public ObservableCollection<string> AudioDevices
        {
            get
            {
                ObservableCollection<String> availableAudioDevices = new ObservableCollection<string>();
                var devices = _devices.AllDevices;

                availableAudioDevices.Add(Properties.Resources.AllAudioDevicesSelectionText);

                foreach (var device in devices)
                {
                    availableAudioDevices.Add(device.DisplayName);
                }

                return availableAudioDevices;
            }
        }
        public ObservableCollection<string> DeviceTypes
        {
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

                modes.Add(Properties.Resources.IndexedText);
                modes.Add(Properties.Resources.ApplicationSelectionText);

                return modes;
            }
        }
        public ObservableCollection<string> Commands
        {
            get
            {
                ObservableCollection<String> commands = new ObservableCollection<string>();

                commands.Add(Properties.Resources.AudioDeviceVolumeText);
                commands.Add(Properties.Resources.AudioDeviceMuteText);
                commands.Add(Properties.Resources.ApplicationVolumeText);
                commands.Add(Properties.Resources.ApplicationMuteText);
                commands.Add(Properties.Resources.SetAsDefaultDevice);

                return commands;
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

        private DeviceCollectionViewModel _devices;
        private DeviceViewModel _selectedDevice = null;
        private Boolean _modeSelectionEnabled = false;
        private String _selectedMode;
        private String _selectedCommand;
        private string _selectedDeviceType;
        private Boolean _indexesApplicationsSelectionEnabled = false;
        private ObservableCollection<String> _applicationIndexesNames = new ObservableCollection<string>();
        private WindowHolder _ControlWizardWindow = null;
        private HardwareConfiguration _hardwareConfiguration = null;
        private CommandControlMappingElement _commandControlMappingElement = null;
        private EarTrumpetHardwareControlsPageViewModel _hardwareControls = null;

        public HardwareSettingsViewModel(DeviceCollectionViewModel devices, EarTrumpetHardwareControlsPageViewModel earTrumpetHardwareControlsPageViewModel)
        {
            _devices = devices;
            _hardwareControls = earTrumpetHardwareControlsPageViewModel;

            SelectControlCommand = new RelayCommand(SelectControl);
            SaveCommandControlMappingCommand = new RelayCommand(SaveCommandControlMapping);

            switch (_hardwareControls.ItemModificationWay)
            {
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_EMPTY:

                    // Set default command.
                    SelectedCommand = Properties.Resources.AudioDeviceVolumeText;

                    // Set default device type.
                    SelectedDeviceType = "MIDI";

                    // Set default selection.
                    SelectedControl = Properties.Resources.NoControlSelectedMessage;

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

        public void SelectControl()
        {
            _ControlWizardWindow = HardwareManager.Current.GetHardwareWizard(SelectedDeviceType, this,
                _hardwareConfiguration);

            if (_ControlWizardWindow != null)
            {
                _ControlWizardWindow.OpenOrBringToFront();
            }
            else
            {
                MessageBox.Show(Properties.Resources.UnknownDeviceTypeSelectedMessageText, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SaveCommandControlMapping()
        {
            if (_hardwareConfiguration == null ||
                string.IsNullOrEmpty(SelectedDevice) ||
                string.IsNullOrEmpty(SelectedCommand) ||
                (ModeSelectionEnabled && string.IsNullOrEmpty(SelectedMode)) ||
                (IndexesApplicationsSelectionEnabled && string.IsNullOrEmpty(SelectedIndexesApplications)) ||
                string.IsNullOrEmpty(SelectedDeviceType))
            {
                // Do nothing if the settings were not done yet.
                MessageBox.Show(Properties.Resources.IncompleteDeviceConfigurationMessage, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CommandControlMappingElement.Command command = CommandControlMappingElement.Command.None;
            CommandControlMappingElement.Mode mode = CommandControlMappingElement.Mode.None;

            if (SelectedCommand == Properties.Resources.AudioDeviceVolumeText)
            {
                command = CommandControlMappingElement.Command.SystemVolume;
            }
            else if (SelectedCommand == Properties.Resources.AudioDeviceMuteText)
            {
                command = CommandControlMappingElement.Command.SystemMute;
            }
            else if (SelectedCommand == Properties.Resources.ApplicationVolumeText)
            {
                command = CommandControlMappingElement.Command.ApplicationVolume;
            }
            else if (SelectedCommand == Properties.Resources.ApplicationMuteText)
            {
                command = CommandControlMappingElement.Command.ApplicationMute;
            }
            else if (SelectedCommand == Properties.Resources.SetAsDefaultDevice)
            {
                command = CommandControlMappingElement.Command.SetDefaultDevice;
            }

            if (SelectedMode == Properties.Resources.IndexedText)
            {
                mode = CommandControlMappingElement.Mode.Indexed;
            }
            else if (SelectedMode == Properties.Resources.ApplicationSelectionText)
            {
                mode = CommandControlMappingElement.Mode.ApplicationSelection;
            }

            _commandControlMappingElement = new CommandControlMappingElement(_hardwareConfiguration, SelectedDevice,
                command, mode, SelectedIndexesApplications);

            // Notify the hardware controls page about the new assignment.
            _hardwareControls.ControlCommandMappingSelectedCallback(_commandControlMappingElement);
        }

        public void ControlSelectedCallback(HardwareConfiguration hardwareConfiguration)
        {
            _hardwareConfiguration = hardwareConfiguration;

            _ControlWizardWindow.OpenOrClose();

            SelectedControl = _hardwareConfiguration.ToString();
            RaisePropertyChanged("SelectedControl");
        }

        private void RefreshApps()
        {
            _applicationIndexesNames.Clear();

            if (Properties.Resources.ApplicationSelectionText == SelectedMode)
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
            else if (Properties.Resources.IndexedText == SelectedMode)
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
                        SelectedMode = Properties.Resources.IndexedText;
                        break;
                    case CommandControlMappingElement.Mode.ApplicationSelection:
                        SelectedMode = Properties.Resources.ApplicationSelectionText;
                        break;
                }

                SelectedIndexesApplications = data.indexApplicationSelection;
            }

            SelectedDevice = data.audioDevice;

            switch (data.command)
            {
                case CommandControlMappingElement.Command.ApplicationMute:
                    SelectedCommand = Properties.Resources.ApplicationMuteText;
                    FillApplication();
                    break;
                case CommandControlMappingElement.Command.ApplicationVolume:
                    SelectedCommand = Properties.Resources.ApplicationVolumeText;
                    FillApplication();
                    break;
                case CommandControlMappingElement.Command.SystemMute:
                    SelectedCommand = Properties.Resources.AudioDeviceMuteText;
                    break;
                case CommandControlMappingElement.Command.SystemVolume:
                    SelectedCommand = Properties.Resources.AudioDeviceVolumeText;
                    break;
                case CommandControlMappingElement.Command.SetDefaultDevice:
                    SelectedCommand = Properties.Resources.SetAsDefaultDevice;
                    break;
            }

            SelectedDeviceType = HardwareManager.Current.GetConfigType(data);
            SelectedControl = data.hardwareConfiguration.ToString();
            _hardwareConfiguration = data.hardwareConfiguration;
        }
    }
}