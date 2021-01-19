using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Windows.Devices.Midi;
using Bugsnag.Payload;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.DataModel.MIDI
{
    public class MidiAppBinding
    {
        private DeviceCollectionViewModel _deviceCollectionViewModel;
        private List<CommandControlMappingElement> _commandControlMappings;
        
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

        private void MidiCallback(MidiInPort sender, MidiControlChangeMessage msg)
        {
            foreach (var command in _commandControlMappings)
            {
                if (command.midiDevice == MidiIn.GetDeviceById(sender.DeviceId).Name)
                {
                    var midiConfig = command.midiControlConfiguration;

                    if (msg.Channel == midiConfig.Channel && msg.Controller == midiConfig.Controller)
                    {
                        var app = GetAppByName(command.audioDevice, command.indexApplicationSelection);
                        if (app == null)
                        {
                            continue;
                        }
                        
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

                        app.Volume = newVolume;
                    }
                }
            }
        }

        public void AddCommand(CommandControlMappingElement command)
        {
            _commandControlMappings.Add(command);
        }
        
        public MidiAppBinding(DeviceCollectionViewModel deviceCollectionViewModel)
        {
            _deviceCollectionViewModel = deviceCollectionViewModel;
            
            MidiIn.AddGeneralCallback(MidiCallback);
            Current = this;
            _commandControlMappings = new List<CommandControlMappingElement>();
        }
    }
}