using System;
using EarTrumpet.UI.Helpers;
using System.Collections.ObjectModel;

namespace EarTrumpet.UI.ViewModels
{
    class HardwareSettingsViewModel : BindableBase
    {
        public string Title { get; private set; }
        
        public HardwareSettingsViewModel(string title)
        {
            Title = title;
        }

        public ObservableCollection<string> AudioDevices
        {
            get
            {
                ObservableCollection<String> availableAudioDevices = new ObservableCollection<string>();

                // ToDo: Scan actual audio devices of the system.
                for (int i = 0; i < 2; i++)
                {
                    String str = "Sample Device " + i.ToString();
                    availableAudioDevices.Add(str);
                }

                return availableAudioDevices;
            }
        }
        public ObservableCollection<string> MidiDevices
        {
            get
            {
                ObservableCollection<String> availableMidiDevices = new ObservableCollection<string>();

                // ToDo: Scan actual MIDI devices connected to the system.
                for(int i = 0; i < 100; i++)
                {
                    String str = "Sample Device " + i.ToString();
                    availableMidiDevices.Add(str);
                }

                return availableMidiDevices;
            }
        }

        public ObservableCollection<string> ApplicationIndexesNames
        {
            get
            {

                ObservableCollection<String> applicationIndexesNames = new ObservableCollection<string>();

                // ToDo: Get currently running applications.
                applicationIndexesNames.Add("Sample Application 1");
                applicationIndexesNames.Add("Sample Application 2");

                // We expect not more than 20 applications to be running.
                for (int i = 0; i < 20; i++)
                {
                    applicationIndexesNames.Add(i.ToString());
                }

                return applicationIndexesNames;
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