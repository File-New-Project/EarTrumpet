using System;
using System.Windows.Media.Effects;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace EarTrumpet.DataModel.MIDI
{
    public class MidiInDevice
    {
        private readonly DeviceInformation _device;

        public string Name => _device.Name;

        public string Id => _device.Id;

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