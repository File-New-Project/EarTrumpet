using System.Windows.Input;
using EarTrumpet.UI.Helpers;
using Windows.Devices.Midi;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace EarTrumpet.UI.ViewModels
{
    class MIDIControlWizardViewModel : BindableBase
    {
        public string Title { get; private set; }

        public ICommand SaveMidiControlCommand { get; }
        public ICommand SetMinValueCommand { get; }
        public ICommand SetMaxValueCommand { get; }

        const byte MIDI_VALUE_MAX = 127;

        private byte _minValue = 0;
        private byte _maxValue = MIDI_VALUE_MAX;

        private HardwareSettingsViewModel _hardwareSettings;
        private ObservableCollection<string> _capturedMidiInControls = new ObservableCollection<string>();
        System.Windows.Threading.Dispatcher _dispatcher;
        private int _liveValue = 0;
        private float _scalingValue = 0;

        public MIDIControlWizardViewModel(string title, HardwareSettingsViewModel hardwareSettings)
        {
            _hardwareSettings = hardwareSettings;

            Title = title;

            SaveMidiControlCommand = new RelayCommand(SaveMidiControl);
            SetMinValueCommand = new RelayCommand(SetMinValue);
            SetMaxValueCommand = new RelayCommand(SetMaxValue);

            _hardwareSettings.SelectedMidiInDevice.AddControlChangeCallback(midiInControlChangeCallback);

            _dispatcher = System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread);

            // Scaling value slider has a default value of 1.0 .
            ScalingValue = 1.0F;
        }
        private async void midiInControlChangeCallback(MidiControlChangeMessage msg)
        {
            await _dispatcher.InvokeAsync(() =>
            {
                bool elementFound = false;
                for (var i = 0; i < _capturedMidiInControls.Count(); i++)
                {
                    if (-1 != _capturedMidiInControls[i].IndexOf("Channel=" + msg.Channel + ", Controller=" + msg.Controller))
                    {
                        // This channel and controller pair is already part of the list.
                        // -> Just refresh the value.

                        _capturedMidiInControls[i] = "Channel=" + msg.Channel + ", Controller=" + msg.Controller + ", Value=" + msg.ControlValue;

                        elementFound = true;

                        // LiveValue must be updated when the changed channel and controller pair is the selected one.
                        if(i == CapturedMidiInControlsSelected)
                        {
                            int fullScaleRange = _maxValue - _minValue;

                            // Division by zero is not allowed.
                            // -> Set minimum full scale range in these cases.
                            if(fullScaleRange == 0)
                            {
                                fullScaleRange = 1;
                            }

                            if(_maxValue > _minValue)
                            {
                                LiveValue = Math.Abs((int)(((msg.ControlValue - _minValue) / (float)fullScaleRange) * ScalingValue * 100.0));
                            }
                            else
                            {
                                LiveValue = 100 - Math.Abs((int)(((msg.ControlValue - _maxValue) / (float)fullScaleRange) * ScalingValue * 100.0));
                            }
                        }

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
            _minValue = GetCurrentRawValue();
        }

        public void SetMaxValue()
        {
            _maxValue = GetCurrentRawValue();
        }
        private byte GetCurrentRawValue()
        {
            int valueStartPosition = _capturedMidiInControls[CapturedMidiInControlsSelected].IndexOf("Value=") + "Value=".Length;
            int valueEndPosition = _capturedMidiInControls[CapturedMidiInControlsSelected].Length;
            string valueString = _capturedMidiInControls[CapturedMidiInControlsSelected].Substring(valueStartPosition, valueEndPosition - valueStartPosition);

            return byte.Parse(valueString);
        }

        public ObservableCollection<string> CapturedMidiInControls
        {
            get
            {
                return _capturedMidiInControls;
            }
        }

        public int CapturedMidiInControlsSelected { get; set; }

        public int LiveValue
        {

            get
            {
                return _liveValue;
            }

            set
            {
                _liveValue = value;
                RaisePropertyChanged("LiveValue");
            }

        }

        public float ScalingValue {
            get
            {
                return _scalingValue;
            }
            set
            {
                _scalingValue = value;
                RaisePropertyChanged("ScalingValue");
            }
        }
    }
}