using EarTrumpet.DataModel.Hardware;

namespace EarTrumpet.DataModel.MIDI
{
    public enum ControllerTypes
    {
        LINEAR_POTENTIOMETER = 0,
        BUTTON = 1,
        ROTARY_ENCODER = 2,
        INVALID_ENTRY = 3
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
                case ControllerTypes.LINEAR_POTENTIOMETER:
                    return Properties.Resources.LinearPotentiometerText;
                case ControllerTypes.BUTTON:
                    return Properties.Resources.ButtonText;
                case ControllerTypes.ROTARY_ENCODER:
                    return Properties.Resources.RotaryEncoderText;
                default:
                    return "";
            }
        }

        public static ControllerTypes GetControllerType(string controllerTypeString)
        {
            if (GetControllerTypeString(ControllerTypes.LINEAR_POTENTIOMETER) == controllerTypeString)
            {
                return ControllerTypes.LINEAR_POTENTIOMETER;
            }
            else if (GetControllerTypeString(ControllerTypes.BUTTON) == controllerTypeString)
            {
                return ControllerTypes.BUTTON;
            }
            else if (GetControllerTypeString(ControllerTypes.ROTARY_ENCODER) == controllerTypeString)
            {
                return ControllerTypes.ROTARY_ENCODER;
            }
            else
            {
                return ControllerTypes.INVALID_ENTRY;
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