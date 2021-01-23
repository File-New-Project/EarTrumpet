using System;
using System.Collections.Generic;
using System.Linq;
using EarTrumpet.DataModel.Deej;
using EarTrumpet.DataModel.MIDI;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.DataModel.Hardware
{
    public class HardwareManager
    {
        public static HardwareManager Current { get; set; }
        
        // map from a config type to a HardwareAppBinding instance
        private Dictionary<Type, HardwareAppBinding> bindings;

        public HardwareManager(DeviceCollectionViewModel deviceCollectionViewModel)
        {
            bindings = new Dictionary<Type, HardwareAppBinding>();

            RegisterAppBinding(new MidiAppBinding(deviceCollectionViewModel));
            RegisterAppBinding(new DeejAppBinding(deviceCollectionViewModel));
            
            Current = this;
        }

        public List<CommandControlMappingElement> GetCommandControlMappings()
        {
            var result = new List<CommandControlMappingElement>();
            foreach (var binding in bindings.Values)
            {
                result.AddRange(binding.GetCommandControlMappings());
            }

            return result;
        }

        public void AddCommand(CommandControlMappingElement command)
        {
            foreach (var type in bindings.Keys)
            {
                try
                {
                    var config = Convert.ChangeType(command.hardwareConfiguration, type);
                    bindings[type].AddCommand(command);
                    break;
                }
                catch (InvalidCastException)
                {
                    
                }
            }
        }

        public void RemoveCommandAt(int index)
        {
            var i = 0;
            foreach (var type in bindings.Keys)
            {
                if (index - i < bindings[type].GetCommandControlMappings().Count)
                {
                    bindings[type].RemoveCommandAt(index - i);
                    break;
                }

                i += bindings[type].GetCommandControlMappings().Count;
            }
        }

        public void ModifyCommandAt(int index, CommandControlMappingElement newCommand)
        {
            // Todo Add logic for changing the hardware type of a command
            var i = 0;
            foreach (var type in bindings.Keys)
            {
                if (index - i < bindings[type].GetCommandControlMappings().Count)
                {
                    bindings[type].ModifyCommandAt(index - i, newCommand);
                    break;
                }

                i += bindings[type].GetCommandControlMappings().Count;
            }
        }
        
        public List<string> GetDeviceTypes()
        {
            return bindings.Values.Select(value => value.Name).ToList();
        }

        public string GetConfigType(CommandControlMappingElement config)
        {
            foreach (var type in bindings.Keys)
            {
                try
                {
                    var c = Convert.ChangeType(config.hardwareConfiguration, type);
                    return bindings[type].Name;
                }
                catch (InvalidCastException)
                {
                    
                }
            }

            return "";
        }
        
        private void RegisterAppBinding(HardwareAppBinding appBinding)
        {
            bindings.Add(appBinding.ConfigType, appBinding);
        }
    }
}