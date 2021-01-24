using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using Windows.Devices.Midi;
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
        
        public MidiAppBinding(DeviceCollectionViewModel deviceCollectionViewModel): base(deviceCollectionViewModel)
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
        
        
        private static bool MidiEquals(MidiConfiguration midiConfig, MidiControlChangeMessage msg)
        {
            return midiConfig.Channel == msg.Channel && midiConfig.Controller == msg.Controller;
        }

        private static int SetVolume(MidiConfiguration midiConfig, MidiControlChangeMessage msg, int oldVolume)
        {
            var newVolume = oldVolume;
            var fullScaleRange = (float) midiConfig.MaxValue - midiConfig.MinValue;

            // Division by zero is not allowed.
            // -> Set minimum full scale range in these cases.
            if(fullScaleRange == 0)
            {
                fullScaleRange = 1;
            }

            switch (midiConfig.ControllerType)
            {
                case ControllerTypes.LinearPotentiometer when midiConfig.MaxValue > midiConfig.MinValue:
                    newVolume = Math.Abs((int)(((msg.ControlValue - midiConfig.MinValue) / (float)fullScaleRange) * 
                                               midiConfig.ScalingValue * 100.0));
                    break;
                case ControllerTypes.LinearPotentiometer:
                    newVolume = 100 - Math.Abs((int)(((msg.ControlValue - midiConfig.MaxValue) / (float)fullScaleRange) 
                                                     * midiConfig.ScalingValue * 100.0));
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
            var fullScaleRange = (float) midiConfig.MaxValue - midiConfig.MinValue;

            // Division by zero is not allowed.
            // -> Set minimum full scale range in these cases.
            if(fullScaleRange == 0)
            {
                fullScaleRange = 1;
            }

            if (midiConfig.ControllerType == ControllerTypes.LinearPotentiometer)
            {
                int calcVolume;
                if(midiConfig.MaxValue > midiConfig.MinValue)
                {
                    calcVolume = Math.Abs((int)(((msg.ControlValue - midiConfig.MinValue) / (float)fullScaleRange) *
                                                midiConfig.ScalingValue * 100.0));
                }
                else
                {
                    calcVolume = 100 - Math.Abs((int)(((msg.ControlValue - midiConfig.MaxValue) / (float)fullScaleRange)
                                                      * midiConfig.ScalingValue * 100.0));
                }

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