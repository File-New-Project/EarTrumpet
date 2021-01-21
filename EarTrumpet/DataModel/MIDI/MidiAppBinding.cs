using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Midi;
using EarTrumpet.DataModel.Storage;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.DataModel.MIDI
{
    public class MidiAppBinding
    {
        private DeviceCollectionViewModel _deviceCollectionViewModel;
        private List<CommandControlMappingElement> _commandControlMappings;
        private ISettingsBag _settings;

        // Maps device ids to device names
        private Dictionary<string, string> _deviceMapping;
        
        public static MidiAppBinding Current { get; set; }

        private DeviceViewModel GetDeviceByName(string name)
        {
            return _deviceCollectionViewModel.AllDevices.FirstOrDefault(device => device.DisplayName == name);
        }

        private IAppItemViewModel GetAppByName(string deviceName, string appName)
        {
            var device = GetDeviceByName(deviceName);

            return (device?.Apps).FirstOrDefault(app => app.DisplayName == appName);
        }
        
        private IAppItemViewModel GetAppByIndex(string deviceName, string index)
        {
            var device = GetDeviceByName(deviceName);
            int i;

            try
            {
                i = int.Parse(index);
            }
            catch (FormatException)
            {
                return null;
            }
            
            if (device.Apps.Count() < i)
            {
                return null;
            }

            return device.Apps[i];
        }

        private bool MidiEquals(MidiControlConfiguration midiConfig, MidiControlChangeMessage msg)
        {
            return midiConfig.Channel == msg.Channel && midiConfig.Controller == msg.Controller;
        }

        private int SetVolume(MidiControlConfiguration midiConfig, MidiControlChangeMessage msg, int oldVolume)
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
                case ControllerTypes.LINEAR_POTENTIOMETER when midiConfig.MaxValue > midiConfig.MinValue:
                    newVolume = Math.Abs((int)(((msg.ControlValue - midiConfig.MinValue) / (float)fullScaleRange) * midiConfig.ScalingValue * 100.0));
                    break;
                case ControllerTypes.LINEAR_POTENTIOMETER:
                    newVolume = 100 - Math.Abs((int)(((msg.ControlValue - midiConfig.MaxValue) / (float)fullScaleRange) * midiConfig.ScalingValue * 100.0));
                    break;
                case ControllerTypes.BUTTON:
                {
                    if (msg.ControlValue == midiConfig.MaxValue)
                    {
                        newVolume = oldVolume > 0 ? 0 : 100;
                    }

                    break;
                }
                case ControllerTypes.ROTARY_ENCODER when msg.ControlValue == midiConfig.MinValue:
                {
                    newVolume -= 1;
                    if (newVolume < 0)
                    {
                        newVolume = 0;
                    }

                    break;
                }
                case ControllerTypes.ROTARY_ENCODER:
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
        
        private bool SetMute(MidiControlConfiguration midiConfig, MidiControlChangeMessage msg, bool oldMute)
        {
            bool newMute = oldMute;
            float fullScaleRange = (float) midiConfig.MaxValue - midiConfig.MinValue;

            // Division by zero is not allowed.
            // -> Set minimum full scale range in these cases.
            if(fullScaleRange == 0)
            {
                fullScaleRange = 1;
            }

            if (midiConfig.ControllerType == ControllerTypes.LINEAR_POTENTIOMETER)
            {
                int calcVolume;
                if(midiConfig.MaxValue > midiConfig.MinValue)
                {
                    calcVolume = Math.Abs((int)(((msg.ControlValue - midiConfig.MinValue) / (float)fullScaleRange) * midiConfig.ScalingValue * 100.0));
                }
                else
                {
                    calcVolume = 100 - Math.Abs((int)(((msg.ControlValue - midiConfig.MaxValue) / (float)fullScaleRange) * midiConfig.ScalingValue * 100.0));
                }

                newMute = calcVolume < 50;
                
            } else if (midiConfig.ControllerType == ControllerTypes.BUTTON)
            {
                if (msg.ControlValue == midiConfig.MaxValue)
                {
                    newMute = !newMute;
                }
            } else if (midiConfig.ControllerType == ControllerTypes.ROTARY_ENCODER)
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
            IAppItemViewModel app = null;
            if (command.mode == CommandControlMappingElement.Mode.ApplicationSelection)
            {
                app = GetAppByName(command.audioDevice, command.indexApplicationSelection);
            } else if (command.mode == CommandControlMappingElement.Mode.Indexed)
            {
                app = GetAppByIndex(command.audioDevice, command.indexApplicationSelection);
            }
                
            if (app == null)
            {
                return;
            }

            app.Volume = SetVolume(command.midiControlConfiguration, msg, app.Volume);
        }

        private void ApplicationMute(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            IAppItemViewModel app = null;
            if (command.mode == CommandControlMappingElement.Mode.ApplicationSelection)
            {
                app = GetAppByName(command.audioDevice, command.indexApplicationSelection);
            } else if (command.mode == CommandControlMappingElement.Mode.Indexed)
            {
                app = GetAppByIndex(command.audioDevice, command.indexApplicationSelection);
            }
                
            if (app == null)
            {
                return;
            }

            app.IsMuted = SetMute(command.midiControlConfiguration, msg, app.IsMuted);
        }

        private void SystemVolume(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            var device = GetDeviceByName(command.audioDevice);
            if (device == null)
            {
                return;
            }

            device.Volume = SetVolume(command.midiControlConfiguration, msg, device.Volume);
        }

        private void SystemMute(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            var device = GetDeviceByName(command.audioDevice);
            if (device == null)
            {
                return;
            }

            device.IsMuted = SetMute(command.midiControlConfiguration, msg, device.IsMuted);
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
                    _deviceMapping.Add(device.Id, device.Name);
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
                if (command.midiDevice == GetMidiDeviceById(sender.DeviceId))
                {
                    if (MidiEquals(command.midiControlConfiguration, msg))
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

        public void AddCommand(CommandControlMappingElement command)
        {
            MidiIn._StartListening(MidiIn.GetDeviceByName(command.midiDevice)?.Id);
            
            _commandControlMappings.Add(command);
            _settings.Set("MidiControls", _commandControlMappings);
        }

        public void RemoveCommandIndex(int index)
        {
            if (_commandControlMappings.Count < index)
            {
                return;
            }
            
            _commandControlMappings.RemoveAt(index);
            _settings.Set("MidiControls", _commandControlMappings);
        }
        
        public List<CommandControlMappingElement> GetCommandControlMappings()
        {
            return _commandControlMappings;
        }

        public void ModifyCommandIndex(int index, CommandControlMappingElement newCommand)
        {
            if (_commandControlMappings.Count < index)
            {
                return;
            }
            MidiIn._StartListening(MidiIn.GetDeviceByName(newCommand.midiDevice)?.Id);
            
            _commandControlMappings[index] = newCommand;
        }
        
        private void SubscribeToDevices()
        {
            foreach (var command in _commandControlMappings)
            {
                MidiIn._StartListening(MidiIn.GetDeviceByName(command.midiDevice)?.Id);
            }
        }

        public MidiAppBinding(DeviceCollectionViewModel deviceCollectionViewModel)
        {
            _deviceCollectionViewModel = deviceCollectionViewModel;
            _settings = StorageFactory.GetSettings();
            
            MidiIn.AddGeneralCallback(MidiCallback);
            Current = this;
            // _settings.Set("MidiControls", new List<CommandControlMappingElement>());
            _commandControlMappings = _settings.Get("MidiControls", new List<CommandControlMappingElement>());
            _deviceMapping = new Dictionary<string, string>();

            foreach (var device in MidiIn.GetAllDevices())
            {
                _deviceMapping.Add(device.Id, device.Name);
            }
            
            SubscribeToDevices();
        }
    }
}