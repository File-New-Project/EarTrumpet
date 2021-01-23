using System.Windows.Input;
using EarTrumpet.UI.Helpers;
using Windows.Devices.Midi;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using EarTrumpet.DataModel.MIDI;
using EarTrumpet.Properties;
using System.Collections.Generic;

namespace EarTrumpet.UI.ViewModels
{
    class MIDIControlWizardViewModel : BindableBase
    {
        public string Title { get; private set; }

        public ICommand SaveMidiControlCommand { get; }
        public ICommand SetMinValueCommand { get; }
        public ICommand SetMaxValueCommand { get; }

        // MIDI commands send control values from 0 to 127.
        const byte MIDI_VALUE_MAX = 127;

        private byte _minValue = 0;
        private byte _maxValue = MIDI_VALUE_MAX;

        private HardwareSettingsViewModel _hardwareSettings;
        private ObservableCollection<string> _capturedMidiInControls = new ObservableCollection<string>();
        System.Windows.Threading.Dispatcher _dispatcher;
        private int _liveValue = 0;
        private float _scalingValue = 0;
        private int _controlTypeSelected = 0;
        private List<MidiInDevice> _availableMidiInDevices;

        public MIDIControlWizardViewModel(string title, HardwareSettingsViewModel hardwareSettings)
        {
            _hardwareSettings = hardwareSettings;

            Title = title;

            SaveMidiControlCommand = new RelayCommand(SaveMidiControl);
            SetMinValueCommand = new RelayCommand(SetMinValue);
            SetMaxValueCommand = new RelayCommand(SetMaxValue);

            _availableMidiInDevices = MidiIn.GetAllDevices();

            _dispatcher = System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread);

            // Scaling value slider has a default value of 1.0 .
            ScalingValue = 1.0F;

            // Set default control type selection.
            ControlTypeSelected = _controlTypeSelected;
        }

        public MIDIControlWizardViewModel(string title, HardwareSettingsViewModel hardwareSettings,
            MidiControlConfiguration config): this(title, hardwareSettings)
        {
            //GetCurrentSelectionProperty("Channel"), GetCurrentSelectionProperty("Controller"), MinValue, MaxValue, ScalingValue
            _capturedMidiInControls.Add("Channel=" + config.Channel + ", Controller=" + config.Controller +
                                        ", Value=0");
            MinValue = config.MinValue;
            MaxValue = config.MaxValue;
            ScalingValue = config.ScalingValue;
            ControlTypeSelected = (int)config.ControllerType;
            SelectedMidi = config.MidiDevice;
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
                            int fullScaleRange = MaxValue - MinValue;

                            // Division by zero is not allowed.
                            // -> Set minimum full scale range in these cases.
                            if(fullScaleRange == 0)
                            {
                                fullScaleRange = 1;
                            }

                            if(MaxValue > MinValue)
                            {
                                LiveValue = Math.Abs((int)(((msg.ControlValue - MinValue) / (float)fullScaleRange) * ScalingValue * 100.0));
                            }
                            else
                            {
                                LiveValue = 100 - Math.Abs((int)(((msg.ControlValue - MaxValue) / (float)fullScaleRange) * ScalingValue * 100.0));
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
            // Generate MIDI control configuration object.
            MidiControlConfiguration midiControlConfiguration = new MidiControlConfiguration(SelectedMidi, GetCurrentSelectionProperty("Channel"), GetCurrentSelectionProperty("Controller"), MidiControlConfiguration.GetControllerType(ControlTypes[_controlTypeSelected]), MinValue, MaxValue, ScalingValue);

            // Notify the hardware settings about the new control configuration.
            _hardwareSettings.MidiControlSelectedCallback(midiControlConfiguration);
        }

        public void SetMinValue()
        {
            MinValue = GetCurrentSelectionProperty("Value");
        }

        public void SetMaxValue()
        {
            MaxValue = GetCurrentSelectionProperty("Value");
        }

        byte GetCurrentSelectionProperty(string property)
        {
            var propertyDesignator = property + "=";

            var propertyStartPosition = _capturedMidiInControls[CapturedMidiInControlsSelected].IndexOf(propertyDesignator) + propertyDesignator.Length;
            var propertyEndPosition = _capturedMidiInControls[CapturedMidiInControlsSelected].IndexOf(",", propertyStartPosition);

            if(-1 == propertyEndPosition)
            {
                // Searched string not found, end reached (last property).
                propertyEndPosition = _capturedMidiInControls[CapturedMidiInControlsSelected].Length;
            }

            var propertyString = _capturedMidiInControls[CapturedMidiInControlsSelected].Substring(propertyStartPosition, propertyEndPosition - propertyStartPosition);

            return byte.Parse(propertyString);
        }

        public ObservableCollection<string> CapturedMidiInControls
        {
            get
            {
                return _capturedMidiInControls;
            }
        }

        public ObservableCollection<string> MidiDevices
        {
            get
            {
                _availableMidiInDevices = MidiIn.GetAllDevices();
                ObservableCollection<String> availableMidiDevices = new ObservableCollection<string>();

                foreach (var dev in _availableMidiInDevices)
                {
                    availableMidiDevices.Add(dev.Name);
                }

                return availableMidiDevices;
            }
        }

        public int CapturedMidiInControlsSelected { get; set; }

        public int ControlTypeSelected {
            get
            {
                return _controlTypeSelected;
            }

            set
            {
                _controlTypeSelected = value;

                string controlTypeSelectedString = ControlTypes[_controlTypeSelected];

                if(MidiControlConfiguration.GetControllerTypeString(ControllerTypes.LINEAR_POTENTIOMETER) == controlTypeSelectedString)
                {
                    ScaleMinValueSelectDescription = Resources.HardwareDeviceLinearControllerScaleMinValueText;
                    ScaleMaxValueSelectDescription = Resources.HardwareDeviceLinearControllerScaleMaxValueText;
                    MidiWizardMinMaxInstructionsText = Resources.MidiWizardMinMaxInstructionsLinearPotentiometerControlType;
                }
                else if (MidiControlConfiguration.GetControllerTypeString(ControllerTypes.BUTTON) == controlTypeSelectedString)
                {
                    ScaleMinValueSelectDescription = Resources.HardwareDeviceButtonControllerScaleMinValueText;
                    ScaleMaxValueSelectDescription = Resources.HardwareDeviceButtonControllerScaleMaxValueText;
                    MidiWizardMinMaxInstructionsText = Resources.MidiWizardMinMaxInstructionsButtonControlType;
                }
                else if (MidiControlConfiguration.GetControllerTypeString(ControllerTypes.ROTARY_ENCODER) == controlTypeSelectedString)
                {
                    ScaleMinValueSelectDescription = Resources.HardwareDeviceRotaryEncoderControllerScaleMinValueText;
                    ScaleMaxValueSelectDescription = Resources.HardwareDeviceRotaryEncoderControllerScaleMaxValueText;
                    MidiWizardMinMaxInstructionsText = Resources.MidiWizardMinMaxInstructionsRotaryEncoderControlType;

                }
                else
                {
                    // Unknown selection. Ignore.
                    return;
                }

                RaisePropertyChanged("ScaleMinValueSelectDescription");
                RaisePropertyChanged("ScaleMaxValueSelectDescription");
                RaisePropertyChanged("MidiWizardMinMaxInstructionsText");
            }
        }

        public string ScaleMinValueSelectDescription { get; set; }

        public string ScaleMaxValueSelectDescription { get; set; }

        public string MidiWizardMinMaxInstructionsText{ get; set; }

        public ObservableCollection<string> ControlTypes
        {
            get
            {
                ObservableCollection<string> controlTypes = new ObservableCollection<string>();

                controlTypes.Add(MidiControlConfiguration.GetControllerTypeString(ControllerTypes.LINEAR_POTENTIOMETER));
                controlTypes.Add(MidiControlConfiguration.GetControllerTypeString(ControllerTypes.BUTTON));
                controlTypes.Add(MidiControlConfiguration.GetControllerTypeString(ControllerTypes.ROTARY_ENCODER));

                return controlTypes;
            }
        }

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

        public byte MinValue {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
                RaisePropertyChanged("MinValue");
            }
        }

        public byte MaxValue {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
                RaisePropertyChanged("MaxValue");
            }
        }
        public string SelectedMidi
        {
            set
            {
                bool deviceFound = false;

                foreach (var dev in _availableMidiInDevices)
                {
                    if (dev.Name == value)
                    {
                        // TODO: Remove previously selected midiindevice (if one was selected).
                        SelectedMidiInDevice = dev;
                        SelectedMidiInDevice.AddControlChangeCallback(midiInControlChangeCallback);
                        deviceFound = true;
                    }
                }

                if (!deviceFound)
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
        public MidiInDevice SelectedMidiInDevice { get; set; }

    }
}