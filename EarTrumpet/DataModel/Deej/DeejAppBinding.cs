using System;
using System.Collections.Generic;
using EarTrumpet.DataModel.Hardware;
using EarTrumpet.DataModel.MIDI;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.DataModel.Deej
{
    public class DeejAppBinding: HardwareAppBinding
    {
        private const string SAVEKEY = "DeejControls";
        
        public DeejAppBinding(DeviceCollectionViewModel deviceViewModel) : base(deviceViewModel)
        {
        }

        public override Type ConfigType => typeof(DeejConfiguration);
        public override string Name => "Deej";
        
        public override void AddCommand(CommandControlMappingElement command)
        {
            throw new NotImplementedException();
        }

        public override void RemoveCommandAt(int index)
        {
            throw new NotImplementedException();
        }

        public override void ModifyCommandAt(int index, CommandControlMappingElement newCommand)
        {
            throw new NotImplementedException();
        }

        public override List<CommandControlMappingElement> GetCommandControlMappings()
        {
            return new List<CommandControlMappingElement>();
        }
    }
}