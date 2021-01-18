using System.Windows.Input;
using EarTrumpet.UI.Helpers;
using Windows.Devices.Midi;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.UI.ViewModels
{
    class MIDIControlWizardViewModel : BindableBase
    {
        public string Title { get; private set; }

        public ICommand SaveMidiControlCommand { get; }
        public ICommand SetMinValueCommand { get; }
        public ICommand SetMaxValueCommand { get; }

        private HardwareSettingsViewModel _hardwareSettings;
        private ObservableCollection<string> _capturedMidiInControls = new ObservableCollection<string>();
        System.Windows.Threading.Dispatcher _dispatcher;

        public MIDIControlWizardViewModel(string title, HardwareSettingsViewModel hardwareSettings)
        {
            _hardwareSettings = hardwareSettings;

            Title = title;

            SaveMidiControlCommand = new RelayCommand(SaveMidiControl);
            SetMinValueCommand = new RelayCommand(SetMinValue);
            SetMaxValueCommand = new RelayCommand(SetMaxValue);

            _hardwareSettings.SelectedMidiInDevice.AddControlChangeCallback(midiInControlChangeCallback);

            _dispatcher = System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread);
        }
        private async void midiInControlChangeCallback(MidiControlChangeMessage msg)
        {
            await _dispatcher.InvokeAsync(() =>
            {
                bool elementFound = false;
                for(var i = 0; i < _capturedMidiInControls.Count(); i++)
                {
                    if(-1 != _capturedMidiInControls[i].IndexOf("Channel=" + msg.Channel + ", Controller=" + msg.Controller))
                    {
                        // This channel and controller pair is already part of the list.
                        // -> Just refresh the value.

                        _capturedMidiInControls[i] = "Channel=" + msg.Channel + ", Controller=" + msg.Controller + ", Value=" + msg.ControlValue;

                        elementFound = true;

                        break;
                    }
                }

                if (!elementFound)
                {
                    // The channel and controller pair was not part of the list, so add it.
                    _capturedMidiInControls.Add("Channel=" + msg.Channel + ", Controller=" + msg.Controller + ", Value=" + msg.ControlValue);
                }
            });
        }

        public void SaveMidiControl()
        {
            // TODO
        }

        public void SetMinValue()
        {
            // TODO
        }

        public void SetMaxValue()
        {
            // TODO
        }

        public ObservableCollection<string> CapturedMidiInControls
        {
            get
            {
                return _capturedMidiInControls;
            }
        }

        public string CapturedMidiInControlsSelected{ get; set; }
    }
}