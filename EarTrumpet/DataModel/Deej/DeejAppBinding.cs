using System;
using System.Collections.Generic;
using EarTrumpet.DataModel.Hardware;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.DataModel.Deej
{
    public class DeejAppBinding: HardwareAppBinding
    {
        private const string SAVEKEY = "DeejControls";

        public static DeejAppBinding Current;
        
        public DeejAppBinding(DeviceCollectionViewModel deviceViewModel) : base(deviceViewModel)
        {
            DeejIn.AddGeneralCallback(DeejCallback);
            
            LoadSettings(SAVEKEY);

            foreach (var command in _commandControlMappings)
            {
                var config = (DeejConfiguration) command.hardwareConfiguration;
                DeejIn._StartListening(config.Port);
            }

            Current = this;
        }

        public override Type ConfigType => typeof(DeejConfiguration);
        public override string Name => "deej";
        
        public override void AddCommand(CommandControlMappingElement command)
        {
            var config = (DeejConfiguration) command.hardwareConfiguration;
            DeejIn._StartListening(config.Port);
            
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

            var config = (DeejConfiguration) newCommand.hardwareConfiguration;
            DeejIn._StartListening(config.Port);

            _commandControlMappings[index] = newCommand;
            SaveSettings(SAVEKEY);
        }

        private void DeejCallback(string port, List<int> channels)
        {
            foreach (var command in _commandControlMappings)
            {
                var config = (DeejConfiguration) command.hardwareConfiguration;

                if (config.Port == port && channels.Count > config.Channel)
                {
                    var value = channels[config.Channel];
                    switch (command.command)
                    {
                        case CommandControlMappingElement.Command.SystemVolume:
                            SystemVolume(command, value);
                            break;
                        case CommandControlMappingElement.Command.SystemMute:
                            SystemMute(command, value);
                            break;
                        case CommandControlMappingElement.Command.ApplicationVolume:
                            ApplicationVolume(command, value);
                            break;
                        case CommandControlMappingElement.Command.ApplicationMute:
                            ApplicationMute(command, value);
                            break;
                    }
                }
            }
        }
        
        private void SystemVolume(CommandControlMappingElement command, int value)
        {
            var devices = GetDevicesByName(command.audioDevice);

            foreach (var device in devices)
            {
                device.Volume = SetVolume((DeejConfiguration)command.hardwareConfiguration, value);
            }
        }

        private void SystemMute(CommandControlMappingElement command, int value)
        {
            var devices = GetDevicesByName(command.audioDevice);

            foreach (var device in devices)
            {
                device.IsMuted = SetMute((DeejConfiguration)command.hardwareConfiguration, value);
            }
        }
        
        private void ApplicationVolume(CommandControlMappingElement command, int value)
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
                app.Volume = SetVolume((DeejConfiguration)command.hardwareConfiguration, value);
            }
        }

        private void ApplicationMute(CommandControlMappingElement command, int value)
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
                app.IsMuted = SetMute((DeejConfiguration)command.hardwareConfiguration, value);
            }
        }
        
        private static int SetVolume(DeejConfiguration config, int value)
        {
            int newVolume;
            var fullScaleRange = (float) config.MaxValue - config.MinValue;

            // Division by zero is not allowed.
            // -> Set minimum full scale range in these cases.
            if(fullScaleRange == 0)
            {
                fullScaleRange = 1;
            }

            if (config.MaxValue > config.MinValue)
            {
                newVolume = Math.Abs((int)(((value - config.MinValue) / (float)fullScaleRange) * 
                                           config.ScalingValue * 100.0));
            }
            else
            {
                newVolume = 100 - Math.Abs((int)(((value - config.MaxValue) / (float)fullScaleRange) 
                                                 * config.ScalingValue * 100.0));
            }

            return newVolume;
        }
        
        private static bool SetMute(DeejConfiguration config, int value)
        {
            var fullScaleRange = (float) config.MaxValue - config.MinValue;

            // Division by zero is not allowed.
            // -> Set minimum full scale range in these cases.
            if(fullScaleRange == 0)
            {
                fullScaleRange = 1;
            }

            int calcVolume;
            if (config.MaxValue > config.MinValue)
            {
                calcVolume = Math.Abs((int)(((value - config.MinValue) / (float)fullScaleRange) * 
                                            config.ScalingValue * 100.0));
            }
            else
            {
                calcVolume = 100 - Math.Abs((int)(((value - config.MaxValue) / (float)fullScaleRange) 
                                                  * config.ScalingValue * 100.0));
            }

            return calcVolume < 50;
        }
    }
}