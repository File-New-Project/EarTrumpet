using System;
using System.Collections.Generic;
using System.Linq;
using EarTrumpet.DataModel.Storage;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.DataModel.Hardware
{
    public abstract class HardwareAppBinding
    {
        public abstract Type ConfigType { get; }
        public abstract string Name { get; }
        
        public abstract void AddCommand(CommandControlMappingElement command);
        public abstract void RemoveCommandAt(int index);
        public abstract void ModifyCommandAt(int index, CommandControlMappingElement newCommand);

        public abstract List<CommandControlMappingElement> GetCommandControlMappings();
        
        protected DeviceCollectionViewModel _deviceCollectionViewModel;
        private ISettingsBag _settings;
        
        protected List<CommandControlMappingElement> _commandControlMappings;
        
        protected HardwareAppBinding(DeviceCollectionViewModel deviceViewModel)
        {
            _deviceCollectionViewModel = deviceViewModel;
            
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
            _commandControlMappings = _settings.Get("MidiControls", new List<CommandControlMappingElement>());
        }

        protected void SaveSettings(string key)
        {
            _settings.Set("MidiControls", _commandControlMappings);
        }
    }
}