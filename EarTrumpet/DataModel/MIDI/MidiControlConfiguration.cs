namespace EarTrumpet.DataModel.MIDI
{
    public enum ControllerTypes
    {
        LINEAR_POTENTIOMETER = 0,
        BUTTON = 1,
        ROTARY_ENCODER = 2,
        INVALID_ENTRY = 3
    }

    public class MidiControlConfiguration
    {
        public byte Channel { get; set; }
        public byte Controller { get; set; }
        public ControllerTypes ControllerType { get; set; }
        public byte MinValue { get; set; }
        public byte MaxValue { get; set; }
        public float ScalingValue { get; set; }

        // Constructor
        public MidiControlConfiguration(byte channel, byte controller, ControllerTypes controllerType, byte minValue, byte maxValue, float scalingValue)
        {
            Channel = channel;
            Controller = controller;
            ControllerType = controllerType;
            MinValue = minValue;
            MaxValue = maxValue;
            ScalingValue = scalingValue;
        }

        // Default constructor required for serialization.
        public MidiControlConfiguration()
        {
            
        }
        public static string GetControllerTypeString(ControllerTypes controllerType)
        {
            // TODO: Use localization.
            switch (controllerType)
            {
                case ControllerTypes.LINEAR_POTENTIOMETER:
                    return "Linear Potentiometer";
                case ControllerTypes.BUTTON:
                    return "Button";
                case ControllerTypes.ROTARY_ENCODER:
                    return "Rotary Encoder";
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
    }
}