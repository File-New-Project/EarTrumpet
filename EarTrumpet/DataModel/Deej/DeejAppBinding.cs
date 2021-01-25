using System;
using System.Collections.Generic;
using System.Windows;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.Hardware;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;

namespace EarTrumpet.DataModel.Deej
{
    public class DeejAppBinding: HardwareAppBinding
    {
        public override string Name => "deej";
        public override Type ConfigType => typeof(DeejConfiguration);
        
        public static DeejAppBinding Current;
        private const string SAVEKEY = "DeejControls";

        public DeejAppBinding(DeviceCollectionViewModel deviceViewModel, IAudioDeviceManager audioDeviceManager) : 
            base(deviceViewModel, audioDeviceManager)
        {
            Current = this;

            DeejIn.AddGeneralCallback(DeejCallback);
            
            LoadSettings(SAVEKEY);

            foreach (var command in _commandControlMappings)
            {
                var config = (DeejConfiguration) command.hardwareConfiguration;
                DeejIn._StartListening(config.Port);
            }
        }

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

        public override Window GetConfigurationWindow(HardwareSettingsViewModel hardwareSettingsViewModel, 
            HardwareConfiguration loadedConfig = null)
        {
            DeejControlWizardViewModel viewModel = null;

            if (loadedConfig == null || !(loadedConfig is DeejConfiguration))
            {
                viewModel = new DeejControlWizardViewModel(Properties.Resources.DeejControlWizardText, 
                    hardwareSettingsViewModel);
            }
            else
            {
                viewModel = new DeejControlWizardViewModel(Properties.Resources.DeejControlWizardText,
                    hardwareSettingsViewModel, (DeejConfiguration)loadedConfig);
            }

            return new DeejControlWizardWindow { DataContext = viewModel };
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
                        case CommandControlMappingElement.Command.SetDefaultDevice:
                            DefaultDevice(command, value);
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
        
        private void DefaultDevice(CommandControlMappingElement command, int value)
        {
            var config = (DeejConfiguration) command.hardwareConfiguration;
            var calcVolume = Current.CalculateVolume(value, config.MinValue, config.MaxValue, config.ScalingValue);

            if (calcVolume > 50)
            {
                foreach (var device in _audioDeviceManager.Devices)
                {
                    if (device.DisplayName == command.audioDevice)
                    {
                        if (_audioDeviceManager.Default != device)
                        {
                            _audioDeviceManager.Default = device;
                        }
                       
                        return;
                    }
                }
            }
        }
        
        private static int SetVolume(DeejConfiguration config, int value)
        {
            return Current.CalculateVolume(value, config.MinValue, config.MaxValue, config.ScalingValue);
        }
        
        private static bool SetMute(DeejConfiguration config, int value)
        {
            var calcVolume = Current.CalculateVolume(value, config.MinValue, config.MaxValue, config.ScalingValue);

            return calcVolume < 50;
        }
    }
}