using System;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace EarTrumpet.DataModel.MIDI
{
    public class MidiInDevice
    {
        public string Name => _device.Name;
        public string Id => _device.Id;
        
        private readonly DeviceInformation _device;

        public MidiInDevice(DeviceInformation device)
        {
            _device = device;
        }

        public void AddControlChangeCallback(Action<MidiControlChangeMessage> callback, byte channel = 255,
            byte controller = 255)
        {
            MidiIn.AddControlChangeCallback(Id, callback, channel, controller);
        }
    }
}