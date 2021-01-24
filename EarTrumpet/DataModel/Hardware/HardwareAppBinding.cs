using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EarTrumpet.DataModel.Storage;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.DataModel.Hardware
{
    public abstract class HardwareAppBinding
    {
        public abstract string Name { get; }
        public abstract Type ConfigType { get; }

        public abstract void AddCommand(CommandControlMappingElement command);
        public abstract CommandControlMappingElement GetCommandAt(int index);
        public abstract void RemoveCommandAt(int index);
        public abstract void ModifyCommandAt(int index, CommandControlMappingElement newCommand);

        public abstract Window GetConfigurationWindow(HardwareSettingsViewModel hardwareSettingsViewModel, 
            HardwareConfiguration loadConfig = null);
        
        protected DeviceCollectionViewModel _deviceCollectionViewModel;
        private ISettingsBag _settings;
        
        protected List<CommandControlMappingElement> _commandControlMappings;

        public List<CommandControlMappingElement> GetCommandControlMappings()
        {
            return _commandControlMappings;
        }
        
        protected HardwareAppBinding(DeviceCollectionViewModel deviceViewModel)
        {
            _deviceCollectionViewModel = deviceViewModel;
            _commandControlMappings = new List<CommandControlMappingElement>();
            
            _settings = StorageFactory.GetSettings();
            
        }
        
        protected List<DeviceViewModel> GetDevicesByName(string name)
        {
            var result = new List<DeviceViewModel>();
            foreach (var device in _deviceCollectionViewModel.AllDevices)
            {
                if (device.DisplayName == name || name == "*All Devices*")
                {
                    result.Add(device);
                }
            }

            return result;
        }
        
        protected List<IAppItemViewModel> GetAppsByName(string deviceName, string appName)
        {
            var devices = GetDevicesByName(deviceName);
            var result = new List<IAppItemViewModel>();

            foreach (var device in devices)
            {
                result.AddRange(device.Apps.Where(app => app.DisplayName == appName));
            }

            return result;
        }
        
        protected List<IAppItemViewModel> GetAppsByIndex(string deviceName, string index)
        {
            var result = new List<IAppItemViewModel>();
            
            foreach (var device in GetDevicesByName(deviceName))
            {
                try
                {
                    var i = int.Parse(index);
                    
                    if (device.Apps.Count() > i)
                    {
                        result.Add(device.Apps[i]);
                    }
                }
                catch (FormatException)
                {
                    return null;
                }
            }
            
            return result;
        }

        protected void LoadSettings(string key)
        {
            _commandControlMappings = _settings.Get(key, new List<CommandControlMappingElement>());
        }

        protected void SaveSettings(string key)
        {
            _settings.Set(key, _commandControlMappings);
        }
    }
}