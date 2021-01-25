using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using Windows.Devices.Midi;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.Hardware;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;

namespace EarTrumpet.DataModel.MIDI
{
    public class MidiAppBinding: HardwareAppBinding
    {
        public override string Name => "MIDI";
        public override Type ConfigType => typeof(MidiConfiguration);
        
        public static MidiAppBinding Current;
        
        // Maps device ids to device names
        private ConcurrentDictionary<string, string> _deviceMapping;
        private const string SAVEKEY = "MidiControls";
        
        public MidiAppBinding(DeviceCollectionViewModel deviceCollectionViewModel, 
            IAudioDeviceManager audioDeviceManager): base(deviceCollectionViewModel, audioDeviceManager)
        {
            Current = this;

            MidiIn.AddGeneralCallback(MidiCallback);
            
            LoadSettings(SAVEKEY);
            
            _deviceMapping = new ConcurrentDictionary<string, string>();

            foreach (var device in MidiIn.GetAllDevices())
            {
                _deviceMapping[device.Id] = device.Name;
            }
            
            SubscribeToDevices();
        }
        
        public override void AddCommand(CommandControlMappingElement command)
        {
            var config = (MidiConfiguration) command.hardwareConfiguration;
            MidiIn._StartListening(MidiIn.GetDeviceByName(config.MidiDevice)?.Id);

            _commandControlMappings.Add(command);
            SaveSettings(SAVEKEY);
        }
        
        public override CommandControlMappingElement GetCommandAt(int index)
        {
            return _commandControlMappings.Count < index ? null : _commandControlMappings[index];
        }

        public override void RemoveCommandAt(int index)
        {
            if (_commandControlMappings.Count < index)
            {
                return;
            }
            
            _commandControlMappings.RemoveAt(index);
            SaveSettings(SAVEKEY);
        }

        public override void ModifyCommandAt(int index, CommandControlMappingElement newCommand)
        {
            if (_commandControlMappings.Count < index)
            {
                return;
            }

            var config = (MidiConfiguration) newCommand.hardwareConfiguration;
            MidiIn._StartListening(MidiIn.GetDeviceByName(config.MidiDevice)?.Id);

            _commandControlMappings[index] = newCommand;
            SaveSettings(SAVEKEY);
        }
        
        public override Window GetConfigurationWindow(HardwareSettingsViewModel hardwareSettingsViewModel, 
            HardwareConfiguration loadedConfig = null)
        {
            MIDIControlWizardViewModel viewModel = null;
           
            if (!(loadedConfig is MidiConfiguration))
            {
                viewModel = new MIDIControlWizardViewModel(EarTrumpet.Properties.Resources.MidiControlWizardText, 
                    hardwareSettingsViewModel);
            }
            else
            {
                viewModel = new MIDIControlWizardViewModel(Properties.Resources.MidiControlWizardText,
                    hardwareSettingsViewModel, (MidiConfiguration)loadedConfig);
            }

            return new MIDIControlWizardWindow { DataContext = viewModel};
        }

        public override int CalculateVolume(int value, int minValue, int maxValue, float scalingValue)
        {
            var fullScaleRange = maxValue - minValue;

            // Division by zero is not allowed.
            // -> Set minimum full scale range in these cases.
            if (fullScaleRange == 0)
            {
                fullScaleRange = 1;
            }
        
            if (maxValue > minValue)
            {
                value = Math.Min(Math.Max(value, minValue), maxValue);
                return Math.Abs((int)(((value - minValue) / (float) fullScaleRange) * scalingValue * 100.0));
            }
            
            value = Math.Min(Math.Max(value, maxValue), minValue);
            return 100 - Math.Abs((int)(((value - maxValue) / (float)fullScaleRange) * scalingValue * 100.0));
        }


        private static bool MidiEquals(MidiConfiguration midiConfig, MidiControlChangeMessage msg)
        {
            return midiConfig.Channel == msg.Channel && midiConfig.Controller == msg.Controller;
        }

        private static int SetVolume(MidiConfiguration midiConfig, MidiControlChangeMessage msg, int oldVolume)
        {
            var newVolume = oldVolume;

            switch (midiConfig.ControllerType)
            {
                case ControllerTypes.LinearPotentiometer:
                    newVolume = Current.CalculateVolume(msg.ControlValue, midiConfig.MinValue, midiConfig.MaxValue,
                        midiConfig.ScalingValue);
                    break;
                case ControllerTypes.Button:
                {
                    if (msg.ControlValue == midiConfig.MaxValue)
                    {
                        newVolume = oldVolume > 0 ? 0 : 100;
                    }

                    break;
                }
                case ControllerTypes.RotaryEncoder when msg.ControlValue == midiConfig.MinValue:
                {
                    newVolume -= 1;
                    if (newVolume < 0)
                    {
                        newVolume = 0;
                    }

                    break;
                }
                case ControllerTypes.RotaryEncoder:
                {
                    if (msg.ControlValue == midiConfig.MaxValue)
                    {
                        newVolume += 1;
                        
                        if (newVolume > 100)
                        {
                            newVolume = 100;
                        }
                    }

                    break;
                }
            }
            
            return newVolume;
        }
        
        private static bool SetMute(MidiConfiguration midiConfig, MidiControlChangeMessage msg, bool oldMute)
        {
            var newMute = oldMute;
            
            if (midiConfig.ControllerType == ControllerTypes.LinearPotentiometer)
            {
                var calcVolume = Current.CalculateVolume(msg.ControlValue, midiConfig.MinValue, midiConfig.MaxValue,
                    midiConfig.ScalingValue);

                newMute = calcVolume < 50;
                
            } else if (midiConfig.ControllerType == ControllerTypes.Button)
            {
                if (msg.ControlValue == midiConfig.MaxValue)
                {
                    newMute = !newMute;
                }
            } else if (midiConfig.ControllerType == ControllerTypes.RotaryEncoder)
            {
                if (msg.ControlValue == midiConfig.MaxValue)
                {
                    newMute = false;
                }
                else
                {
                    newMute = true;
                }
            }
            
            return newMute;
        }
        
        private void ApplicationVolume(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            List<IAppItemViewModel> apps = null;
            if (command.mode == CommandControlMappingElement.Mode.ApplicationSelection)
            {
                apps = GetAppsByName(command.audioDevice, command.indexApplicationSelection);
            } else if (command.mode == CommandControlMappingElement.Mode.Indexed)
            {
                apps = GetAppsByIndex(command.audioDevice, command.indexApplicationSelection);
            }
                
            if (apps == null)
            {
                return;
            }

            foreach (var app in apps)
            {
                app.Volume = SetVolume((MidiConfiguration)command.hardwareConfiguration, msg, app.Volume);
            }
        }

        private void ApplicationMute(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            List<IAppItemViewModel> apps = null;
            if (command.mode == CommandControlMappingElement.Mode.ApplicationSelection)
            {
                apps = GetAppsByName(command.audioDevice, command.indexApplicationSelection);
            } else if (command.mode == CommandControlMappingElement.Mode.Indexed)
            {
                apps = GetAppsByIndex(command.audioDevice, command.indexApplicationSelection);
            }
                
            if (apps == null)
            {
                return;
            }
            
            foreach (var app in apps)
            {
                app.IsMuted = SetMute((MidiConfiguration)command.hardwareConfiguration, msg, app.IsMuted);
            }
        }

        private void SystemVolume(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            var devices = GetDevicesByName(command.audioDevice);

            foreach (var device in devices)
            {
                device.Volume = SetVolume((MidiConfiguration)command.hardwareConfiguration, msg, device.Volume);
            }
        }

        private void SystemMute(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            var devices = GetDevicesByName(command.audioDevice);

            foreach (var device in devices)
            {
                device.IsMuted = SetMute((MidiConfiguration)command.hardwareConfiguration, msg, device.IsMuted);
            }
        }

        private void DefaultDevice(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            var config = (MidiConfiguration) command.hardwareConfiguration;
            var calcVolume = Current.CalculateVolume(msg.ControlValue, config.MinValue, config.MaxValue, config.ScalingValue);

            if (calcVolume > 50)
            {
                foreach (var device in _audioDeviceManager.Devices)
                {
                    if (device.DisplayName == command.audioDevice)
                    {
                        _audioDeviceManager.Default = device;

                        return;
                    }
                }
            }
        }

        private void CycleDefaultDevice(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            var config = (MidiConfiguration) command.hardwareConfiguration;
            var calcVolume = CalculateVolume(msg.ControlValue, config.MinValue, config.MaxValue, config.ScalingValue);

            var cycle = false;
            var direction = 1;
            
            switch (config.ControllerType)
            {
                case ControllerTypes.Button:
                case ControllerTypes.LinearPotentiometer:
                    if (calcVolume > 50)
                    {
                        cycle = true;
                        direction = 1;
                    }
                    break;
                
                case ControllerTypes.RotaryEncoder when msg.ControlValue == config.MinValue:
                    cycle = true;
                    direction = -1;
                    break;
                case ControllerTypes.RotaryEncoder:
                    cycle = true;
                    direction = 1;
                    break;
            }

            if (cycle)
            {
                var index = -1;
                for (var i = 0; i < _audioDeviceManager.Devices.Count; i++)
                {
                    if (_audioDeviceManager.Devices[i] == _audioDeviceManager.Default)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    index = index + direction;
                    index = (index + _audioDeviceManager.Devices.Count) % _audioDeviceManager.Devices.Count;
                }
                else
                {
                    index = 0;
                }
                
                _audioDeviceManager.Default = _audioDeviceManager.Devices[index];
            }
        }
        
        private string GetMidiDeviceById(string id)
        {
            if (_deviceMapping.ContainsKey(id))
            {
                return _deviceMapping[id];
            }
            
            foreach (var device in MidiIn.GetAllDevices())
            {
                if (!_deviceMapping.ContainsKey(device.Id))
                {
                    _deviceMapping[device.Id] = device.Name;
                    if (device.Id == id)
                    {
                        return device.Name;
                    }
                }
            }

            return "";
        }

        private void MidiCallback(MidiInPort sender, MidiControlChangeMessage msg)
        {
            foreach (var command in _commandControlMappings)
            {
                var config = (MidiConfiguration) command.hardwareConfiguration;
                
                if (config.MidiDevice == GetMidiDeviceById(sender.DeviceId))
                {
                    if (MidiEquals(config, msg))
                    {
                        switch (command.command)
                        {
                            case CommandControlMappingElement.Command.SystemVolume:
                                SystemVolume(command, msg);
                                break;
                            case CommandControlMappingElement.Command.SystemMute:
                                SystemMute(command, msg);
                                break;
                            case CommandControlMappingElement.Command.ApplicationVolume:
                                ApplicationVolume(command, msg);
                                break;
                            case CommandControlMappingElement.Command.ApplicationMute:
                                ApplicationMute(command, msg);
                                break;
                            case CommandControlMappingElement.Command.SetDefaultDevice:
                                DefaultDevice(command, msg);
                                break;
                            case CommandControlMappingElement.Command.CycleDefaultDevice:
                                CycleDefaultDevice(command, msg);
                                break;
                        }
                    }
                }
            }
        }

        private void SubscribeToDevices()
        {
            foreach (var command in _commandControlMappings)
            {
                var config = (MidiConfiguration) command.hardwareConfiguration;
                MidiIn._StartListening(MidiIn.GetDeviceByName(config.MidiDevice)?.Id);
            }
        }
    }
}