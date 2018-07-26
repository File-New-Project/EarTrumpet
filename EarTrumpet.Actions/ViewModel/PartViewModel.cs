using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using System;
using System.ComponentModel;

namespace EarTrumpet_Actions.ViewModel
{
    public class PartViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Part Part { get; }
        public string Description => Part.Description;

        private string _currentDescription;
        public string CurrentDescription
        {
            get => _currentDescription;
            set
            {
                if (_currentDescription != value)
                {
                    _currentDescription = value;
                    RaisePropertyChanged(nameof(CurrentDescription));
                }
            }
        }

        public string Verb

        {
            get
            {
                if (Part is BaseTrigger)
                {
                    return "When";
                }
                else if (Part is BaseCondition)
                {
                    return "If";
                }
                else if (Part is BaseAction)
                {
                    return "Do";
                }
                else throw new NotImplementedException();
            }
        }

        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public PartViewModel(Part part)
        {
            Part = part;

            UpdateDescription();
        }

        protected void UpdateDescription()
        {
            CurrentDescription = Part.Describe();
        }

        /*
        internal Part Part => _part;

        public bool IsShowApp => _isShowApp && IsExpanded;
        public bool IsShowDevice => _isShowDevice && IsExpanded;
        public bool IsShowOption => _issShowOption && IsExpanded;
        public bool IsShowHotkey => _isShowHotkey && IsExpanded;
        public bool IsShowText => _isShowTextBox && IsExpanded;
        public bool IsShowSlider => _isShowSlider && IsExpanded;

        public string Text { get; set; }
        public string PromptText { get; set; }
        public string HotkeyText => $"{_hotkey}";
        public double Volume { get; set; }

        public bool _isShowApp;
        public bool _isShowDevice;
        public bool _issShowOption;
        public bool _isShowHotkey;
        public bool _isShowTextBox;
        public bool _isShowSlider;
        public HotkeyData Hotkey
        {
            get => _hotkey;
            set
            {
                if (value != _hotkey)
                {
                    _hotkey = value;
                    RaisePropertyChanged(nameof(Hotkey));
                }
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    RaisePropertyChanged(nameof(IsExpanded));
                    RaisePropertyChanged(nameof(IsShowApp));
                    RaisePropertyChanged(nameof(IsShowDevice));
                    RaisePropertyChanged(nameof(IsShowHotkey));
                    RaisePropertyChanged(nameof(IsShowOption));
                    RaisePropertyChanged(nameof(IsShowText));
                    RaisePropertyChanged(nameof(IsShowSlider));
                    ExpandChanged(value);
                }
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (value != _displayName)
                {
                    _displayName = value;
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        public ObservableCollection<Option> AllDevices { get; private set; }

        Option _selectedDevice;
        public Option SelectedDevice
        {
            get
            {
                if (_selectedDevice == null && AllDevices != null)
                {
                    _selectedDevice = AllDevices.FirstOrDefault(o => ((Device)o.Value)?.Id == _device?.Id);
                    UpdateDisplayName();
                }
                return _selectedDevice;
            }
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    _device = (Device)_selectedDevice.Value;
                    RaisePropertyChanged(nameof(SelectedDevice));
                    UpdateDisplayName();
                }
            }
        }

        public ObservableCollection<Option> AllApps { get; private set; }

        Option _selectedApp;
        public Option SelectedApp
        {
            get
            {
                if (_selectedApp == null && AllApps != null)
                {
                    _selectedApp = AllApps.FirstOrDefault(o => ((App)o.Value)?.Id == _app?.Id);
                    UpdateDisplayName();
                }
                return _selectedApp;
            }
            set
            {
                if (_selectedApp != value)
                {
                    _selectedApp = value;
                    _app = (App)_selectedApp.Value;
                    RaisePropertyChanged(nameof(SelectedApp));
                    UpdateDisplayName();
                }
            }
        }

        Option _selected;
        public Option Selected
        {
            get
            {
                if (_selected == null && Options != null)
                {
                    _selected = Options.FirstOrDefault(o => o.Value == _part.Option);
                    UpdateDisplayName();
                }
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    _part.Option = _selected.Value;
                    RaisePropertyChanged(nameof(Selected));
                    UpdateDisplayName();
                }
            }
        }

        public List<Option> Options => _part.Options;

        private void UpdateDisplayName()
        {
            _part.Loaded();
            DisplayName = _part.DisplayName;
            RaisePropertyChanged(nameof(DisplayName));
        }


        protected virtual void ExpandChanged(bool isExpanding)
        {
            if (!isExpanding)
            {
                SaveParts();
            }
            UpdateDisplayName();
        }

        private Part _part;
        private Device _device;
        private App _app;
        private HotkeyData _hotkey;

        public PartViewModel(Part part)
        {
            _part = part;

            if (part is EventTrigger)
            {
                var et = (EventTrigger)part;
                _issShowOption = true;
            }
            else if (part is HotkeyTrigger)
            {
                var et = (HotkeyTrigger)part;
                _hotkey = et.Hotkey;
                _isShowHotkey = true;
            }
            else if (part is AudioDeviceEventTrigger)
            {
                var et = (AudioDeviceEventTrigger)part;
                _device = et.Device;
                _isShowDevice = true;
                _issShowOption = true;
            }
            else if (part is AudioDeviceSessionEventTrigger)
            {
                var et = (AudioDeviceSessionEventTrigger)part;
                _device = et.Device;
                _app = et.DeviceSession;
                _isShowDevice = true;
                _issShowOption = true;
                _isShowApp = true;
            }
            else if (part is ProcessTrigger)
            {
                var et = (ProcessTrigger)part;
                Text = et.ProcessName;
                PromptText = "Select a process by name (e.g. notepad.exe)";
                _isShowTextBox = true;
                _issShowOption = true;

            }
            else if (part is ProcessCondition)
            {
                var et = (ProcessCondition)part;
                Text = et.ProcessName;
                PromptText = "Select a process by name (e.g. notepad.exe)";
                _isShowTextBox = true;
                _issShowOption = true;

            }
            else if (part is DefaultPlaybackDeviceCondition)
            {
                var et = (DefaultPlaybackDeviceCondition)part;
                _device = et.Device;
                _isShowDevice = true;
                _issShowOption = true;

            }
            else if (part is SetDefaultDeviceAction)
            {
                var et = (SetDefaultDeviceAction)part;
                _device = et.Device;
                _isShowDevice = true;
                _issShowOption = true;

            }
            else if (part is ChangeDeviceVolumeAction)
            {
                var et = (ChangeDeviceVolumeAction)part;
                _device = et.Device;
                Volume = et.Volume;
                _isShowDevice = true;
                _isShowSlider = true;
                _issShowOption = true;
            }
            else if (part is ChangeAppVolumeAction)
            {
                var et = (ChangeAppVolumeAction)part;
                _device = et.Device;
                _app = et.DeviceSession;
                Volume = et.Volume;
                _isShowDevice = true;
                _isShowApp = true;
                _isShowSlider = true;
                _issShowOption = true;
            }

            ReloadAppsAndDevices();
        }

        private void SaveParts()
        {
            var part = _part;
            if (part is EventTrigger)
            {
                var et = (EventTrigger)part;

            }
            else if (part is HotkeyTrigger)
            {
                var et = (HotkeyTrigger)part;
                et.Hotkey = _hotkey;
            }
            else if (part is AudioDeviceEventTrigger)
            {
                var et = (AudioDeviceEventTrigger)part;
                et.Device = _device;
            }
            else if (part is AudioDeviceSessionEventTrigger)
            {
                var et = (AudioDeviceSessionEventTrigger)part;
                et.Device = _device;
                et.DeviceSession = _app;
            }
            else if (part is ProcessTrigger)
            {
                var et = (ProcessTrigger)part;
                et.ProcessName = Text;
            }
            else if (part is ProcessCondition)
            {
                var et = (ProcessCondition)part;
                et.ProcessName = Text;
            }
            else if (part is DefaultPlaybackDeviceCondition)
            {
                var et = (DefaultPlaybackDeviceCondition)part;
                et.Device = _device;
            }
            else if (part is SetDefaultDeviceAction)
            {
                var et = (SetDefaultDeviceAction)part;
                et.Device = _device;
            }
            else if (part is ChangeDeviceVolumeAction)
            {
                var et = (ChangeDeviceVolumeAction)part;
                et.Device = _device;
            }
            else if (part is ChangeAppVolumeAction)
            {
                var et = (ChangeAppVolumeAction)part;
                et.Device = _device;
                _app = et.DeviceSession;
                et.DeviceSession = _app;
            }
        }

        private void ReloadAppsAndDevices()
        {
            AllDevices = Device.AllDevices;
            if (AllDevices.FirstOrDefault(o => ((Device)o.Value)?.Id == _device?.Id) == null)
            {
                AllDevices.Add(new Option(_device.Id, _device));
            }
            RaisePropertyChanged(nameof(AllDevices));

            var newApps = App.AllApps;
            if (newApps.FirstOrDefault(o => ((App)o.Value)?.Id == _app?.Id) == null)
            {
                if (AllApps != null)
                {
                    newApps.Add(AllApps.FirstOrDefault(o => ((App)o.Value)?.Id == _app?.Id));
                }
                else
                {
                    newApps.Add(new Option(_app.Id, _app));
                }

                AllApps = newApps;
            }
            RaisePropertyChanged(nameof(AllApps));
        }

    */
    }
}