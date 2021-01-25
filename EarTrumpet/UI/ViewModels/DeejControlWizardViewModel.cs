using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EarTrumpet.Properties;
using EarTrumpet.UI.Helpers;
using EarTrumpet.DataModel.Deej;

namespace EarTrumpet.UI.ViewModels
{
    class DeejControlWizardViewModel : BindableBase
    {
        public string Title { get; private set; }
        public ICommand SaveDeejControlCommand { get; }
        public ICommand SetMinValueCommand { get; }
        public ICommand SetMaxValueCommand { get; }
        public string SelectedDeejInDevice { get; set; }
        public int CapturedDeejInControlsSelected
        {
            get
            {
                return _capturedDeejInControlsSelected;
            }

            set
            {
                _capturedDeejInControlsSelected = value;
                UpdatePreviewValue();
            }
        }
        public string ScaleMinValueSelectDescription { get; set; }
        public string ScaleMaxValueSelectDescription { get; set; }
        public string DeejWizardMinMaxInstructionsText { get; set; }
        public ObservableCollection<string> CapturedDeejInControls
        {
            get
            {
                return _capturedDeejInControls;
            }
        }
        public ObservableCollection<string> DeejDevices
        {
            get
            {
                _availableDeejInDevices = DeejIn.GetAllDevices();
                ObservableCollection<String> availableDeejDevices = new ObservableCollection<string>();

                foreach (var dev in _availableDeejInDevices)
                {
                    availableDeejDevices.Add(dev);
                }

                return availableDeejDevices;
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
        public int MinValue
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
        public int MaxValue
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
        public string SelectedDeej
        {
            set
            {
                bool deviceFound = false;
                DeejIn.RemoveCallback(SelectedDeejInDevice, deejInControlChangeCallback);
                
                foreach (var dev in _availableDeejInDevices)
                {
                    if (dev == value)
                    {
                        // Clear captured controls from previously selected device.
                        _capturedDeejInControls.Clear();

                        SelectedDeejInDevice = dev;
                        DeejIn.AddCallback(dev, deejInControlChangeCallback);
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
                if (SelectedDeejInDevice != null)
                {
                    return SelectedDeejInDevice;
                }

                return "";
            }
        }

        // Deej controllers send control values from 0 to 1023.
        const int DEEJ_VALUE_MAX = 1023;

        private int _minValue = 0;
        private int _maxValue = DEEJ_VALUE_MAX;
        private HardwareSettingsViewModel _hardwareSettings;
        private ObservableCollection<string> _capturedDeejInControls = new ObservableCollection<string>();
        System.Windows.Threading.Dispatcher _dispatcher;
        private int _previewValue = 0;
        private float _scalingValue = 0;
        private List<string> _availableDeejInDevices;
        private int _controlValue = 0;
        private int _capturedDeejInControlsSelected = 0;

        public DeejControlWizardViewModel(string title, HardwareSettingsViewModel hardwareSettings)
        {
            _hardwareSettings = hardwareSettings;

            Title = title;

            SaveDeejControlCommand = new RelayCommand(SaveDeejControl);
            SetMinValueCommand = new RelayCommand(SetMinValue);
            SetMaxValueCommand = new RelayCommand(SetMaxValue);

            _availableDeejInDevices = DeejIn.GetAllDevices();

            _dispatcher = System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread);

            // Scaling value slider has a default value of 1.0 .
            ScalingValue = 1.0F;
        }

        public DeejControlWizardViewModel(string title, HardwareSettingsViewModel hardwareSettings,
            DeejConfiguration config): this(title, hardwareSettings)
        {
            MinValue = config.MinValue;
            MaxValue = config.MaxValue;
            ScalingValue = config.ScalingValue;
            SelectedDeej = config.Port;
            _capturedDeejInControls.Add("Channel=" + config.Channel +
                                        ", Value=0");
        }

        public void SaveDeejControl()
        {
            // Check for valid widget entries.
            if (string.IsNullOrEmpty(SelectedDeej) ||
                string.IsNullOrEmpty(CapturedDeejInControls[CapturedDeejInControlsSelected]))
            {
                MessageBox.Show(Resources.IncompleteDeviceConfigurationMessage, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Generate Deej control configuration object.
            DeejConfiguration deejConfiguration = new DeejConfiguration(SelectedDeej, GetCurrentSelectionProperty("Channel"), MinValue, MaxValue, ScalingValue);

            // Notify the hardware settings about the new control configuration.
            _hardwareSettings.ControlSelectedCallback(deejConfiguration);
        }

        public void SetMinValue()
        {
            MinValue = GetCurrentSelectionProperty("Value");
            UpdatePreviewValue();
        }

        public void SetMaxValue()
        {
            MaxValue = GetCurrentSelectionProperty("Value");
            UpdatePreviewValue();
        }

        private async void deejInControlChangeCallback(List<int> values)
        {
            await _dispatcher.InvokeAsync(() =>
            {
                for (var valueIterator = 0; valueIterator < values.Count; valueIterator++)
                {
                    bool elementFound = false;
                    for (var i = 0; i < _capturedDeejInControls.Count(); i++)
                    {
                        if (-1 != _capturedDeejInControls[i].IndexOf("Channel=" + valueIterator))
                        {
                            // This channel is already part of the list.
                            // -> Just refresh the value.

                            _capturedDeejInControls[i] = "Channel=" + i + ", Value=" + values[valueIterator];

                            elementFound = true;

                            // PreviewValue must be updated when the changed channel and controller pair is the selected one.
                            if (i == CapturedDeejInControlsSelected)
                            {
                                UpdatePreviewValue();
                            }

                            break;
                        }
                    }

                    if (!elementFound)
                    {
                        // The channel and controller pair was not part of the list, so add it.
                        _capturedDeejInControls.Add("Channel=" + valueIterator + ", Value=" + values[valueIterator]);
                        
                        if (_capturedDeejInControls.Count == 1)
                        {
                            CapturedDeejInControlsSelected = 0;
                            RaisePropertyChanged("CapturedDeejInControlsSelected");
                        }
                    }
                }
            });
        }

        private int GetCurrentSelectionProperty(string property)
        {
            // Cannot read propery when no item is selected or selected item
            // is not in list.
            if (CapturedDeejInControlsSelected < 0 ||
            _capturedDeejInControls.Count < CapturedDeejInControlsSelected + 1)
            {
                return 0;
            }

            var propertyDesignator = property + "=";

            var propertyStartPosition = _capturedDeejInControls[CapturedDeejInControlsSelected].IndexOf(propertyDesignator) + propertyDesignator.Length;
            var propertyEndPosition = _capturedDeejInControls[CapturedDeejInControlsSelected].IndexOf(",", propertyStartPosition);

            if(-1 == propertyEndPosition)
            {
                // Searched string not found, end reached (last property).
                propertyEndPosition = _capturedDeejInControls[CapturedDeejInControlsSelected].Length;
            }

            var propertyString = _capturedDeejInControls[CapturedDeejInControlsSelected].Substring(propertyStartPosition, propertyEndPosition - propertyStartPosition);

            return int.Parse(propertyString);
        }

        private void UpdatePreviewValue()
        {
            _controlValue = GetCurrentSelectionProperty("Value");

            int fullScaleRange = MaxValue - MinValue;

            // Division by zero is not allowed.
            // -> Set minimum full scale range in these cases.
            if (fullScaleRange == 0)
            {
                fullScaleRange = 1;
            }

            if (MaxValue > MinValue)
            {
                PreviewValue = Math.Abs((int)(((_controlValue - MinValue) / (float) fullScaleRange) * ScalingValue * 100.0));
            }
            else
                {
                    PreviewValue = 100 - Math.Abs((int)(((_controlValue - MaxValue) / (float)fullScaleRange) * ScalingValue * 100.0));
                }
        }
    }
}