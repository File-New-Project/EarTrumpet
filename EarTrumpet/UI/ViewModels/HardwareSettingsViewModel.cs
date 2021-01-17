using System;
using System.Collections.ObjectModel;
using EarTrumpet.DataModel.MIDI;

namespace EarTrumpet.UI.ViewModels
{
    class HardwareSettingsViewModel : BindableBase
    {
        private DeviceCollectionViewModel _devices;
        private DeviceViewModel _selectedDevice = null;
        private String _selectedMode;
        private ObservableCollection<String> _applicationIndexesNames = new ObservableCollection<string>();
        
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
        
        public string SelectedMode {
            set
            {
                _selectedMode = value;

                RefreshApps();
            }
        }

        public string SelectedMidi { set; get; }
        public string SelectedCommand { set; get; }
        public string SelectedIndexesApplications { set; get; }

        private void RefreshApps()
        {
            _applicationIndexesNames.Clear();

            // ToDo: Use localization.
            if ("Application Selection" == _selectedMode)
            {
                foreach (var app in _selectedDevice?.Apps)
                {
                    _applicationIndexesNames.Add(app.DisplayName);
                }
            }
            else if ("Indexed" == _selectedMode)
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
            
        public HardwareSettingsViewModel(DeviceCollectionViewModel devices)
        {
            _devices = devices;
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

    }
}