using EarTrumpet.DataModel.Hardware;

namespace EarTrumpet.DataModel.MIDI
{
    public enum ControllerTypes
    {
        LinearPotentiometer = 0,
        Button = 1,
        RotaryEncoder = 2,
        InvalidEntry = 3
    }

    public class MidiConfiguration : HardwareConfiguration
    {
        public byte Channel { get; set; }
        public byte Controller { get; set; }
        public ControllerTypes ControllerType { get; set; }
        public byte MinValue { get; set; }
        public byte MaxValue { get; set; }
        public float ScalingValue { get; set; }
        public string MidiDevice { get; set; }

        // Constructor
        public MidiConfiguration(string device, byte channel, byte controller, ControllerTypes controllerType, 
            byte minValue, byte maxValue, float scalingValue)
        {
            MidiDevice = device;
            Channel = channel;
            Controller = controller;
            ControllerType = controllerType;
            MinValue = minValue;
            MaxValue = maxValue;
            ScalingValue = scalingValue;
        }

        // Default constructor required for serialization.
        public MidiConfiguration()
        {
            
        }
        
        public static string GetControllerTypeString(ControllerTypes controllerType)
        {
            switch (controllerType)
            {
                case ControllerTypes.LinearPotentiometer:
                    return Properties.Resources.LinearPotentiometerText;
                case ControllerTypes.Button:
                    return Properties.Resources.ButtonText;
                case ControllerTypes.RotaryEncoder:
                    return Properties.Resources.RotaryEncoderText;
                default:
                    return "";
            }
        }

        public static ControllerTypes GetControllerType(string controllerTypeString)
        {
            if (GetControllerTypeString(ControllerTypes.LinearPotentiometer) == controllerTypeString)
            {
                return ControllerTypes.LinearPotentiometer;
            }
            else if (GetControllerTypeString(ControllerTypes.Button) == controllerTypeString)
            {
                return ControllerTypes.Button;
            }
            else if (GetControllerTypeString(ControllerTypes.RotaryEncoder) == controllerTypeString)
            {
                return ControllerTypes.RotaryEncoder;
            }
            else
            {
                return ControllerTypes.InvalidEntry;
            }
        }

        public override string ToString()
        {
            return $"Device={MidiDevice}, Channel={Channel}, Controller={Controller}, " +
                   $"Controller Type={GetControllerTypeString(ControllerType)}, " +
                   $"Min Value={MinValue}, Max Value={MaxValue}, Scaling Value={ScalingValue}";
        }
    }
}