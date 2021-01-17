using System;
using System.Collections.ObjectModel;
using EarTrumpet.DataModel.MIDI;
using EarTrumpet.UI.Helpers;
using System.Windows;
using EarTrumpet.UI.Views;
using System.Windows.Input;

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

        private WindowHolder _midiControlWizardWindow;

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

        public string SelectedMidi { set; get; }
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
            
        // Constructor
        public HardwareSettingsViewModel(DeviceCollectionViewModel devices)
        {
            _devices = devices;

            SelectMidiControlCommand = new RelayCommand(SelectMidiControl);
            _midiControlWizardWindow = new WindowHolder(CreateMIDIControlWizardExperience);
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
                var devices = MidiIn.GetAllDevices();
                ObservableCollection<String> availableMidiDevices = new ObservableCollection<string>();
                
                foreach(var dev in devices)
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

       private Window CreateMIDIControlWizardExperience()
        {
            var viewModel = new MIDIControlWizardViewModel(EarTrumpet.Properties.Resources.MIDIControlWizardText);
            return new MIDIControlWizardWindow { DataContext = viewModel};
        }

    }
}