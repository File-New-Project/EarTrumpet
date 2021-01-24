using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Windows.Devices.Midi;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EarTrumpet.Properties;
using EarTrumpet.UI.Helpers;
using EarTrumpet.DataModel.MIDI;

namespace EarTrumpet.UI.ViewModels
{
    class MIDIControlWizardViewModel : BindableBase
    {
        public string Title { get; private set; }
        public ICommand SaveMidiControlCommand { get; }
        public ICommand SetMinValueCommand { get; }
        public ICommand SetMaxValueCommand { get; }
        public string ScaleMinValueSelectDescription { get; set; }
        public string ScaleMaxValueSelectDescription { get; set; }
        public string MidiWizardMinMaxInstructions { get; set; }
        public MidiInDevice SelectedMidiInDevice { get; set; }
        public int ScalingMaximum { get; set; }
        public float ScalingTickFrequency { get; set; }
        public int CapturedMidiInControlsSelected { get; set; }
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
        public int ControlTypeSelected
        {
            get
            {
                return _controlTypeSelected;
            }

            set
            {
                _controlTypeSelected = value;

                string controlTypeSelectedString = ControlTypes[_controlTypeSelected];

                if (MidiConfiguration.GetControllerTypeString(ControllerTypes.LinearPotentiometer) == controlTypeSelectedString)
                {
                    ScaleMinValueSelectDescription = Resources.MinimumText;
                    ScaleMaxValueSelectDescription = Resources.MaximumText;
                    MidiWizardMinMaxInstructions = Resources.HardwareControlWizardMinMaxLinearPotentiometerControlTypeInstructions;
                    ScalingMaximum = 1;
                    ScalingTickFrequency = 0.01F;
                }
                else if (MidiConfiguration.GetControllerTypeString(ControllerTypes.Button) == controlTypeSelectedString)
                {
                    ScaleMinValueSelectDescription = Resources.ReleasedText;
                    ScaleMaxValueSelectDescription = Resources.PushedText;
                    MidiWizardMinMaxInstructions = Resources.HardwareControlWizardMinMaxButtonControlTypeInstructions;
                    ScalingMaximum = 1;
                    ScalingTickFrequency = 0.01F;
                }
                else if (MidiConfiguration.GetControllerTypeString(ControllerTypes.RotaryEncoder) == controlTypeSelectedString)
                {
                    ScaleMinValueSelectDescription = Resources.DecreaseText;
                    ScaleMaxValueSelectDescription = Resources.IncreaseText;
                    MidiWizardMinMaxInstructions = Resources.HardwareControlWizardMinMaxRotaryEncoderControlTypeInstructions;
                    ScalingMaximum = 100;
                    ScalingTickFrequency = 1.0F;
                }
                else
                {
                    // Unknown selection. Ignore.
                    return;
                }

                RaisePropertyChanged("ScaleMinValueSelectDescription");
                RaisePropertyChanged("ScaleMaxValueSelectDescription");
                RaisePropertyChanged("MidiWizardMinMaxInstructions");
                RaisePropertyChanged("ScalingMaximum");
                RaisePropertyChanged("ScalingTickFrequency");
            }
        }
        public ObservableCollection<string> ControlTypes
        {
            get
            {
                ObservableCollection<string> controlTypes = new ObservableCollection<string>();

                controlTypes.Add(MidiConfiguration.GetControllerTypeString(ControllerTypes.LinearPotentiometer));
                controlTypes.Add(MidiConfiguration.GetControllerTypeString(ControllerTypes.Button));
                controlTypes.Add(MidiConfiguration.GetControllerTypeString(ControllerTypes.RotaryEncoder));

                return controlTypes;
            }
        }
        public int PreviewValue
        {
            get
            {
                return _previewValue;
            }

            set
            {
                _previewValue = value;
                RaisePropertyChanged("PreviewValue");
            }
        }
        public float ScalingValue
        {
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
        public byte MinValue
        {
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
        public byte MaxValue
        {
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
                    // The selected device is unknown. This should never happen.
                    MessageBox.Show(Resources.UnknownDeviceSelectedMessageText, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // MIDI commands send control values from 0 to 127.
        const byte MIDI_VALUE_MAX = 127;

        private byte _minValue = 0;
        private byte _maxValue = MIDI_VALUE_MAX;
        private HardwareSettingsViewModel _hardwareSettings;
        private ObservableCollection<string> _capturedMidiInControls = new ObservableCollection<string>();
        System.Windows.Threading.Dispatcher _dispatcher;
        private int _previewValue = 0;
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

            // Default scaling maximum is 1 .
            ScalingMaximum = 1;

            // Default scaling tick frequency is 0.01 .
            ScalingTickFrequency = 0.01F;

            // Scaling value slider has a default value of 1.0 .
            ScalingValue = 1.0F;

            // Set default control type selection.
            ControlTypeSelected = _controlTypeSelected;
        }

        public MIDIControlWizardViewModel(string title, HardwareSettingsViewModel hardwareSettings,
            MidiConfiguration config): this(title, hardwareSettings)
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

        public void SaveMidiControl()
        {
            // Check for valid widget entries.
            if(string.IsNullOrEmpty(SelectedMidi) ||
                string.IsNullOrEmpty(CapturedMidiInControls[CapturedMidiInControlsSelected]) ||
                string.IsNullOrEmpty(ControlTypes[ControlTypeSelected]))
            {
                MessageBox.Show(Resources.IncompleteDeviceConfigurationMessage, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Generate MIDI control configuration object.
            MidiConfiguration midiConfiguration = new MidiConfiguration(SelectedMidi, GetCurrentSelectionProperty("Channel"), GetCurrentSelectionProperty("Controller"), MidiConfiguration.GetControllerType(ControlTypes[_controlTypeSelected]), MinValue, MaxValue, ScalingValue);

            // Notify the hardware settings about the new control configuration.
            _hardwareSettings.ControlSelectedCallback(midiConfiguration);
        }

        public void SetMinValue()
        {
            MinValue = GetCurrentSelectionProperty("Value");
        }

        public void SetMaxValue()
        {
            MaxValue = GetCurrentSelectionProperty("Value");
        }

        private byte GetCurrentSelectionProperty(string property)
        {
            var propertyDesignator = property + "=";

            var propertyStartPosition = _capturedMidiInControls[CapturedMidiInControlsSelected].IndexOf(propertyDesignator) + propertyDesignator.Length;
            var propertyEndPosition = _capturedMidiInControls[CapturedMidiInControlsSelected].IndexOf(",", propertyStartPosition);

            if (-1 == propertyEndPosition)
            {
                // Searched string not found, end reached (last property).
                propertyEndPosition = _capturedMidiInControls[CapturedMidiInControlsSelected].Length;
            }

            var propertyString = _capturedMidiInControls[CapturedMidiInControlsSelected].Substring(propertyStartPosition, propertyEndPosition - propertyStartPosition);

            return byte.Parse(propertyString);
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

                        // PreviewValue must be updated when the changed channel and controller pair is the selected one.
                        if (i == CapturedMidiInControlsSelected)
                        {
                            int fullScaleRange = MaxValue - MinValue;

                            // Division by zero is not allowed.
                            // -> Set minimum full scale range in these cases.
                            if (fullScaleRange == 0)
                            {
                                fullScaleRange = 1;
                            }

                            if (MaxValue > MinValue)
                            {
                                PreviewValue = Math.Abs((int)(((msg.ControlValue - MinValue) / (float)fullScaleRange) * ScalingValue * 100.0));
                            }
                            else
                            {
                                PreviewValue = 100 - Math.Abs((int)(((msg.ControlValue - MaxValue) / (float)fullScaleRange) * ScalingValue * 100.0));
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
    }
}