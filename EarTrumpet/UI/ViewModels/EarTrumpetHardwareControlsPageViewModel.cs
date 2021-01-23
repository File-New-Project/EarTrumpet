using EarTrumpet.UI.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using EarTrumpet.UI.Views;
using EarTrumpet.DataModel.MIDI;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EarTrumpet.UI.ViewModels
{
    class EarTrumpetHardwareControlsPageViewModel : SettingsPageViewModel
    {
        public ICommand AddMidiControlCommand { get; }
        public ICommand EditMidiControlCommand { get; }
        public ICommand DeleteMidiControlCommand { get; }
        public ICommand AddFromExistingMidiControlCommand { get; }

        private WindowHolder _hardwareSettingsWindow;
        
        public bool IsTelemetryEnabled
        {
            get => _settings.IsTelemetryEnabled;
            set => _settings.IsTelemetryEnabled = value;
        }

        private readonly AppSettings _settings;
        private DeviceCollectionViewModel _devices;

        ObservableCollection<String> _commandControlList = new ObservableCollection<string>();

        public enum ItemModificationWays
        {
            NEW_EMPTY,
            NEW_FROM_EXISTING,
            EDIT_EXISTING
        }

        public ItemModificationWays ItemModificationWay { get; set; }

        public EarTrumpetHardwareControlsPageViewModel(AppSettings settings, DeviceCollectionViewModel devices) : base(null)
        {
            _settings = settings;
            _devices = devices;
            Glyph = "\xF8A6";
            Title = Properties.Resources.HardwareControlsTitle;

            AddMidiControlCommand = new RelayCommand(AddMidiControl);
            EditMidiControlCommand = new RelayCommand(EditMidiControl);
            DeleteMidiControlCommand = new RelayCommand(DeleteMidiControl);
            AddFromExistingMidiControlCommand = new RelayCommand(AddFromExistingMidiControl);

            _hardwareSettingsWindow = new WindowHolder(CreateHardwareSettingsExperience);

            UpdateCommandControlsList();

            // The command controls list should have no item selected on startup.
            SelectedIndex = -1;
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
            ItemModificationWay = ItemModificationWays.NEW_EMPTY;
            _hardwareSettingsWindow.OpenOrBringToFront();
        }
        private void EditMidiControl()
        {
            var selectedIndex = SelectedIndex;

            if (selectedIndex < 0)
            {
                // ToDo: Use localization.
                System.Windows.Forms.MessageBox.Show("No control selected!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ItemModificationWay = ItemModificationWays.EDIT_EXISTING;
            _hardwareSettingsWindow.OpenOrBringToFront();
        }

        private void AddFromExistingMidiControl()
        {
            var selectedIndex = SelectedIndex;

            if (selectedIndex < 0)
            {
                // ToDo: Use localization.
                System.Windows.Forms.MessageBox.Show("No control selected!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ItemModificationWay = ItemModificationWays.NEW_FROM_EXISTING;
            _hardwareSettingsWindow.OpenOrBringToFront();
        }
        
        private void DeleteMidiControl()
        {
            var selectedIndex = SelectedIndex;

            if(selectedIndex < 0)
            {
                // ToDo: Use localization.
                System.Windows.Forms.MessageBox.Show("No control selected!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MidiAppBinding.Current.RemoveCommandIndex(selectedIndex);
            UpdateCommandControlsList();
        }

        public void ControlCommandMappingSelectedCallback(CommandControlMappingElement commandControlMappingElement)
        {
            switch (ItemModificationWay)
            {
                case ItemModificationWays.NEW_EMPTY:
                case ItemModificationWays.NEW_FROM_EXISTING:
                    MidiAppBinding.Current.AddCommand(commandControlMappingElement);
                    break;
                case ItemModificationWays.EDIT_EXISTING:
                    // Notify the hardware controls page about the new assignment.
                    MidiAppBinding.Current.ModifyCommandIndex(SelectedIndex, commandControlMappingElement);
                    break;
            }
            
            UpdateCommandControlsList();

            _hardwareSettingsWindow.OpenOrClose();
        }

        private void UpdateCommandControlsList()
        {
            var commandControlsList = MidiAppBinding.Current.GetCommandControlMappings();

            ObservableCollection<String> commandControlsStringList = new ObservableCollection<string>();

            foreach (var item in commandControlsList)
            {
                string commandControlsString = 
                    "Audio Device=" + item.audioDevice + 
                    ", Command=" + item.command + 
                    ", Mode=" + item.mode + 
                    ", Selection=" + item.indexApplicationSelection +
                    ", Device Type=" + item.deviceType +
                    ", Device=" + item.midiControlConfiguration.Device + 
                    ", Channel=" + item.midiControlConfiguration.Channel + 
                    ", Controller=" + item.midiControlConfiguration.Controller + 
                    ", Controller Type=" + MidiControlConfiguration.GetControllerTypeString(item.midiControlConfiguration.ControllerType) +
                    ", Min Value=" + item.midiControlConfiguration.MinValue + 
                    ", Max Value=" + item.midiControlConfiguration.MaxValue + 
                    ", Value Scaling=" + item.midiControlConfiguration.ScalingValue;

                commandControlsStringList.Add(commandControlsString);
            }

            HardwareControls = commandControlsStringList;
        }
    }
}
