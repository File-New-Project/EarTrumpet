using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Windows.Devices.Midi;
using Bugsnag.Payload;
using EarTrumpet.DataModel.Storage;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.DataModel.MIDI
{
    public class MidiAppBinding
    {
        private DeviceCollectionViewModel _deviceCollectionViewModel;
        private List<CommandControlMappingElement> _commandControlMappings;
        private ISettingsBag _settings;
        
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
                i = Int32.Parse(index);
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

        private int ToVolume(MidiControlConfiguration midiConfig, MidiControlChangeMessage msg)
        {
            int newVolume;
            float fullScaleRange = (float) midiConfig.MaxValue - midiConfig.MinValue;

            // Division by zero is not allowed.
            // -> Set minimum full scale range in these cases.
            if(fullScaleRange == 0)
            {
                fullScaleRange = 1;
            }
                        
            if(midiConfig.MaxValue > midiConfig.MinValue)
            {
                newVolume = Math.Abs((int)(((msg.ControlValue - midiConfig.MinValue) / (float)fullScaleRange) * midiConfig.ScalingValue * 100.0));
            }
            else
            {
                newVolume = 100 - Math.Abs((int)(((msg.ControlValue - midiConfig.MaxValue) / (float)fullScaleRange) * midiConfig.ScalingValue * 100.0));
            }

            return newVolume;
        }
        private void ApplicationVolume(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            IAppItemViewModel app = null;
            if (command.mode == "Application Selection")
            {
                app = GetAppByName(command.audioDevice, command.indexApplicationSelection);
            } else if (command.mode == "Indexed")
            {
                app = GetAppByIndex(command.audioDevice, command.indexApplicationSelection);
            }
                
            if (app == null)
            {
                return;
            }

            app.Volume = ToVolume(command.midiControlConfiguration, msg);
        }

        private void ApplicationMute(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            IAppItemViewModel app = null;
            if (command.mode == "Application Selection")
            {
                app = GetAppByName(command.audioDevice, command.indexApplicationSelection);
            } else if (command.mode == "Indexed")
            {
                app = GetAppByIndex(command.audioDevice, command.indexApplicationSelection);
            }
                
            if (app == null)
            {
                return;
            }
            
            int volume = ToVolume(command.midiControlConfiguration, msg);
            app.IsMuted = (volume < 50);
        }

        private void SystemVolume(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            var device = GetDeviceByName(command.audioDevice);
            if (device == null)
            {
                return;
            }

            device.Volume = ToVolume(command.midiControlConfiguration, msg);
        }

        private void SystemMute(CommandControlMappingElement command, MidiControlChangeMessage msg)
        {
            var device = GetDeviceByName(command.audioDevice);
            if (device == null)
            {
                return;
            }

            int volume = ToVolume(command.midiControlConfiguration, msg);
            device.IsMuted = (volume < 50);
        }
        
        private void MidiCallback(MidiInPort sender, MidiControlChangeMessage msg)
        {
            foreach (var command in _commandControlMappings)
            {
                if (command.midiDevice == MidiIn.GetDeviceById(sender.DeviceId).Name)
                {
                    if (MidiEquals(command.midiControlConfiguration, msg))
                    {
                        if (command.command == "System Volume")
                        {
                            SystemVolume(command, msg);
                        } else if (command.command == "System Mute")
                        {
                            SystemMute(command, msg);
                        } else if (command.command == "Application Volume")
                        {
                            ApplicationVolume(command, msg);
                        } else if (command.command == "Application Mute")
                        {
                            ApplicationMute(command, msg);
                        }
                    }
                }
            }
        }

        public void AddCommand(CommandControlMappingElement command)
        {
            MidiIn._StartListening(MidiIn.GetDeviceByName(command.midiDevice).Id);
            
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
        
        private void SubsribeToDevices()
        {
            foreach (var command in _commandControlMappings)
            {
                MidiIn._StartListening(MidiIn.GetDeviceByName(command.midiDevice).Id);
            }
        }

        public MidiAppBinding(DeviceCollectionViewModel deviceCollectionViewModel)
        {
            _deviceCollectionViewModel = deviceCollectionViewModel;
            _settings = StorageFactory.GetSettings();
            
            MidiIn.AddGeneralCallback(MidiCallback);
            Current = this;
            _commandControlMappings = _settings.Get("MidiControls", new List<CommandControlMappingElement>());
            SubsribeToDevices();
        }
    }
}